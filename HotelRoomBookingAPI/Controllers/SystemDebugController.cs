using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemDebugController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SystemDebugController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _context.Roles.ToListAsync();
        return Ok(roles);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.Select(u => new 
        { 
            u.UserId, 
            u.Email, 
            u.RoleId, 
            RoleName = u.Role != null ? u.Role.RoleName : "NULL",
            HasPassword = !string.IsNullOrEmpty(u.PasswordHash) 
        }).ToListAsync();
        
        return Ok(users);
    }
}
