namespace HotelRoomBookingAPI.DTOs;

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
}
