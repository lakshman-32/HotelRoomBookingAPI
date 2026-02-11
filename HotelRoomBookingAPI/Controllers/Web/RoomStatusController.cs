using Microsoft.AspNetCore.Mvc;
using HotelRoomBookingAPI.Models.Web;
using HotelRoomBookingAPI.Services.Web;

namespace HotelRoomBookingAPI.Controllers.Web;

[ApiExplorerSettings(IgnoreApi = true)]
public class RoomStatusController : Controller
{
    private readonly ApiService _apiService;

    public RoomStatusController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        // Default to today if no date selected
        if (!date.HasValue)
        {
            date = DateTime.Today;
        }

        string url = $"api/RoomStatus?date={date.Value:yyyy-MM-dd}";
        ViewBag.SelectedDate = date.Value.ToString("yyyy-MM-dd");
        
        var statuses = await _apiService.GetAsync<List<RoomStatus>>(url);
        return View(statuses ?? new List<RoomStatus>());
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] RoomStatus model)
    {
        if (model == null) return BadRequest("Invalid data");

        // We need to set CreatedAt if API doesn't handle it, usually API model has default.
        // But let's let API handle it. 
        // We just post to API.
        
        var response = await _apiService.PostAsync("api/RoomStatus", model);
        
        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }
        
        return BadRequest("Failed to save status");
    }
}
