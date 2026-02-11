namespace HotelRoomBookingAPI.Models.Web.ViewModels.Reports;

public class ReportsIndexVM
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<ReportRowVM> Reports { get; set; } = new();
    
    public List<ReportRowVM> ChartData { get; set; } = new();
    public string ReportType { get; set; } = "";
    public string Period { get; set; } = "";
}
