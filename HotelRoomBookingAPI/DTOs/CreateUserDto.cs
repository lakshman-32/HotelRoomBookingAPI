namespace HotelRoomBookingAPI.DTOs;

public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public int RoleId { get; set; } // 1 for Admin, 2 for User
}
