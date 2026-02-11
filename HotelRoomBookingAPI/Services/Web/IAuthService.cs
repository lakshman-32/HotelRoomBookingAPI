using HotelRoomBookingAPI.Models.Web.DTOs;

namespace HotelRoomBookingAPI.Services.Web;

public interface IAuthService
{
    Task<(bool Success, string Message)> LoginAsync(LoginDto loginDto);
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto);
    Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task LogoutAsync();
}
