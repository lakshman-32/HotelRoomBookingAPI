using HotelRoomBookingAPI.Models.Web;
using HotelRoomBookingAPI.Models.Web.ViewModels;
using HotelRoomBookingAPI.Services.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelRoomBookingAPI.Controllers.Web;

[ApiExplorerSettings(IgnoreApi = true)]
public class RoomsController : Controller
{
    private readonly ApiService _apiService;

    public RoomsController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _apiService.GetAsync<List<Room>>("api/rooms");
        return View(rooms ?? new List<Room>());
    }

    public async Task<IActionResult> Details(int id)
    {
        var room = await _apiService.GetAsync<Room>($"api/rooms/{id}");
        if (room == null) return NotFound();
        return View(room);
    }
    
    // GET: Rooms/Available
    // GET: Rooms/Available
    public async Task<IActionResult> Available(DateTime? checkInDate)
    {
        var viewModel = new AvailabilitySearchVM
        {
            Date = checkInDate ?? DateTime.Now,
            CheckOutDate = (checkInDate ?? DateTime.Now).AddDays(1),
            FromTime = DateTime.Now.AddMinutes(5).TimeOfDay,
            ToTime = DateTime.Now.AddMinutes(5).TimeOfDay
        };
        
        // Load buildings for dropdown
        var buildings = await _apiService.GetAsync<List<BuildingsMaster>>("api/Buildings");
        viewModel.Buildings = buildings ?? new List<BuildingsMaster>();
        viewModel.BuildingsSelectList = new SelectList(viewModel.Buildings, "Building_ID", "Building_Name");
        
        return View(viewModel);
    }

    // POST: Rooms/Available
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Available(AvailabilitySearchVM model)
    {
        // Reload buildings for dropdown
        var buildings = await _apiService.GetAsync<List<BuildingsMaster>>("api/Buildings");
        model.Buildings = buildings ?? new List<BuildingsMaster>();
        model.BuildingsSelectList = new SelectList(model.Buildings, "Building_ID", "Building_Name", model.BuildingId);
        
        // Load floors for the selected building
        if (model.BuildingId.HasValue)
        {
            var floors = await _apiService.GetAsync<List<Floor>>($"api/Floors?buildingId={model.BuildingId}");
            model.Floors = floors ?? new List<Floor>();
            model.FloorsSelectList = new SelectList(model.Floors, "FloorId", "FloorNumber", model.FloorId);
        }
        
        // Validate required fields
        if (!model.BuildingId.HasValue || !model.Date.HasValue)
        {
            ModelState.AddModelError("", "Building and Check-in Date are required.");
            return View(model);
        }
        
        // Determine if this is a same-day or multi-day booking
        DateTime selectedStart, selectedEnd;
        
        if (model.CheckOutDate.HasValue && model.CheckOutDate.Value.Date != model.Date.Value.Date)
        {
            // Multi-day booking: Use dates WITH specific times if provided
            selectedStart = model.Date.Value.Date;
            if (model.FromTime.HasValue)
            {
                selectedStart = selectedStart.Add(model.FromTime.Value);
            }

            selectedEnd = model.CheckOutDate.Value.Date;
            if (model.ToTime.HasValue)
            {
                selectedEnd = selectedEnd.Add(model.ToTime.Value);
            }
            else
            {
                selectedEnd = selectedEnd.AddDays(1).AddSeconds(-1); // End of check-out day if no time
            }
        }
        else
        {
            // Same-day booking: Require times
            if (!model.FromTime.HasValue || !model.ToTime.HasValue)
            {
                ModelState.AddModelError("", "For same-day bookings, From Time and To Time are required.");
                return View(model);
            }
            
            // Validate time range
            if (model.FromTime >= model.ToTime)
            {
                ModelState.AddModelError("", "From Time must be before To Time.");
                return View(model);
            }
            
            // Combine Date + Time to create DateTime ranges
            selectedStart = model.Date.Value.Date + model.FromTime.Value;
            selectedEnd = model.Date.Value.Date + model.ToTime.Value;
        }
        
        // Get all rooms for the selected building (and optionally floor)
        var allRooms = await _apiService.GetAsync<List<Room>>($"api/rooms?buildingId={model.BuildingId}");
        
        // Filter by floor if selected
        if (model.FloorId.HasValue && allRooms != null)
        {
            allRooms = allRooms.Where(r => r.FloorId == model.FloorId.Value).ToList();
        }
        
        if (allRooms == null || !allRooms.Any())
        {
            model.AvailableRoomsCount = 0;
            model.AvailableRooms = new List<AvailableRoomRow>();
            model.SearchPerformed = true;
            return View(model);
        }
        
        // Get all bookings for these rooms
        var allBookings = await _apiService.GetAsync<List<Booking>>("api/bookings");
        
        // Filter available rooms (no booking conflicts)
        var availableRooms = new List<AvailableRoomRow>();
        
        foreach (var room in allRooms)
        {
            // Check if this room has any conflicting bookings
            var hasConflict = allBookings?.Any(b =>
                b.RoomId == room.RoomId &&
                b.BookingStatus != "Cancelled" &&
                (
                    // Rule 1: Strict Occupancy (User Request)
                    // If a room is CheckedIn and ANY occupant is NOT checked out, it is BLOCKED.
                    // This prevents booking a room that is currently occupied, regardless of the requested dates.
                    (b.BookingStatus == "CheckedIn" && b.Occupants != null && b.Occupants.Any(o => !o.IsCheckedOut)) ||
                    
                    // Rule 2: Date Overlap
                    // Standard check for future/planned bookings
                    (
                        (b.BookingStartDateTime ?? b.CheckInDate) < selectedEnd &&
                        (b.BookingEndDateTime ?? b.CheckOutDate) > selectedStart
                    )
                )
            ) ?? false;
            
            if (!hasConflict)
            {
                availableRooms.Add(new AvailableRoomRow
                {
                    RoomId = room.RoomId,
                    RoomNumber = room.RoomNumber,
                    RoomTypeName = room.RoomType?.RoomTypeName ?? "N/A",
                    Capacity = room.RoomType?.Capacity ?? 0,
                    FloorNumber = room.Floor?.FloorNumber.ToString() ?? "N/A",
                    BuildingName = room.Building?.Building_Name ?? "N/A",
                    BuildingLocation = room.Building?.Building_Location ?? "N/A",
                    Status = room.Status ?? "Available"
                });
            }
        }
        
        model.AvailableRooms = availableRooms;
        model.AvailableRoomsCount = availableRooms.Count;
        model.SearchPerformed = true;
        
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetFloorsByBuilding(int buildingId)
    {
        var floors = await _apiService.GetAsync<List<Floor>>($"api/Floors?buildingId={buildingId}");
        return Json(floors ?? new List<Floor>());
    }
}
