using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HotelRoomBookingAPI.Models.Web;
using HotelRoomBookingAPI.Models.Web.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text.Json;

namespace HotelRoomBookingAPI.Services.Web;

public class AuthService : IAuthService
{
    private readonly ApiService _apiService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(ApiService apiService, IHttpContextAccessor httpContextAccessor)
    {
        _apiService = apiService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<(bool Success, string Message)> LoginAsync(LoginDto loginDto)
    {
        var response = await _apiService.PostAsync("api/auth/login", loginDto);

        if (response.IsSuccessStatusCode)
        {
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (authResponse != null && !string.IsNullOrEmpty(authResponse.Token))
            {
                await SignInUser(authResponse.Token);
                return (true, "Login successful");
            }
        }
        
        var error = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrEmpty(error) ? "Invalid login attempt." : error);
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto registerDto)
    {
        // PhoneNumber is already in DTO and will be serialized automatically
        var response = await _apiService.PostAsync("api/auth/register", registerDto);
        if (response.IsSuccessStatusCode)
        {
             return (true, "Registration successful");
        }
        var error = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrEmpty(error) ? "Registration failed." : error);
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var response = await _apiService.PostAsync("api/auth/forgot-password", forgotPasswordDto);
        
        // For security, checking status code. API returns OK even if email not found (security practice)
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            string msg = "If the account exists, a password reset link has been sent.";
            if (result.TryGetProperty("message", out var messageProp))
            {
                msg = messageProp.GetString() ?? msg;
            }
            return (true, msg);
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrEmpty(error) ? "Request failed." : error);
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var response = await _apiService.PostAsync("api/auth/reset-password", resetPasswordDto);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            string msg = "Password reset successful.";
            if (result.TryGetProperty("message", out var messageProp))
            {
                msg = messageProp.GetString() ?? msg;
            }
            return (true, msg);
        }

        var error = await response.Content.ReadAsStringAsync();
        // Clean error message if it is just a string in quotes
        error = error.Trim('"');
        return (false, string.IsNullOrEmpty(error) ? "Password reset failed." : error);
    }

    public async Task LogoutAsync()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
             _httpContextAccessor.HttpContext.Response.Cookies.Delete("AuthToken");
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    private async Task SignInUser(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var claims = new List<Claim>(jwtToken.Claims);
        // Add the token itself as a claim or store in cookie for later API calls
        // Storing in a separate cookie "AuthToken" for easy access in ApiService
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("AuthToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = _httpContextAccessor.HttpContext?.Request.IsHttps ?? false,
            SameSite = SameSiteMode.Lax,
            Expires = jwtToken.ValidTo
        });

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = jwtToken.ValidTo
        };

        if (_httpContextAccessor.HttpContext != null)
        {
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }
    }
}
