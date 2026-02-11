using HotelRoomBookingAPI.Models.Web.DTOs;
using HotelRoomBookingAPI.Services.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelRoomBookingAPI.Controllers.Web;

[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto loginDto, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _authService.LoginAsync(loginDto);
            if (success)
            {
                // Get user role from claims for role-based redirection
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                
                // Admin redirection removed as per requirement - all users go to Home
                // if (roleClaim == "Admin") { return RedirectToAction("Dashboard", "Admin"); }
                
                // Check for return URL
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                // Default redirect for regular users
                return RedirectToAction("Index", "Dashboard");
            }
            // Message might be JSON error object, let's keep it simple or try to parse
             // Ideally we just show the message. If backend returns "Invalid email", we show it.
            ModelState.AddModelError(string.Empty, message);
        }
        return View(loginDto);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _authService.RegisterAsync(registerDto);
            if (success)
            {
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
             ModelState.AddModelError(string.Empty, message);
        }
        return View(registerDto);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _authService.ForgotPasswordAsync(forgotPasswordDto);
            if (success)
            {
                TempData["SuccessMessage"] = message;
                // Stay on page to show success message, or redirect to login? 
                // Plan said: "Show success message". View handles TempData["SuccessMessage"]
                ModelState.Clear(); // Clear form
                return View();
            }
            ModelState.AddModelError(string.Empty, message);
        }
        return View(forgotPasswordDto);
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return BadRequest("Invalid password reset link.");
        }

        var model = new ResetPasswordDto
        {
            Email = email,
            Token = token
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        if (ModelState.IsValid)
        {
            var (success, message) = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToAction("Login");
            }
            TempData["ErrorMessage"] = message;
        }
        return View(resetPasswordDto);
    }

    [HttpGet]
    [Route("Logout")]
    [Route("Auth/Logout")] // Handle the 404 path user is hitting
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
