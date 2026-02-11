namespace HotelRoomBookingAPI.DTOs;

public class UpdateBookingStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
