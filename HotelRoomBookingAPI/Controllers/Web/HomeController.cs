using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelRoomBookingAPI.Models.Web;
using HotelRoomBookingAPI.Models.Web.ViewModels;

namespace HotelRoomBookingAPI.Controllers.Web;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
