using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users
            .Include(u => u.Role)
            .ToListAsync();
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult<User>> PostUser([FromBody] User user)
    {
        if (user == null)
        {
            return BadRequest("User data is required.");
        }

        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            return BadRequest("FullName is required.");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return BadRequest("Email is required.");
        }

        if (user.RoleId <= 0)
        {
            return BadRequest("RoleId is required and must be greater than 0.");
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
    }

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, [FromBody] User user)
    {
        if (user == null)
        {
            return BadRequest("User data is required.");
        }

        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            return BadRequest("FullName is required.");
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return BadRequest("Email is required.");
        }

        if (user.RoleId <= 0)
        {
            return BadRequest("RoleId is required and must be greater than 0.");
        }

        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        existingUser.FullName = user.FullName;
        existingUser.Email = user.Email;
        existingUser.RoleId = user.RoleId;
        existingUser.CompanyName = user.CompanyName;
        existingUser.PasswordHash = user.PasswordHash;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound($"User with ID {id} not found.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Users/5/role - Update user role (Admin only)
    [HttpPut("{id}/role")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] DTOs.UpdateRoleDto dto)
    {
        if (dto == null || dto.RoleId <= 0)
        {
            return BadRequest("Valid RoleId is required.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        // Verify role exists
        var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == dto.RoleId);
        if (!roleExists)
        {
            return BadRequest("Invalid RoleId. Role does not exist.");
        }

        user.RoleId = dto.RoleId;
        await _context.SaveChangesAsync();

        return Ok(new { message = "User role updated successfully", userId = user.UserId, newRoleId = user.RoleId });
    }

    // POST: api/Users/create-user - Create new user (Admin only)
    [HttpPost("create-user")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> CreateUser([FromBody] HotelRoomBookingAPI.DTOs.CreateUserDto dto)
    {
        if (dto == null)
        {
            return BadRequest("User data is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest("FullName is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest("Password is required.");
        }

        if (dto.Password.Length < 6)
        {
            return BadRequest("Password must be at least 6 characters long.");
        }

        if (dto.RoleId <= 0)
        {
             return BadRequest("Valid RoleId is required.");
        }

        // Check if email already exists
        var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
        if (emailExists)
        {
            return Conflict("Email is already registered. Please use a different email.");
        }

        // Hash password using BCrypt
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Create new user
        var newUser = new User
        {
            FullName = dto.FullName,
            Email = dto.Email.ToLower(),
            PasswordHash = passwordHash,
            RoleId = dto.RoleId,
            CompanyName = dto.CompanyName ?? "Hotel Management"
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = newUser.UserId }, new
        {
            userId = newUser.UserId,
            fullName = newUser.FullName,
            email = newUser.Email,
            roleId = newUser.RoleId,
            message = "User created successfully"
        });
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.UserId == id);
    }
}
