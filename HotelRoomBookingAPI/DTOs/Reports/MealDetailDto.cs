namespace HotelRoomBookingAPI.DTOs.Reports;

public class MealDetailDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsBreakfast { get; set; }
    public bool IsLunch { get; set; }
    public bool IsDinner { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
}
