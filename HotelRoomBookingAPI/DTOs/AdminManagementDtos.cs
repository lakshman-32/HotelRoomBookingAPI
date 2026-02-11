namespace HotelRoomBookingAPI.DTOs;

public class UpdateRoleDto
{
    public int RoleId { get; set; }
}

public class CreateAdminDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
}
