using Microsoft.AspNetCore.Mvc;
using HotelRoomBookingAPI.Models.Web.ViewModels.Reports;
using HotelRoomBookingAPI.Services.Web;

namespace HotelRoomBookingAPI.Controllers.Web;

[ApiExplorerSettings(IgnoreApi = true)]
public class ReportsController : Controller
{
    private readonly ApiService _apiService;

    public ReportsController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        // Default: Today to Today + 30 days
        var start = fromDate ?? DateTime.Today;
        var end = toDate ?? DateTime.Today.AddDays(30);

        var viewModel = new ReportsIndexVM
        {
            FromDate = start,
            ToDate = end,
            Reports = new List<ReportRowVM>()
        };

        try
        {
            // Call API
            var endpoint = $"api/Reports/meals?fromDate={start:yyyy-MM-dd}&toDate={end:yyyy-MM-dd}";
            var reports = await _apiService.GetAsync<List<ReportRowVM>>(endpoint);
            
            if (reports != null)
            {
                viewModel.Reports = reports;
            }
        }
        catch (Exception ex)
        {
            // Handle error (maybe log it or show a message)
            ModelState.AddModelError("", "Failed to load reports: " + ex.Message);
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetDetails(DateTime date, string? mealType)
    {
        try
        {
            var endpoint = $"api/Reports/meals/details?date={date:yyyy-MM-dd}";
            var detailsRaw = await _apiService.GetAsync<List<MealDetailVM>>(endpoint); // Note: returns list of Dtos which map to VM properties
            
            var details = detailsRaw ?? new List<MealDetailVM>();

            // Filter if mealType is specified
            if (!string.IsNullOrEmpty(mealType))
            {
                if (mealType.Equals("Breakfast", StringComparison.OrdinalIgnoreCase))
                {
                    details = details.Where(x => x.IsBreakfast).ToList();
                }
                else if (mealType.Equals("Lunch", StringComparison.OrdinalIgnoreCase))
                {
                    details = details.Where(x => x.IsLunch).ToList();
                }
                else if (mealType.Equals("Dinner", StringComparison.OrdinalIgnoreCase))
                {
                     details = details.Where(x => x.IsDinner).ToList();
                }
            }
            
            ViewData["MealType"] = mealType; // Pass to view for display context
            return PartialView("_MealDetails", details);
        }
        catch (Exception)
        {
            return BadRequest("Failed to load details");
        }
    }
    public async Task<IActionResult> Availability(DateTime? fromDate, DateTime? toDate)
    {
        var start = fromDate ?? DateTime.Today;
        var end = toDate ?? DateTime.Today.AddDays(30);

        var viewModel = new AvailabilityIndexVM
        {
            FromDate = start,
            ToDate = end
        };

        try
        {
            var endpoint = $"api/Reports/availability?fromDate={start:yyyy-MM-dd}&toDate={end:yyyy-MM-dd}";
            var reports = await _apiService.GetAsync<List<AvailabilityRowVM>>(endpoint);
            
            if (reports != null)
            {
                viewModel.Reports = reports;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Failed to load availability: " + ex.Message);
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailabilityDetails(DateTime date, string? status)
    {
        try
        {
            var endpoint = $"api/Reports/availability/details?date={date:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(status))
            {
                endpoint += $"&status={status}";
            }

            var details = await _apiService.GetAsync<List<AvailabilityDetailVM>>(endpoint);
            
            ViewData["Status"] = status; // Pass to view for title
            return PartialView("_AvailabilityDetails", details ?? new List<AvailabilityDetailVM>());
        }
        catch (Exception)
        {
            return BadRequest("Failed to load details");
        }
    }
}
