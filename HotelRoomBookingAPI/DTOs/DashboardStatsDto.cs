namespace HotelRoomBookingAPI.DTOs;

public class DashboardStatsDto
{
    public int AvailableRooms { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int PendingBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int TotalRooms { get; set; } // Added
    public int BookedRoomsCount { get; set; } // Added
    public int TotalOccupants { get; set; } // Added
    public int OccupantsCheckedIn { get; set; } // Added
    public int TotalMeals { get; set; }
    public int BreakfastCount { get; set; }
    public int LunchCount { get; set; }
    public int DinnerCount { get; set; }
}

