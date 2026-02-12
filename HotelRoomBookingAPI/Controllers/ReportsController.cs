using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.DTOs.Reports;

namespace HotelRoomBookingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Reports/meals
    [HttpGet("meals")]
    public async Task<ActionResult<List<ReportRowDto>>> GetMealReports([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        // 1. Fetch all active bookings that overlap with the requested range
        // Logic: Booking Start < Range End AND Booking End > Range Start
        // We fetch occupants and room info as well
        var bookings = await _context.Bookings
            .Include(b => b.Occupants)
                .ThenInclude(o => o.DailyMeals) // Include Daily Meals!
            .Include(b => b.Room) 
            .Where(b => b.BookingStatus == "Booked" || b.BookingStatus == "CheckedIn")
            .Where(b => b.CheckInDate <= toDate && b.CheckOutDate >= fromDate)
            .ToListAsync();

        var report = new List<ReportRowDto>();

        // 2. Iterate through each day in the range
        for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
        {
            var row = new ReportRowDto { Date = date };

            // 3. Find bookings that cover this specific date
            // Inclusive rule: Date >= CheckIn AND Date <= CheckOut
            var activeBookingsForDay = bookings.Where(b => 
                date >= b.CheckInDate.Date && 
                date <= b.CheckOutDate.Date
            ).ToList();

            foreach (var booking in activeBookingsForDay)
            {
                foreach (var occupant in booking.Occupants)
                {
                    // Check for Daily Meal Record first
                    var dailyMeal = occupant.DailyMeals?.FirstOrDefault(dm => dm.Date.Date == date.Date);

                    if (dailyMeal != null)
                    {
                        if (dailyMeal.HasBreakfast) row.BreakfastCount++;
                        if (dailyMeal.HasLunch) row.LunchCount++;
                        if (dailyMeal.HasDinner) row.DinnerCount++;
                    }
                    else
                    {
                        // Fallback to global flags (for legacy or if auto-generation failed/wasn't done yet)
                        if (occupant.HasBreakfast) row.BreakfastCount++;
                        if (occupant.HasLunch) row.LunchCount++;
                        if (occupant.HasDinner) row.DinnerCount++;
                    }
                }
            }

            // Only add row if there is at least one meal to report
            if (row.BreakfastCount > 0 || row.LunchCount > 0 || row.DinnerCount > 0)
            {
                report.Add(row);
            }
        }

        return report;
    }

    // GET: api/Reports/meals/details
    [HttpGet("meals/details")]
    public async Task<ActionResult<List<MealDetailDto>>> GetMealDetails([FromQuery] DateTime date)
    {
        // Find bookings active on this specific date
        // Date >= Start and Date < End
        var bookings = await _context.Bookings
            .Include(b => b.Occupants)
                .ThenInclude(o => o.DailyMeals)
            .Include(b => b.Room)
                .ThenInclude(r => r.Building)
            .Where(b => b.BookingStatus == "Booked" || b.BookingStatus == "CheckedIn")
            .Where(b => b.CheckInDate.Date <= date.Date && b.CheckOutDate.Date >= date.Date)
            .ToListAsync();

        var details = new List<MealDetailDto>();

        foreach (var booking in bookings)
        {
            foreach (var occupant in booking.Occupants)
            {
                var dailyMeal = occupant.DailyMeals?.FirstOrDefault(dm => dm.Date.Date == date.Date);
                
                var hasBreakfast = dailyMeal?.HasBreakfast ?? occupant.HasBreakfast;
                var hasLunch = dailyMeal?.HasLunch ?? occupant.HasLunch;
                var hasDinner = dailyMeal?.HasDinner ?? occupant.HasDinner;

                details.Add(new MealDetailDto
                {
                    FullName = occupant.FullName,
                    PhoneNumber = occupant.PhoneNumber,
                    IsBreakfast = hasBreakfast,
                    IsLunch = hasLunch,
                    IsDinner = hasDinner,
                    RoomNumber = booking.Room?.RoomNumber ?? "N/A",
                    BuildingName = booking.Room?.Building?.Building_Name ?? "N/A"
                });
            }
        }

        return details;
    }
    // GET: api/Reports/availability
    [HttpGet("availability")]
    public async Task<ActionResult<List<AvailabilityRowDto>>> GetAvailabilityReports([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        // 1. Get Total Rooms count
        var totalRooms = await _context.Rooms.CountAsync();

        // 2. Fetch active bookings.
        // We need to fetch bookings that are:
        // - Booked/CheckedIn within range
        // - OR CheckedIn and possibly overstaying (CheckOutDate < fromDate BUT !IsCheckedOut)
        // Note: For overstaying, we need to check Occupants.
        var bookings = await _context.Bookings
            .Include(b => b.Occupants)
            .Where(b => 
                (b.BookingStatus == "Booked" && b.CheckInDate <= toDate && b.CheckOutDate >= fromDate) ||
                (b.BookingStatus == "CheckedIn") // Fetch all CheckedIn, filter in memory for overstays to avoid complex EF translation
            )
            .ToListAsync();

        var report = new List<AvailabilityRowDto>();

        // 3. Iterate through each day
        for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
        {
            var bookedCount = 0;
            foreach (var b in bookings)
            {
                // Standard Date Overlap
                bool dateOverlap = date >= b.CheckInDate.Date && date <= b.CheckOutDate.Date;
                
                // Overstay Logic:
                // If date is Today or earlier, and status is CheckedIn, and NOT all checked out
                // We consider it occupied if it overlaps OR if it's an overstay for THIS date
                // Simplification: If Status is CheckedIn, and ANY occupant is !IsCheckedOut, 
                // AND date >= CheckInDate AND date <= Today (if overstaying). 
                
                // Actually, if they are NOT checked out, they are physically in the room right now (Today).
                // They occupy the room for any date from CheckInDate up to Today (inclusive).
                // Does it block tomorrow? Technically yes until they leave, but for reporting "Availability", usually we check if it's currently occupied.
                
                bool isOccupied = false;

                if (b.BookingStatus == "Booked")
                {
                    isOccupied = dateOverlap;
                }
                else if (b.BookingStatus == "CheckedIn")
                {
                    if (dateOverlap)
                    {
                        isOccupied = true;
                    }
                    else
                    {
                        // Check for overstay
                        // Only relevant if date > CheckOutDate (overstaying)
                        bool overstaying = date > b.CheckOutDate.Date;
                        // And date needs to be <= Now (we can't say they are overstaying next year yet)
                        // Actually, if they haven't checked out today, the room is not available TODAY.
                        
                        if (overstaying && date.Date <= DateTime.Today && b.Occupants.Any(o => !o.IsCheckedOut))
                        {
                            isOccupied = true;
                        }
                    }
                }

                if (isOccupied)
                {
                    bookedCount++;
                }
            }

            report.Add(new AvailabilityRowDto
            {
                Date = date,
                TotalRooms = totalRooms,
                BookedRooms = bookedCount,
                AvailableRooms = totalRooms - bookedCount
            });
        }

        return report;
    }

    // GET: api/Reports/availability/details
    [HttpGet("availability/details")]
    public async Task<ActionResult<List<AvailabilityDetailDto>>> GetAvailabilityDetails([FromQuery] DateTime date, [FromQuery] string? status = null)
    {
        // 1. Get all rooms with Building and RoomType info
        var rooms = await _context.Rooms
            .Include(r => r.Building)
            .Include(r => r.RoomType)
            .ToListAsync();

        // 2. Get bookings relevant to this date
        var bookings = await _context.Bookings
            .Include(b => b.Occupants)
            .Where(b => 
                (b.BookingStatus == "Booked" && b.CheckInDate <= date && b.CheckOutDate >= date) ||
                (b.BookingStatus == "CheckedIn") // Fetch all CheckedIn, filter below
            )
            .ToListAsync();

        var details = new List<AvailabilityDetailDto>();

        foreach (var room in rooms)
        {
            // Find if room is booked/occupied on this date
            var booking = bookings.FirstOrDefault(b => 
            {
                if (b.RoomId != room.RoomId) return false;

                bool dateOverlap = date.Date >= b.CheckInDate.Date && date.Date <= b.CheckOutDate.Date;

                if (b.BookingStatus == "Booked") return dateOverlap;
                
                if (b.BookingStatus == "CheckedIn")
                {
                    if (dateOverlap) return true;
                    // Overstay Logic: Date > CheckOut AND Date <= Today AND !IsCheckedOut
                    if (date.Date > b.CheckOutDate.Date && date.Date <= DateTime.Today && b.Occupants.Any(o => !o.IsCheckedOut))
                    {
                        return true;
                    }
                }
                return false;
            });

            var isBooked = booking != null;

            if (status == "Booked" && !isBooked) continue;
            if (status == "Available" && isBooked) continue;

            details.Add(new AvailabilityDetailDto
            {
                RoomNumber = room.RoomNumber,
                RoomTypeName = room.RoomType?.RoomTypeName ?? "N/A",
                BuildingName = room.Building?.Building_Name ?? "N/A",
                Status = isBooked ? "Booked" : "Available",
                OccupantName = isBooked ? booking!.Occupants.FirstOrDefault()?.FullName : null
            });
        }

        return details;
    }
}
