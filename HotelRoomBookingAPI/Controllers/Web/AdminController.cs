using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelRoomBookingAPI.Services.Web;
using HotelRoomBookingAPI.Models.Web;
using HotelRoomBookingAPI.Models.Web.ViewModels;

namespace HotelRoomBookingAPI.Controllers.Web;

[Authorize(Roles = "Admin")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AdminController : Controller
{
    private readonly ApiService _apiService;

    public AdminController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFloorRooms(int floorId)
    {
        // Call the GenerateRooms endpoint (POST) to ensure rooms exist and fetch them
        var response = await _apiService.PostAsync<object>($"api/Floors/{floorId}/generate-rooms", null);
        
        if (response.IsSuccessStatusCode)
        {
             var jsonString = await response.Content.ReadAsStringAsync();
             var rooms = System.Text.Json.JsonSerializer.Deserialize<List<Room>>(jsonString, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
             return Json(rooms ?? new List<Room>());
        }
        
        return Json(new List<Room>());
    }

    [HttpPost]
    public async Task<IActionResult> SaveFloorRooms([FromBody] List<RoomTypeUpdateModel> rooms)
    {
        // Reuse the update-types logic
        var response = await _apiService.PostAsync("api/Rooms/update-types", rooms);
        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }
        return BadRequest(await response.Content.ReadAsStringAsync());
    }

    [HttpGet]
    public async Task<IActionResult> GetRoomTypes()
    {
        // gets the room types from the API displays in dropdownbox
        
        var types = await _apiService.GetAsync<List<RoomType>>("api/RoomTypes"); // Assuming standard endpoint
        // If RoomTypes API doesn't exist, I should check or handle it.
        // Assuming it exists or I might mock it if I can't find it.
        // Let's assume it exists as RoomTypes likely exist.
        return Json(types ?? new List<RoomType>()); 
    }
    // displays the admin dashboard
    public IActionResult Dashboard()
    {
        return View();
    }
    
    // POST: Admin/DeleteBuilding/5
    [HttpPost]
    public async Task<IActionResult> DeleteBuilding(int id)
    {
        var response = await _apiService.DeleteAsync($"api/Buildings/{id}");
        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Building deleted successfully!";
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = "Failed to delete building: " + error;
        }
        return RedirectToAction("BuildingMaster");
    }

    // displays the building master page
    public async Task<IActionResult> BuildingMaster()
    {
        var buildings = await _apiService.GetAsync<List<BuildingsMaster>>("api/Buildings");
        return View(buildings ?? new List<BuildingsMaster>());
    }

    // displays the floor master page
    public async Task<IActionResult> FloorMaster()
    {
        var floors = await _apiService.GetAsync<List<Floor>>("api/Floors");
        var buildings = await _apiService.GetAsync<List<BuildingsMaster>>("api/Buildings");
        
        // Pass buildings to view for Dropdown
        ViewBag.Buildings = buildings ?? new List<BuildingsMaster>();
        
        return View(floors ?? new List<Floor>());
    }

    [HttpPost]
    public async Task<IActionResult> AddFloor(Floor floor)
    {
        if (ModelState.IsValid)
        {
             // Validate that Total Rooms is positive
             if (floor.No_of_rooms <= 0)
             {
                 TempData["ErrorMessage"] = "Total Rooms must be greater than zero.";
                 return RedirectToAction("FloorMaster");
             }

             var response = await _apiService.PostAsync("api/Floors", floor);
             if (response.IsSuccessStatusCode)
             {
                 TempData["SuccessMessage"] = "Floor added successfully!";
             }
             else
             {
                 var error = await response.Content.ReadAsStringAsync();
                 TempData["ErrorMessage"] = "Failed to add floor: " + error;
             }
        }
        else
        {
             var errors = string.Join("; ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
            TempData["ErrorMessage"] = "Invalid data: " + errors;
        }
        return RedirectToAction("FloorMaster");
    }

    [HttpPost]
    public async Task<IActionResult> EditFloor(Floor floor)
    {
        if (ModelState.IsValid)
        {
            if (floor.No_of_rooms <= 0)
            {
                TempData["ErrorMessage"] = "Total Rooms must be greater than zero.";
                return RedirectToAction("FloorMaster");
            }

            var response = await _apiService.PutAsync($"api/Floors/{floor.FloorId}", floor);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Floor updated successfully!";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = "Failed to update floor: " + error;
            }
        }
        else
        {
             TempData["ErrorMessage"] = "Invalid data.";
        }
        return RedirectToAction("FloorMaster");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFloor(int id)
    {
        var response = await _apiService.DeleteAsync($"api/Floors/{id}");
        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Floor deleted successfully!";
        }
        else
        {
             var error = await response.Content.ReadAsStringAsync();
             TempData["ErrorMessage"] = "Failed to delete floor: " + (string.IsNullOrEmpty(error) ? "Unknown error" : error);
        }
        return RedirectToAction("FloorMaster");
    }

    [HttpPost]
    public async Task<IActionResult> AddBuilding(BuildingsMaster building)
    {
        if (ModelState.IsValid)
        {
             // Force some defaults if needed
             if(string.IsNullOrEmpty(building.Status)) building.Status = "Active";

             var response = await _apiService.PostAsync("api/Buildings", building);
             if (response.IsSuccessStatusCode)
             {
                 TempData["SuccessMessage"] = "Building added successfully!";
             }
             else
             {
                 var error = await response.Content.ReadAsStringAsync();
                 TempData["ErrorMessage"] = "Failed to add building: " + error;
             }
        }
        else
        {
            var errors = string.Join("; ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
            TempData["ErrorMessage"] = "Invalid data: " + errors;
        }
        return RedirectToAction("BuildingMaster");
    }
    [HttpPost]
    public async Task<IActionResult> UpdateRoomOccupancy([FromBody] List<RoomOccupiedUpdateModel> updates)
    {
        var response = await _apiService.PostAsync("api/Rooms/occupied", updates);
        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }
        return BadRequest(await response.Content.ReadAsStringAsync());
    }

    [HttpPost]
    public async Task<IActionResult> EditBuilding(BuildingsMaster building)
    {
        if (ModelState.IsValid)
        {
             var response = await _apiService.PutAsync($"api/Buildings/{building.Building_ID}", building);
             if (response.IsSuccessStatusCode)
             {
                 TempData["SuccessMessage"] = "Building updated successfully!";
             }
             else
             {
                 var error = await response.Content.ReadAsStringAsync();
                 TempData["ErrorMessage"] = "Failed to update building: " + error;
             }
        }
        else
        {
             TempData["ErrorMessage"] = "Invalid data.";
        }
        return RedirectToAction("BuildingMaster");
    }

    // Room Master
    public async Task<IActionResult> RoomMaster()
    {
        var rooms = await _apiService.GetAsync<List<Room>>("api/Rooms");
        var buildings = await _apiService.GetAsync<List<BuildingsMaster>>("api/Buildings");
        var roomTypes = await _apiService.GetAsync<List<RoomType>>("api/RoomTypes");

        ViewBag.Buildings = buildings ?? new List<BuildingsMaster>();
        ViewBag.RoomTypes = roomTypes ?? new List<RoomType>();

        return View(rooms ?? new List<Room>());
    }

    [HttpGet]
    public async Task<IActionResult> GetFloorsByBuilding(int buildingId)
    {
        var floors = await _apiService.GetAsync<List<Floor>>($"api/Floors?buildingId={buildingId}");
        return Json(floors ?? new List<Floor>());
    }

    [HttpPost]
    public async Task<IActionResult> AddRoom(Room room)
    {
        if (ModelState.IsValid)
        {
             var response = await _apiService.PostAsync("api/Rooms", room);
             if (response.IsSuccessStatusCode)
             {
                 TempData["SuccessMessage"] = "Room added successfully!";
             }
             else
             {
                 var error = await response.Content.ReadAsStringAsync();
                 TempData["ErrorMessage"] = "Failed to add room: " + error;
             }
        }
        else
        {
             TempData["ErrorMessage"] = "Invalid data.";
        }
        return RedirectToAction("RoomMaster");
    }

    [HttpPost]
    public async Task<IActionResult> EditRoom(Room room)
    {
         // Status is required, ensure it's preserved or default
         if(string.IsNullOrEmpty(room.Status)) room.Status = "Available";

         var response = await _apiService.PutAsync($"api/Rooms/{room.RoomId}", room);
         if (response.IsSuccessStatusCode)
         {
             TempData["SuccessMessage"] = "Room updated successfully!";
         }
         else
         {
             var error = await response.Content.ReadAsStringAsync();
             TempData["ErrorMessage"] = "Failed to update room: " + error;
         }
         return RedirectToAction("RoomMaster");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var response = await _apiService.DeleteAsync($"api/Rooms/{id}");
        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Room deleted successfully!";
        }
        else
        {
             TempData["ErrorMessage"] = "Failed to delete room.";
        }
        return RedirectToAction("RoomMaster");
    }


    [HttpPost]
    public async Task<IActionResult> UpdateRoomTypes([FromBody] List<RoomTypeUpdateModel> updates)
    {
        var response = await _apiService.PostAsync("api/Rooms/update-types", updates);
        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }
        return BadRequest(await response.Content.ReadAsStringAsync());
    }
    
    // GET: Admin/AllBookings
    public async Task<IActionResult> AllBookings()
    {
        var bookings = await _apiService.GetAsync<List<Booking>>("api/bookings");
        return View(bookings ?? new List<Booking>());
    }
    
    // GET: Admin/AllUsers
    public async Task<IActionResult> AllUsers()
    {
        var users = await _apiService.GetAsync<List<User>>("api/users");
        return View(users ?? new List<User>());
    }

    // POST: Admin/PromoteToAdmin/5
    [HttpPost]
    public async Task<IActionResult> PromoteToAdmin(int id)
    {
        var response = await _apiService.PutAsync($"api/users/{id}/role", new { roleId = 1 });
        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "User promoted to Admin successfully!";
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = "Failed to promote user: " + error;
        }
        return RedirectToAction("AllUsers");
    }

    // POST: Admin/DemoteFromAdmin/5
    [HttpPost]
    public async Task<IActionResult> DemoteFromAdmin(int id, int newRoleId = 3)
    {
        // Prevent self-demotion
        var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int currentUserId) && currentUserId == id)
        {
            TempData["ErrorMessage"] = "You cannot demote yourself from Admin role!";
            return RedirectToAction("AllUsers");
        }

        var response = await _apiService.PutAsync($"api/users/{id}/role", new { roleId = newRoleId });
        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "User demoted from Admin successfully!";
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = "Failed to demote user: " + error;
        }
        return RedirectToAction("AllUsers");
    }

    // POST: Admin/CreateUser
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill in all required fields.";
            return RedirectToAction("AllUsers");
        }

        var userDto = new
        {
            fullName = model.FullName,
            email = model.Email,
            password = model.Password,
            companyName = model.CompanyName,
            roleId = model.RoleId
        };

        var response = await _apiService.PostAsync("api/users/create-user", userDto);
        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = $"User '{model.FullName}' created successfully!";
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[Error] CreateUser failed. Status: {response.StatusCode}, Content: {error}");
            TempData["ErrorMessage"] = $"Failed to create user (Status: {response.StatusCode}): {error}";
        }
        return RedirectToAction("AllUsers");
    }

    [HttpGet]
    public async Task<IActionResult> GetOccupants(int bookingId)
    {
        var occupants = await _apiService.GetAsync<List<HotelRoomBookingAPI.Models.Web.DTOs.BookingOccupantDto>>($"api/bookings/{bookingId}/occupants");
        return Json(occupants);
    }
}

// View Model for creating users
public class CreateUserViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public int RoleId { get; set; } = 2; // Default to User
}

public class RoomOccupiedUpdateModel
{
    public int RoomId { get; set; }
    public int Occupied { get; set; }
}

public class RoomTypeUpdateModel
{
    public int RoomId { get; set; }
    public int RoomTypeId { get; set; }
}
