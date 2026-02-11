using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelRoomBookingAPI.Controllers.Web;

[Authorize]
public class MealsController : Controller
{
    private readonly ApplicationDbContext _context;

    public MealsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? date)
    {
        var selectedDate = date?.Date ?? DateTime.Today;
        ViewBag.SelectedDate = selectedDate;

        // 1. Get all bookings relevant to this date
        // We need bookings where the date falls within CheckIn -> CheckOut range
        var bookings = await _context.Bookings
            .Include(b => b.Occupants)
            .ThenInclude(o => o.DailyMeals)
            .Where(b => b.CheckInDate <= selectedDate && b.CheckOutDate >= selectedDate) // CheckOutDate is usually exclusive for stay but inclusive for breakfast? Let's assume broad range first.
            // Actually, if CheckOut is Feb 5, they might have breakfast but not Lunch/Dinner. 
            // The DailyMeals table gives precise granularity.
            .ToListAsync();

        // Better approach: Query OccupantDailyMeals directly for granular accuracy
        var dailyMeals = await _context.OccupantDailyMeals
            .Include(dm => dm.BookingOccupant)
            .ThenInclude(bo => bo!.Booking)
            .Where(dm => dm.Date == selectedDate)
            .ToListAsync();

        // METRICS
        // 1. On Request - Specific ad-hoc requests
        // Note: We check Has... && Is...OnRequest just to be safe, though Is...OnRequest implies Has...
        var requestBreakfast = dailyMeals.Count(dm => dm.HasBreakfast && dm.IsBreakfastOnRequest && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");
        var requestLunch = dailyMeals.Count(dm => dm.HasLunch && dm.IsLunchOnRequest && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");
        var requestDinner = dailyMeals.Count(dm => dm.HasDinner && dm.IsDinnerOnRequest && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");

        // 2. Planned (Regular) - Booked meals that are NOT on request
        var plannedBreakfast = dailyMeals.Count(dm => dm.HasBreakfast && !dm.IsBreakfastOnRequest && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");
        var plannedLunch = dailyMeals.Count(dm => dm.HasLunch && !dm.IsLunchOnRequest && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");
        var plannedDinner = dailyMeals.Count(dm => dm.HasDinner && !dm.IsDinnerOnRequest && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");

        // 3. Actual (Checked In) - Occupants who are checked in (includes both Planned and OnRequest)
        var actualBreakfast = dailyMeals.Count(dm => dm.HasBreakfast && dm.BookingOccupant!.IsCheckedIn && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");
        var actualLunch = dailyMeals.Count(dm => dm.HasLunch && dm.BookingOccupant!.IsCheckedIn && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");
        var actualDinner = dailyMeals.Count(dm => dm.HasDinner && dm.BookingOccupant!.IsCheckedIn && dm.BookingOccupant!.Booking!.BookingStatus != "Cancelled");

        // 4. Cancelled - Bookings marked Cancelled OR Individual meals marked Cancelled
        // Logic: EITHER the whole booking is cancelled OR the specific meal was cancelled manually
        var cancelledBreakfast = dailyMeals.Count(dm => (dm.BookingOccupant!.Booking!.BookingStatus == "Cancelled") || (dm.BookingOccupant.Booking.BookingStatus != "Cancelled" && dm.IsBreakfastCancelled));
        var cancelledLunch = dailyMeals.Count(dm => (dm.BookingOccupant!.Booking!.BookingStatus == "Cancelled") || (dm.BookingOccupant.Booking.BookingStatus != "Cancelled" && dm.IsLunchCancelled));
        var cancelledDinner = dailyMeals.Count(dm => (dm.BookingOccupant!.Booking!.BookingStatus == "Cancelled") || (dm.BookingOccupant.Booking.BookingStatus != "Cancelled" && dm.IsDinnerCancelled));

        var viewModel = new MealsDashboardViewModel
        {
            Date = selectedDate,
            
            TotalPlanned = plannedBreakfast + plannedLunch + plannedDinner,
            PlannedBreakfast = plannedBreakfast,
            PlannedLunch = plannedLunch,
            PlannedDinner = plannedDinner,

            TotalActual = actualBreakfast + actualLunch + actualDinner,
            ActualBreakfast = actualBreakfast,
            ActualLunch = actualLunch,
            ActualDinner = actualDinner,

            TotalCancelled = cancelledBreakfast + cancelledLunch + cancelledDinner,
            CancelledBreakfast = cancelledBreakfast,
            CancelledLunch = cancelledLunch,
            CancelledDinner = cancelledDinner,

            TotalOnRequest = requestBreakfast + requestLunch + requestDinner,
            OnRequestBreakfast = requestBreakfast,
            OnRequestLunch = requestLunch,
            OnRequestDinner = requestDinner
        };

        return View(viewModel);
    }
}

public class MealsDashboardViewModel
{
    public DateTime Date { get; set; }

    public int TotalPlanned { get; set; }
    public int PlannedBreakfast { get; set; }
    public int PlannedLunch { get; set; }
    public int PlannedDinner { get; set; }

    public int TotalActual { get; set; }
    public int ActualBreakfast { get; set; }
    public int ActualLunch { get; set; }
    public int ActualDinner { get; set; }

    public int TotalCancelled { get; set; }
    public int CancelledBreakfast { get; set; }
    public int CancelledLunch { get; set; }
    public int CancelledDinner { get; set; }

    public int TotalOnRequest { get; set; }
    public int OnRequestBreakfast { get; set; }
    public int OnRequestLunch { get; set; }
    public int OnRequestDinner { get; set; }
}
