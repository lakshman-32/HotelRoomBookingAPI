namespace HotelRoomBookingAPI.Models.Web.ViewModels.Reports;

public class AvailabilityIndexVM
{
    public DateTime FromDate { get; set; } = DateTime.Today;
    public DateTime ToDate { get; set; } = DateTime.Today.AddDays(30);
    public List<AvailabilityRowVM> Reports { get; set; } = new();
}

public class AvailabilityRowVM
{
    public DateTime Date { get; set; }
    public int TotalRooms { get; set; }
    public int BookedRooms { get; set; }
    public int AvailableRooms { get; set; }
}

public class AvailabilityDetailVM
{
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? OccupantName { get; set; }
}
