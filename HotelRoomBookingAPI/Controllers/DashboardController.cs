using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.DTOs;
using System.Security.Claims;

namespace HotelRoomBookingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Dashboard/stats
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats(DateTime? date = null)
    {
        var targetDate = date?.Date ?? DateTime.Today;
        
        // Get available rooms (not booked for targetDate)
        var allRooms = await _context.Rooms.CountAsync();
        var bookedRoomIds = await _context.Bookings
            .Where(b => (b.BookingStatus == "Booked" || b.BookingStatus == "CheckedIn") &&
                       b.CheckInDate.Date <= targetDate.Date &&
                       b.CheckOutDate.Date >= targetDate.Date)
            .Select(b => b.RoomId)
            .Distinct()
            .CountAsync();
        
        var availableRooms = allRooms - bookedRoomIds;

        // Check-ins for targetDate
        var todayCheckIns = await _context.Bookings
            .Where(b => b.CheckInDate.Date == targetDate &&
                       b.BookingStatus != "Cancelled")
            .CountAsync();

        // Check-outs for targetDate
        var todayCheckOuts = await _context.Bookings
            .Where(b => b.CheckOutDate.Date == targetDate &&
                       b.BookingStatus != "Cancelled")
            .CountAsync();

        // Pending bookings for targetDate
        var pendingBookings = await _context.Bookings
            .Include(b => b.Occupants)
            .Where(b => b.BookingStatus == "Booked" &&
                       b.CheckInDate.Date == targetDate &&
                       (b.Occupants.Count == 0 || !b.Occupants.Any(o => o.IsCheckedIn)))
            .CountAsync();

        // Cancelled bookings for targetDate
        var cancelledBookings = await _context.Bookings
            .Where(b => b.BookingStatus == "Cancelled" &&
                       b.CheckInDate.Date == targetDate)
            .CountAsync();

        // Calculate Meal Counts for targetDate (Admin)
        var activeBookingsForMeals = await _context.Bookings
            .Include(b => b.Occupants)
                .ThenInclude(o => o.DailyMeals)
            .Where(b => b.BookingStatus != "Cancelled" &&
                       b.CheckInDate <= targetDate &&
                       b.CheckOutDate >= targetDate)
            .ToListAsync();

        int breakfastCount = 0;
        int lunchCount = 0;
        int dinnerCount = 0;

        foreach (var booking in activeBookingsForMeals)
        {
            foreach (var occupant in booking.Occupants)
            {
                // Check specific daily meal record for targetDate
                var dailyMeal = occupant.DailyMeals?.FirstOrDefault(dm => dm.Date.Date == targetDate);
                
                if (dailyMeal != null)
                {
                    if (dailyMeal.HasBreakfast) breakfastCount++;
                    if (dailyMeal.HasLunch) lunchCount++;
                    if (dailyMeal.HasDinner) dinnerCount++;
                }
                else
                {
                    // Fallback to global flags only if no daily plan exists (Legacy)
                    if (occupant.HasBreakfast) breakfastCount++;
                    if (occupant.HasLunch) lunchCount++;
                    if (occupant.HasDinner) dinnerCount++;
                }
            }
        }

        // Calculate Occupant Stats
        var totalOccupants = activeBookingsForMeals.SelectMany(b => b.Occupants).Count();
        var checkedInOccupants = activeBookingsForMeals.SelectMany(b => b.Occupants).Count(o => o.IsCheckedIn);

        var stats = new DashboardStatsDto
        {
            AvailableRooms = availableRooms,
            TodayCheckIns = todayCheckIns,
            TodayCheckOuts = todayCheckOuts,
            PendingBookings = pendingBookings,
            CancelledBookings = cancelledBookings,
            TotalMeals = breakfastCount + lunchCount + dinnerCount,
            BreakfastCount = breakfastCount,
            LunchCount = lunchCount,
            DinnerCount = dinnerCount,
            TotalRooms = allRooms, // Added
            BookedRoomsCount = bookedRoomIds, // Added
            TotalOccupants = totalOccupants, // Added
            OccupantsCheckedIn = checkedInOccupants // Added
        };

        return stats;
    }

    // GET: api/Dashboard/user-stats/{userId}
    [HttpGet("user-stats/{userId}")]
    public async Task<ActionResult<DashboardStatsDto>> GetUserDashboardStats(int userId, DateTime? date = null)
    {
        var targetDate = date?.Date ?? DateTime.Today;
        
        // For user dashboard, show their personal stats
        var userBookings = _context.Bookings.Where(b => b.UserId == userId);

        // Available rooms (system-wide, same as admin)
        var allRooms = await _context.Rooms.CountAsync();
        var bookedRoomIds = await _context.Bookings
            .Where(b => (b.BookingStatus == "Booked" || b.BookingStatus == "CheckedIn") &&
                       b.CheckInDate.Date <= targetDate.Date &&
                       b.CheckOutDate.Date >= targetDate.Date)
            .Select(b => b.RoomId)
            .Distinct()
            .CountAsync();
        
        var availableRooms = allRooms - bookedRoomIds;

        // User's check-ins for targetDate
        var todayCheckIns = await userBookings
            .Where(b => b.CheckInDate.Date == targetDate &&
                       b.BookingStatus != "Cancelled")
            .CountAsync();

        // User's check-outs for targetDate
        var todayCheckOuts = await userBookings
            .Where(b => b.CheckOutDate.Date == targetDate &&
                       b.BookingStatus != "Cancelled")
            .CountAsync();

        // User's pending bookings for targetDate
        var pendingBookings = await userBookings
            .Include(b => b.Occupants)
            .Where(b => b.BookingStatus == "Booked" &&
                       b.CheckInDate.Date == targetDate &&
                       (b.Occupants.Count == 0 || !b.Occupants.Any(o => o.IsCheckedIn)))
            .CountAsync();

        // User's cancelled bookings for targetDate
        var cancelledBookings = await userBookings
            .Where(b => b.BookingStatus == "Cancelled" &&
                       b.CheckInDate.Date == targetDate)
            .CountAsync();
            
        // Calculate Meal Counts for targetDate (User)
        var activeUserBookings = await userBookings
            .Include(b => b.Occupants)
                .ThenInclude(o => o.DailyMeals)
            .Where(b => b.BookingStatus != "Cancelled" &&
                       b.CheckInDate <= targetDate &&
                       b.CheckOutDate >= targetDate)
            .ToListAsync();

        int breakfastCount = 0;
        int lunchCount = 0;
        int dinnerCount = 0;

        foreach (var booking in activeUserBookings)
        {
            foreach (var occupant in booking.Occupants)
            {
                // Check specific daily meal record for targetDate
                var dailyMeal = occupant.DailyMeals?.FirstOrDefault(dm => dm.Date.Date == targetDate);

                if (dailyMeal != null)
                {
                    if (dailyMeal.HasBreakfast) breakfastCount++;
                    if (dailyMeal.HasLunch) lunchCount++;
                    if (dailyMeal.HasDinner) dinnerCount++;
                }
                else
                {
                    // Fallback
                    if (occupant.HasBreakfast) breakfastCount++;
                    if (occupant.HasLunch) lunchCount++;
                    if (occupant.HasDinner) dinnerCount++;
                }
            }
        }

        // Calculate Occupant Stats
        var totalOccupants = activeUserBookings.SelectMany(b => b.Occupants).Count();
        var checkedInOccupants = activeUserBookings.SelectMany(b => b.Occupants).Count(o => o.IsCheckedIn);

        var stats = new DashboardStatsDto
        {
            AvailableRooms = availableRooms,
            TodayCheckIns = todayCheckIns,
            TodayCheckOuts = todayCheckOuts,
            PendingBookings = pendingBookings,
            CancelledBookings = cancelledBookings,
            TotalMeals = breakfastCount + lunchCount + dinnerCount,
            BreakfastCount = breakfastCount,
            LunchCount = lunchCount,
            DinnerCount = dinnerCount,
            TotalRooms = allRooms, // Added
            BookedRoomsCount = bookedRoomIds, // Added
            TotalOccupants = totalOccupants, // Added
            OccupantsCheckedIn = checkedInOccupants // Added
        };

        return stats;
    }
}
