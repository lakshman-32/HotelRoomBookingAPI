using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelRoomBookingAPI.Models.Web.ViewModels;
using HotelRoomBookingAPI.Services.Web;
using System.Security.Claims;

namespace HotelRoomBookingAPI.Controllers.Web;

[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class DashboardController : Controller
{
    private readonly ApiService _apiService;

    public DashboardController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim);
            var isAdmin = User.IsInRole("Admin");
            var targetDate = date ?? DateTime.Today;
            ViewBag.SelectedDate = targetDate; // Use DateTime object

            // Fetch dashboard stats from API
            var endpoint = isAdmin ? "api/dashboard/stats" : $"api/dashboard/user-stats/{userId}";
            
            // Format for API call
            endpoint += $"?date={targetDate:yyyy-MM-dd}";
            
            var stats = await _apiService.GetAsync<DashboardViewModel>(endpoint);

            if (stats == null)
            {
                stats = new DashboardViewModel();
            }

            return View(stats);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Failed to load dashboard statistics.";
            return View(new DashboardViewModel());
        }
    }
}
