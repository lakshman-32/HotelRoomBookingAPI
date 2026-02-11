using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomTypesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoomTypesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/RoomTypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomType>>> GetRoomTypes()
    {
        return await _context.RoomTypes.ToListAsync();
    }

    // GET: api/RoomTypes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<RoomType>> GetRoomType(int id)
    {
        var roomType = await _context.RoomTypes.FindAsync(id);

        if (roomType == null)
        {
            return NotFound();
        }

        return roomType;
    }

    // POST: api/RoomTypes
    [HttpPost]
    public async Task<ActionResult<RoomType>> PostRoomType([FromBody] RoomType roomType)
    {
        if (roomType == null)
        {
            return BadRequest("RoomType data is required.");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(roomType.RoomTypeName))
        {
            return BadRequest("RoomTypeName is required.");
        }

        if (roomType.Capacity <= 0)
        {
            return BadRequest("Capacity must be greater than 0.");
        }

        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoomType), new { id = roomType.RoomTypeId }, roomType);
    }

    // PUT: api/RoomTypes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutRoomType(int id, [FromBody] RoomType roomType)
    {
        if (roomType == null)
        {
            return BadRequest("RoomType data is required.");
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(roomType.RoomTypeName))
        {
            return BadRequest("RoomTypeName is required.");
        }

        if (roomType.Capacity <= 0)
        {
            return BadRequest("Capacity must be greater than 0.");
        }

        // Check if room type exists
        var existingRoomType = await _context.RoomTypes.FindAsync(id);
        if (existingRoomType == null)
        {
            return NotFound($"RoomType with ID {id} not found.");
        }

        // Update the existing room type with new values (ignore RoomTypeId from body, use URL parameter)
        existingRoomType.RoomTypeName = roomType.RoomTypeName;
        existingRoomType.Capacity = roomType.Capacity;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoomTypeExists(id))
            {
                return NotFound($"RoomType with ID {id} not found.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/RoomTypes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoomType(int id)
    {
        var roomType = await _context.RoomTypes.FindAsync(id);
        if (roomType == null)
        {
            return NotFound();
        }

        _context.RoomTypes.Remove(roomType);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool RoomTypeExists(int id)
    {
        return _context.RoomTypes.Any(e => e.RoomTypeId == id);
    }
}
