using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FloorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FloorsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Floors
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Floor>>> GetFloors([FromQuery] int? buildingId = null)
    {
        var query = _context.Floors.Include(f => f.BuildingsMaster).AsQueryable();

        if (buildingId.HasValue)
        {
            query = query.Where(f => f.Building_ID == buildingId.Value);
        }

        return await query.ToListAsync();
    }

    // GET: api/Floors/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Floor>> GetFloor(int id)
    {
        var floor = await _context.Floors.Include(f => f.BuildingsMaster).FirstOrDefaultAsync(f => f.FloorId == id);

        if (floor == null)
        {   
            return NotFound();
        }

        return floor;
    }

    // POST: api/Floors
    [HttpPost]
    public async Task<ActionResult<Floor>> PostFloor([FromBody] Floor floor)
    {
        if (floor == null)
        {
            return BadRequest("Floor data is required.");
        }

        if (floor.Building_ID <= 0)
        {
             return BadRequest("Valid Building_ID is required.");
        }

        _context.Floors.Add(floor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFloor), new { id = floor.FloorId }, floor);
    }

    // PUT: api/Floors/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFloor(int id, [FromBody] Floor floor)
    {
        if (id != floor.FloorId)
        {
            return BadRequest("ID mismatch");
        }

        _context.Entry(floor).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FloorExists(id))
            {
                return NotFound($"Floor with ID {id} not found.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Floors/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFloor(int id)
    {
        var floor = await _context.Floors.FindAsync(id);
        if (floor == null)
        {
            return NotFound();
        }

        // Cascade delete: Delete associated rooms first
        var rooms = await _context.Rooms.Where(r => r.FloorId == id).ToListAsync();
        if (rooms.Any())
        {
            _context.Rooms.RemoveRange(rooms);
        }

        _context.Floors.Remove(floor);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool FloorExists(int id)
    {
        return _context.Floors.Any(e => e.FloorId == id);
    }

    // POST: api/Floors/5/generate-rooms
    // POST: api/Floors/5/generate-rooms
    [HttpPost("{id}/generate-rooms")]
    public async Task<ActionResult<IEnumerable<Room>>> GenerateRooms(int id)
    {
        var floor = await _context.Floors.FindAsync(id);
        if (floor == null)
        {
            return NotFound("Floor not found.");
        }

        // Check if rooms already exist
        var existingRooms = await _context.Rooms
            .Where(r => r.FloorId == id)
            .Include(r => r.RoomType) // Include RoomType for display
            .ToListAsync();

        int currentCount = existingRooms.Count;
        int targetCount = floor.No_of_rooms;

        if (currentCount < targetCount)
        {
            // Generate additional rooms
            var newRooms = new List<Room>();
            int startRoomNo = (floor.FloorNumber * 100) + 1;

            // Start loop from currentCount to targetCount
            for (int i = currentCount; i < targetCount; i++)
            {
                var room = new Room
                {
                    Building_ID = floor.Building_ID,
                    FloorId = floor.FloorId,
                    RoomNumber = (startRoomNo + i).ToString(),
                    Status = "Available",
                    Occupied = 0,
                    RoomTypeId = null
                };
                newRooms.Add(room);
            }

            _context.Rooms.AddRange(newRooms);
            await _context.SaveChangesAsync();
            
            // Re-fetch rooms to include new ones and RoomType relations
            existingRooms = await _context.Rooms
            .Where(r => r.FloorId == id)
            .Include(r => r.RoomType)
            .ToListAsync();
        }
        else if (currentCount > targetCount)
        {
            // Delete extra rooms (remove from the end)
            var roomsToDelete = existingRooms
                .OrderByDescending(r => r.RoomNumber)
                .Take(currentCount - targetCount)
                .ToList();
            
            _context.Rooms.RemoveRange(roomsToDelete);
            await _context.SaveChangesAsync();
            
            // Re-fetch remaining rooms
            existingRooms = await _context.Rooms
                .Where(r => r.FloorId == id)
                .Include(r => r.RoomType)
                .ToListAsync();
        }

        return Ok(existingRooms);
    }
}
