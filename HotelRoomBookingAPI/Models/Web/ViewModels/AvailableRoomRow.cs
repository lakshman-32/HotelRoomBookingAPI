namespace HotelRoomBookingAPI.Models.Web.ViewModels;

public class AvailableRoomRow
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    
    public string FloorNumber { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string BuildingLocation { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";
}
