namespace HotelRoomBookingAPI.Models.Web.ViewModels;

public class DashboardViewModel
{
    public int AvailableRooms { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public int PendingBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int TotalRooms { get; set; }
    public int BookedRoomsCount { get; set; }
    public int TotalOccupants { get; set; } // Added
    public int OccupantsCheckedIn { get; set; } // Added
    public int TotalMeals { get; set; }
    public int BreakfastCount { get; set; }
    public int LunchCount { get; set; }
    public int DinnerCount { get; set; }

}
