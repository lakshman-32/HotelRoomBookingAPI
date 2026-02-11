namespace HotelRoomBookingAPI.Models.Web.ViewModels.Reports;

public class ReportRowVM
{
    public DateTime Date { get; set; }
    public int BreakfastCount { get; set; }
    public int LunchCount { get; set; }
    public int DinnerCount { get; set; }
    
    // Optional extras if needed for other views, but ensuring core view works
    public string GroupKey { get; set; } = "";
    public int Count { get; set; }
}
