using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;
using HotelRoomBookingAPI.DTOs;
using HotelRoomBookingAPI.Helpers;
using HotelRoomBookingAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly IEmailService _emailService;

    public AuthController(ApplicationDbContext context, JwtHelper jwtHelper, IEmailService emailService)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _emailService = emailService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (registerDto == null)
        {
            return BadRequest("Registration data is required.");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(registerDto.FullName))
        {
            return BadRequest("FullName is required.");
        }

        if (string.IsNullOrWhiteSpace(registerDto.Email))
        {
            return BadRequest("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(registerDto.Password))
        {
            return BadRequest("Password is required.");
        }

        if (registerDto.Password.Length < 6)
        {
            return BadRequest("Password must be at least 6 characters long.");
        }

        if (registerDto.RoleId <= 0)
        {
            return BadRequest("RoleId is required and must be greater than 0.");
        }

        // Validate PhoneNumber
        if (string.IsNullOrWhiteSpace(registerDto.PhoneNumber))
        {
            return BadRequest("Phone Number is required.");
        }

        var phone = registerDto.PhoneNumber.Trim();
        // Simple regex for length 10-15, allowing optional + at start and digits
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?\d{10,15}$"))
        {
            return BadRequest("Phone number must be 10-15 digits long and may start with '+'.");
        }

        // SECURITY: Prevent admin role assignment via public registration
        if (registerDto.RoleId == 1)
        {
            return StatusCode(403, "Admin role cannot be assigned through public registration. Please contact system administrator.");
        }

        // Check if email already exists
        var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());
        if (emailExists)
        {
            return Conflict("Email is already registered. Please use a different email.");
        }

        // Verify RoleId exists
        var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == registerDto.RoleId);
        if (!roleExists)
        {
            return BadRequest("Invalid RoleId. Role does not exist.");
        }

        // Hash password using BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Create new user
        var user = new User
        {
            FullName = registerDto.FullName,
            Email = registerDto.Email.ToLower(),
            PasswordHash = passwordHash,
            RoleId = registerDto.RoleId,
            CompanyName = registerDto.CompanyName ?? string.Empty,
            PhoneNumber = registerDto.PhoneNumber
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Return success response (without password)
        return CreatedAtAction(nameof(Register), new { id = user.UserId }, new
        {
            userId = user.UserId,
            fullName = user.FullName,
            email = user.Email,
            roleId = user.RoleId,
            companyName = user.CompanyName,
            phoneNumber = user.PhoneNumber,
            message = "User registered successfully"
        });
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (loginDto == null)
        {
            return BadRequest("Login credentials are required.");
        }

        if (string.IsNullOrWhiteSpace(loginDto.Email))
        {
            return BadRequest("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(loginDto.Password))
        {
            return BadRequest("Password is required.");
        }

        // Find user by email (case-insensitive)
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // Check if password hash exists
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return Unauthorized("Account is not set up with a password. Please register first.");
        }

        // Verify password using BCrypt
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return Unauthorized("Invalid email or password.");
        }

        // Generate JWT token
        string token = _jwtHelper.GenerateToken(user, user.Role);

        // Return only the token (no user object, no password)
        return Ok(new
        {
            token = token,
            tokenType = "Bearer"
        });
    }

    // POST: api/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
        
        // Always return success message to prevent email enumeration
        if (user == null)
        {
            // Fake work to prevent timing attacks (optional but good practice)
            await Task.Delay(new Random().Next(100, 300));
            return Ok(new { message = "If the account exists, a password reset link has been sent." });
        }

        // Generate secure random token
        var tokenData = RandomNumberGenerator.GetBytes(32);
        var tokenString = Convert.ToBase64String(tokenData).Replace("+", "-").Replace("/", "_").Replace("=", ""); // URL-safe
        
        // Hash token for storage
        using var sha256 = SHA256.Create();
        var tokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenString));

        // Create reset token entity
        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        // Send email
        // hardcoded frontend URL for now, ideally from config
        var resetLink = $"https://localhost:7086/Account/ResetPassword?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(tokenString)}";
        
        var subject = "Password Reset Request";
        var message = $@"
            <h3>Password Reset Request</h3>
            <p>You requested a password reset for your Hotel Booking account.</p>
            <p>Please click the link below to reset your password (valid for 15 minutes):</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>If you did not request this, please ignore this email.</p>";

        await _emailService.SendEmailAsync(user.Email, subject, message);

        return Ok(new { message = "If the account exists, a password reset link has been sent." });
    }

    // POST: api/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());
        if (user == null)
            return BadRequest("Invalid or expired password reset link.");

        // Verify token
        using var sha256 = SHA256.Create();
        var tokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dto.Token));

        var validToken = await _context.PasswordResetTokens
            .Where(t => t.UserId == user.UserId && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAt) // Get latest valid? Or just match hash
            .ToListAsync();

        // Find the matching token hash (doing in memory comparison for byte arrays if EF provider issues, but SQL varbinary compare works)
        // EF Core can compare byte arrays directly in Where usually.
        // Let's rely on finding one that matches.
        var matchedToken = validToken.FirstOrDefault(t => t.TokenHash.SequenceEqual(tokenHash));

        if (matchedToken == null)
        {
            return BadRequest("Invalid or expired password reset link.");
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        
        // Mark token as used
        matchedToken.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Password has been reset successfully. You can now login with your new password." });
    }
    // Logout is handled by AccountController, route alias 'Auth/Logout' added there.
}
