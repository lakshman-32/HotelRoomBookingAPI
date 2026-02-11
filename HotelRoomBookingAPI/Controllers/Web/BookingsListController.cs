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
public class BookingsListController : Controller
{
    private readonly ApiService _apiService;

    public BookingsListController(ApiService apiService)
    {
        _apiService = apiService;
    }

    // GET: BookingsList?filter=todayCheckins&date=yyyy-MM-dd&search=...
    public async Task<IActionResult> Index(string? filter, DateTime? date, string? search)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out int userId))
        {
            return Unauthorized();
        }

        // Check for Admin Role (Name "Admin" or RoleId "1")
        var isAdmin = User.IsInRole("Admin") || User.HasClaim("RoleId", "1");

        List<Booking>? bookings;
        if (isAdmin)
        {
             // Admin sees ALL bookings
             bookings = await _apiService.GetAsync<List<Booking>>("api/bookings");
        }
        else
        {
             // Regular Users see ONLY their bookings
             bookings = await _apiService.GetAsync<List<Booking>>($"api/bookings/user/{userId}");
        }

        if (bookings == null)
        {
            bookings = new List<Booking>();
        }

        // Apply Search Filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower().Trim();
            bookings = bookings.Where(b => 
                (b.Occupants != null && b.Occupants.Any(o => o.FullName != null && o.FullName.ToLower().Contains(search))) ||
                (b.Room?.Building?.Building_Name != null && b.Room.Building.Building_Name.ToLower().Contains(search)) ||
                (b.Room?.RoomNumber != null && b.Room.RoomNumber.ToLower().Contains(search)) ||
                b.BookingId.ToString().Contains(search)
            ).ToList();
        }

        var targetDate = date?.Date ?? DateTime.Today;
        var isToday = targetDate == DateTime.Today;
        List<Booking> filteredBookings;
        string pageTitle;

        switch (filter?.ToLower())
        {
            case "todaycheckins":
                filteredBookings = bookings.Where(b =>
                    b.CheckInDate.Date == targetDate &&
                    b.BookingStatus != "Cancelled"
                ).ToList();
                pageTitle = isToday ? "Today's Check-ins" : $"Check-ins on {targetDate:MMM dd, yyyy}";
                break;

            case "todaycheckouts":
                filteredBookings = bookings.Where(b =>
                    b.CheckOutDate.Date == targetDate &&
                    b.BookingStatus != "Cancelled"
                ).ToList();
                pageTitle = isToday ? "Today's Check-outs" : $"Check-outs on {targetDate:MMM dd, yyyy}";
                break;

            case "pending":
                // Pending: Bookings where check-in date is today (or selected date) and NOT cancelled, and occupants NOT checked in.
                filteredBookings = bookings.Where(b =>
                    b.BookingStatus == "Booked" &&
                    b.CheckInDate.Date == targetDate &&
                    (b.Occupants == null || b.Occupants.Count == 0 || !b.Occupants.Any(o => o.IsCheckedIn))
                ).ToList();
                pageTitle = isToday ? "Pending Bookings" : $"Pending Bookings ({targetDate:MMM dd, yyyy})";
                break;

            case "cancelled":
                filteredBookings = bookings.Where(b =>
                    b.BookingStatus == "Cancelled" &&
                    b.CheckInDate.Date == targetDate
                ).ToList();
                pageTitle = isToday ? "Cancelled Bookings" : $"Cancelled Bookings ({targetDate:MMM dd, yyyy})";
                break;

            case "reserved": // Handling potential legacy name
            case "booked":
                filteredBookings = bookings.Where(b =>
                    (b.BookingStatus == "Booked" || b.BookingStatus == "CheckedIn") &&
                    b.CheckInDate.Date <= targetDate.Date &&
                    b.CheckOutDate.Date >= targetDate.Date
                ).ToList();
                pageTitle = isToday ? "Booked Rooms" : $"Booked Rooms ({targetDate:MMM dd, yyyy})";
                break;

            default:
                filteredBookings = bookings;
                pageTitle = "All Bookings";
                break;
        }

        ViewBag.PageTitle = pageTitle;
        ViewBag.Filter = filter;
        ViewBag.Search = search; // Pass search term back to view
        ViewBag.SelectedDate = targetDate;
        
        return View(filteredBookings);
    }
}
