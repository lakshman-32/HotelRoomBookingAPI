namespace HotelRoomBookingAPI.Models.Web;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
}
