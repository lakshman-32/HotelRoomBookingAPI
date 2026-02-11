namespace HotelRoomBookingAPI.DTOs.Reports;

public class ReportRowDto
{
    public DateTime Date { get; set; }
    public int BreakfastCount { get; set; }
    public int LunchCount { get; set; }
    public int DinnerCount { get; set; }
}
