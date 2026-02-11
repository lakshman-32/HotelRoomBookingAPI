namespace HotelRoomBookingAPI.Models.Web.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
}
