using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Roles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        return await _context.Roles.ToListAsync();
    }

    // GET: api/Roles/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Role>> GetRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);

        if (role == null)
        {
            return NotFound();
        }

        return role;
    }

    // POST: api/Roles
    [HttpPost]
    public async Task<ActionResult<Role>> PostRole([FromBody] Role role)
    {
        if (role == null)
        {
            return BadRequest("Role data is required.");
        }

        if (string.IsNullOrWhiteSpace(role.RoleName))
        {
            return BadRequest("RoleName is required.");
        }

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, role);
    }

    // PUT: api/Roles/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutRole(int id, [FromBody] Role role)
    {
        if (role == null)
        {
            return BadRequest("Role data is required.");
        }

        if (string.IsNullOrWhiteSpace(role.RoleName))
        {
            return BadRequest("RoleName is required.");
        }

        var existingRole = await _context.Roles.FindAsync(id);
        if (existingRole == null)
        {
            return NotFound($"Role with ID {id} not found.");
        }

        existingRole.RoleName = role.RoleName;


        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoleExists(id))
            {
                return NotFound($"Role with ID {id} not found.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Roles/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool RoleExists(int id)
    {
        return _context.Roles.Any(e => e.RoleId == id);
    }
}
