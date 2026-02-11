using System.Net.Http.Json;
using System.Security.Claims;
using HotelRoomBookingAPI.Models.Web;
using HotelRoomBookingAPI.Models.Web.DTOs;
using HotelRoomBookingAPI.Models.Web.ViewModels;
using HotelRoomBookingAPI.Services.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelRoomBookingAPI.Controllers.Web;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class ReservationsController : Controller
{
    private readonly ApiService _apiService;
    private readonly IBookingsService _bookingsService;

    public ReservationsController(ApiService apiService, IBookingsService bookingsService)
    {
        _apiService = apiService;
        _bookingsService = bookingsService;
    }

    // GET: Bookings/Create?roomId=5&date=2026-01-21&checkOutDate=2026-01-23&from=09:00&to=18:00
    [HttpGet]
    public async Task<IActionResult> Create(int roomId, string? date, string? checkOutDate, string? from, string? to)
    {
        if (roomId <= 0)
        {
             return BadRequest("Invalid room ID.");
        }

        var room = await _apiService.GetAsync<Room>($"api/rooms/{roomId}");
        if (room == null) return NotFound();

        var booking = new Booking
        {
            RoomId = roomId,
            Room = room
        };
        
        // If date parameters provided (from availability search)
        if (!string.IsNullOrEmpty(date))
        {
            if (DateTime.TryParse(date, out DateTime parsedCheckInDate))
            {
                booking.CheckInDate = parsedCheckInDate.Date;
                
                // Check if this is a multi-day booking (check-out date is DIFFERENT from check-in date)
                if (!string.IsNullOrEmpty(checkOutDate) && 
                    DateTime.TryParse(checkOutDate, out DateTime parsedCheckOutDate) &&
                    parsedCheckOutDate.Date != parsedCheckInDate.Date)
                {
                    // Multi-day booking - use times if provided, otherwise use full day
                    booking.CheckOutDate = parsedCheckOutDate.Date;
                    
                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to) &&
                        TimeSpan.TryParse(from, out TimeSpan fromTime) && TimeSpan.TryParse(to, out TimeSpan toTime))
                    {
                        // Use the selected times for multi-day booking
                        booking.BookingStartDateTime = parsedCheckInDate.Date + fromTime;
                        booking.BookingEndDateTime = parsedCheckOutDate.Date + toTime;
                    }
                    else
                    {
                        // No times provided - use full days
                        booking.BookingStartDateTime = parsedCheckInDate.Date;
                        booking.BookingEndDateTime = parsedCheckOutDate.Date.AddDays(1).AddSeconds(-1);
                    }
                }
                else if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                {
                    // Same-day booking with times
                    if (TimeSpan.TryParse(from, out TimeSpan fromTime) && TimeSpan.TryParse(to, out TimeSpan toTime))
                    {
                        booking.BookingStartDateTime = parsedCheckInDate.Date + fromTime;
                        booking.BookingEndDateTime = parsedCheckInDate.Date + toTime;
                        booking.CheckOutDate = parsedCheckInDate.Date;
                    }
                }
            }
        }

        return View(booking);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Booking booking)
    {
        // Set UserId from logged in user
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            booking.UserId = userId;
        }
        else
        {
             // Should not happen if authorized
             return Unauthorized();
        }
        
        // Ensure BookingStartDateTime and BookingEndDateTime are set
        // If they're not set, use CheckInDate/CheckOutDate as fallback
        if (!booking.BookingStartDateTime.HasValue && booking.CheckInDate != default)
        {
            booking.BookingStartDateTime = booking.CheckInDate;
        }
        if (!booking.BookingEndDateTime.HasValue && booking.CheckOutDate != default)
        {
            booking.BookingEndDateTime = booking.CheckOutDate;
        }
        
        // CONFLICT CHECK: Re-verify room availability before confirming booking
        var allBookings = await _apiService.GetAsync<List<Booking>>("api/bookings");
        if (allBookings != null && allBookings.Any())
        {
            // Check for conflicts with existing bookings for this room
            var conflictingBooking = allBookings.FirstOrDefault(b =>
                b.RoomId == booking.RoomId &&
                b.BookingStatus != "Cancelled" &&
                b.BookingStartDateTime.HasValue &&
                b.BookingEndDateTime.HasValue &&
                booking.BookingStartDateTime.HasValue &&
                booking.BookingEndDateTime.HasValue &&
                (
                    // New booking starts during an existing booking
                    (booking.BookingStartDateTime.Value >= b.BookingStartDateTime.Value && 
                     booking.BookingStartDateTime.Value < b.BookingEndDateTime.Value) ||
                    // New booking ends during an existing booking
                    (booking.BookingEndDateTime.Value > b.BookingStartDateTime.Value && 
                     booking.BookingEndDateTime.Value <= b.BookingEndDateTime.Value) ||
                    // New booking completely encompasses an existing booking
                    (booking.BookingStartDateTime.Value <= b.BookingStartDateTime.Value && 
                     booking.BookingEndDateTime.Value >= b.BookingEndDateTime.Value)
                )
            );

            if (conflictingBooking != null)
            {
                // Room is no longer available - show error
                var room = await _apiService.GetAsync<Room>($"api/rooms/{booking.RoomId}");
                booking.Room = room;
                
                ModelState.AddModelError("", $"Sorry, this room is no longer available for the selected time slot. It was just booked by another user. Please search for available rooms again.");
                return View(booking);
            }
        }
        
        booking.BookingStatus = "Booked";

        var response = await _apiService.PostAsync("api/bookings", booking);
        if (response.IsSuccessStatusCode)
        {
            var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
            TempData["SuccessMessage"] = "Booking confirmed! Please add occupant details.";
            return RedirectToAction("ConfirmBooking", new { id = createdBooking.BookingId });
        }

        // If failure, fetch room details again to show in view
        var roomForError = await _apiService.GetAsync<Room>($"api/rooms/{booking.RoomId}");
        booking.Room = roomForError;
        
        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError("", !string.IsNullOrEmpty(error) ? error : "Booking failed. Room might be unavailable.");
        return View(booking);
    }

    [HttpPost]
    public async Task<IActionResult> QuickBook(int roomId, string? date, string? checkOutDate, string? from, string? to)
    {
        // 1. Validate Input
        if (roomId <= 0) return BadRequest("Invalid Room ID");
        
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        // 2. Build Booking Object (Reuse logic)
        var booking = new Booking
        {
            RoomId = roomId,
            UserId = userId,
            BookingStatus = "Booked"
        };

        if (DateTime.TryParse(date, out DateTime parsedCheckInDate))
        {
            booking.CheckInDate = parsedCheckInDate.Date;
            
            // Time Handling
            if (!string.IsNullOrEmpty(checkOutDate) && 
                DateTime.TryParse(checkOutDate, out DateTime parsedCheckOutDate) &&
                parsedCheckOutDate.Date != parsedCheckInDate.Date)
            {
                booking.CheckOutDate = parsedCheckOutDate.Date;
                 if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to) &&
                    TimeSpan.TryParse(from, out TimeSpan fromTime) && TimeSpan.TryParse(to, out TimeSpan toTime))
                {
                    booking.BookingStartDateTime = parsedCheckInDate.Date + fromTime;
                    booking.BookingEndDateTime = parsedCheckOutDate.Date + toTime;
                }
                else
                {
                    booking.BookingStartDateTime = parsedCheckInDate.Date;
                    booking.BookingEndDateTime = parsedCheckOutDate.Date.AddDays(1).AddSeconds(-1);
                }
            }
            else // Same day or default
            {
                booking.CheckOutDate = parsedCheckInDate.Date;
                if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to) &&
                    TimeSpan.TryParse(from, out TimeSpan fromTime) && TimeSpan.TryParse(to, out TimeSpan toTime))
                {
                    booking.BookingStartDateTime = parsedCheckInDate.Date + fromTime;
                    booking.BookingEndDateTime = parsedCheckInDate.Date + toTime;
                }
                else
                {
                    // Fallback to full day? Or error? Assuming full day if no time.
                     booking.BookingStartDateTime = parsedCheckInDate.Date;
                     booking.BookingEndDateTime = parsedCheckInDate.Date.AddDays(1).AddSeconds(-1);
                }
            }
        }
        else
        {
            return BadRequest("Invalid Date");
        }

        // 3. Post to API
        var response = await _apiService.PostAsync("api/bookings", booking);
        
        if (response.IsSuccessStatusCode)
        {
            var createdBooking = await response.Content.ReadFromJsonAsync<Booking>();
            TempData["SuccessMessage"] = "Room booked successfully! Please add occupants.";
            // Redirect DIRECTLY to Occupant Page
            return RedirectToAction("ConfirmBooking", new { id = createdBooking.BookingId });
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = "Booking Failed: " + error;
            // Redirect back to search if failed
            return RedirectToAction("Available", "Rooms"); // Or relevant page
        }
    }

        public async Task<IActionResult> MyBookings(string? status = "Booked", string? date = null)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        List<Booking>? bookings;

        // If Admin, show ALL bookings
        if (User.IsInRole("Admin"))
        {
             bookings = await _apiService.GetAsync<List<Booking>>("api/bookings");
        }
        else
        {
            // If User, show only THEIR bookings
            bookings = await _apiService.GetAsync<List<Booking>>($"api/bookings/user/{userId}");
        }

        ViewData["CurrentStatus"] = status;
        ViewData["CurrentDate"] = date ?? DateTime.Today.ToString("yyyy-MM-dd");

        return View(bookings ?? new List<Booking>());
    }

    // POST: Bookings/Cancel/5
    [HttpPost]
    public async Task<IActionResult> CancelBooking(int id, string? remarks, string? status, string? date)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        // Get the booking to verify ownership
        var booking = await _apiService.GetAsync<Booking>($"api/bookings/{id}");
        
        if (booking == null)
        {
            TempData["ErrorMessage"] = "Booking not found.";
            return RedirectToAction("MyBookings", new { status, date });
        }

        // Verify the booking belongs to the current user
        if (booking.UserId != userId)
        {
            TempData["ErrorMessage"] = "You can only cancel your own bookings.";
            return RedirectToAction("MyBookings", new { status, date });
        }

        // Check if booking is already cancelled
        if (booking.BookingStatus == "Cancelled")
        {
            TempData["ErrorMessage"] = "This booking is already cancelled.";
            return RedirectToAction("MyBookings", new { status, date });
        }

        // Use the dedicated status update endpoint with JSON object
        var updateDto = new UpdateBookingStatusDto 
        { 
            Status = "Cancelled", 
            Remarks = remarks 
        };
        
        var options = new System.Text.Json.JsonSerializerOptions 
        { 
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase 
        };

        var statusJson = System.Text.Json.JsonSerializer.Serialize(updateDto, options);
        var content = new StringContent(statusJson, System.Text.Encoding.UTF8, "application/json");
        var response = await _apiService.GetHttpClient().PutAsync($"api/bookings/{id}/status", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Booking cancelled successfully!";
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Failed to cancel booking: {error}";
        }

        return RedirectToAction("MyBookings", new { status, date });
    }

    // GET: Bookings/ConfirmBooking/5
    [HttpGet]
    public async Task<IActionResult> ConfirmBooking(int id, string? fromStatus, string? fromDate)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        var booking = await _bookingsService.GetBookingAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        // Verify ownership
        if (booking.UserId != userId)
        {
            return Forbid();
        }

        // Load existing occupants
        var occupants = await _bookingsService.GetOccupantsAsync(id);
        var clientKinds = await _bookingsService.GetClientKindsAsync();

        var viewModel = new ConfirmBookingViewModel
        {
            Booking = booking,
            Occupants = occupants,
            ClientKinds = clientKinds,
            NewOccupant = new AddOccupantDto { BookingId = id }
        };

        ViewData["FromStatus"] = fromStatus;
        ViewData["FromDate"] = fromDate;

        return View(viewModel);
    }

    // POST: Bookings/AddOccupant
    [HttpPost]
    public async Task<IActionResult> AddOccupant(ConfirmBookingViewModel viewModel, string? fromStatus, string? fromDate)
    {
        if (!ModelState.IsValid)
        {
            // Reload data
            var booking = await _bookingsService.GetBookingAsync(viewModel.NewOccupant.BookingId);
            var occupants = await _bookingsService.GetOccupantsAsync(viewModel.NewOccupant.BookingId);
            
            viewModel.Booking = booking!;
            viewModel.Occupants = occupants;
            
            ViewData["FromStatus"] = fromStatus;
            ViewData["FromDate"] = fromDate;
            
            return View("ConfirmBooking", viewModel);
        }

        var success = await _bookingsService.AddOccupantAsync(viewModel.NewOccupant);
        if (success)
        {
            TempData["SuccessMessage"] = "Occupant added successfully.";
        }
        else
        {
            // Could be duplicate Aadhaar or other error
             TempData["ErrorMessage"] = "Failed to add occupant. Duplicate Aadhaar or system error.";
        }

        return RedirectToAction("ConfirmBooking", new { id = viewModel.NewOccupant.BookingId, fromStatus, fromDate });
    }

    // POST: Bookings/RemoveOccupant
    [HttpPost]
    public async Task<IActionResult> RemoveOccupant(int id, int bookingId, string? fromStatus, string? fromDate)
    {
        var success = await _bookingsService.RemoveOccupantAsync(id);
        if (success)
        {
             TempData["SuccessMessage"] = "Occupant removed successfully.";
        }
        else
        {
             TempData["ErrorMessage"] = "Failed to remove occupant.";
        }

        return RedirectToAction("ConfirmBooking", new { id = bookingId, fromStatus, fromDate });
    }

    // POST: Bookings/UpdateClientType
    [HttpPost]
    public async Task<IActionResult> UpdateClientType(int bookingId, int clientKindId, string? fromStatus, string? fromDate)
    {
        var booking = await _bookingsService.GetBookingAsync(bookingId);
        if (booking == null)
        {
            return NotFound();
        }

        // Update Client Kind
        booking.ClientKindId = clientKindId;
        
        // Sanitize the object before sending to API to prevent validation errors on nested objects
        booking.Room = null;
        booking.User = null;
        booking.ClientKind = null;
        
        var result = await _bookingsService.UpdateBookingAsync(booking);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Client Type updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Failed to update Client Type. {result.Message}";
        }

        return RedirectToAction("ConfirmBooking", new { id = bookingId, fromStatus, fromDate });
    }

    [HttpGet]
    public async Task<IActionResult> GetOccupants(int bookingId)
    {
        var occupants = await _bookingsService.GetOccupantsAsync(bookingId);
        return Json(occupants);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOccupantStatus([FromBody] HotelRoomBookingAPI.Models.Web.DTOs.UpdateOccupantStatusDto request)
    {
        // Call API to update status
        // Using HttpClient directly or via Service if supported. 
        // Assuming ApiService or BookingsService needs update, or just use raw HttpClient here for speed if service is not flexible.
        // Let's use ApiService via reflection or just access HttpClient if exposed?
        // _apiService.GetHttpClient() is available.
        
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), System.Text.Encoding.UTF8, "application/json");
        var response = await _apiService.GetHttpClient().PutAsync($"api/bookings/occupants/{request.OccupantId}/status", content);
        
        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }
        return BadRequest();
    }
}
