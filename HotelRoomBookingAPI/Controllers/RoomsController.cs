using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Rooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms([FromQuery] int? floorId = null, [FromQuery] int? buildingId = null)
    {
        var query = _context.Rooms
            .Include(r => r.Building) // Corrected from Hotel
            .Include(r => r.Floor)
            .Include(r => r.RoomType)
            .AsQueryable();

        if (floorId.HasValue)
        {
            query = query.Where(r => r.FloorId == floorId.Value);
        }
        
        if (buildingId.HasValue)
        {
            query = query.Where(r => r.Building_ID == buildingId.Value);
        }

        return await query.ToListAsync();
    }

    // GET: api/Rooms/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Room>> GetRoom(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.Building)
            .Include(r => r.Floor)
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.RoomId == id);

        if (room == null)
        {
            return NotFound();
        }

        return room;
    }

    // GET: api/Rooms/available
    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<Room>>> GetAvailableRooms(
        [FromQuery(Name = "checkInDate")] DateTime? checkInDate,
        [FromQuery(Name = "checkIn")] DateTime? checkIn,
        [FromQuery(Name = "checkOutDate")] DateTime? checkOutDate,
        [FromQuery(Name = "checkOut")] DateTime? checkOut,
        [FromQuery] int? buildingId, // Correct parameter name
        [FromQuery] int? roomTypeId,
        [FromQuery] int? capacity,
        [FromQuery] int? floorNumber)
    {
        var query = _context.Rooms
            .Include(r => r.Building)
            .Include(r => r.Floor)
            .Include(r => r.RoomType)
            // Only consider rooms whose Status is Available
            .Where(r => r.Status == "Available");

        // Filter by building
        if (buildingId.HasValue)
        {
            query = query.Where(r => r.Building_ID == buildingId.Value);
        }

        // Filter by room type
        if (roomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
        }

        // Filter by capacity
        if (capacity.HasValue)
        {
            query = query.Where(r => r.RoomType.Capacity >= capacity.Value);
        }

        // Filter by floor number
        if (floorNumber.HasValue)
        {
            query = query.Where(r => r.Floor.FloorNumber == floorNumber.Value);
        }

        // Filter by date availability
        // Support both checkInDate/checkOutDate and checkIn/checkOut parameter names
        var actualCheckIn = checkInDate ?? checkIn;
        var actualCheckOut = checkOutDate ?? checkOut;

        if (actualCheckIn.HasValue && actualCheckOut.HasValue)
        {
            var bookedRoomIds = await _context.Bookings
                .Where(b => b.BookingStatus != "Cancelled" &&
                           ((b.CheckInDate <= actualCheckIn.Value && b.CheckOutDate > actualCheckIn.Value) ||
                            (b.CheckInDate < actualCheckOut.Value && b.CheckOutDate >= actualCheckOut.Value) ||
                            (b.CheckInDate >= actualCheckIn.Value && b.CheckOutDate <= actualCheckOut.Value)))
                .Select(b => b.RoomId)
                .Distinct()
                .ToListAsync();

            query = query.Where(r => !bookedRoomIds.Contains(r.RoomId));
        }

        return await query.ToListAsync();
    }

    // POST: api/Rooms
    [HttpPost]
    public async Task<ActionResult<Room>> PostRoom([FromBody] Room room)
    {
        if (room == null)
        {
            return BadRequest("Room data is required.");
        }

        if (string.IsNullOrWhiteSpace(room.RoomNumber))
        {
            return BadRequest("RoomNumber is required.");
        }

        if (string.IsNullOrWhiteSpace(room.Status))
        {
            return BadRequest("Status is required.");
        }

        if (room.Building_ID <= 0)
        {
            return BadRequest("Building_ID is required and must be greater than 0.");
        }

        if (room.FloorId <= 0)
        {
            return BadRequest("FloorId is required and must be greater than 0.");
        }

        if (room.RoomTypeId <= 0)
        {
            return BadRequest("RoomTypeId is required and must be greater than 0.");
        }

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoom), new { id = room.RoomId }, room);
    }

    // PUT: api/Rooms/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutRoom(int id, [FromBody] Room room)
    {
        if (room == null)
        {
            return BadRequest("Room data is required.");
        }

        if (string.IsNullOrWhiteSpace(room.RoomNumber))
        {
            return BadRequest("RoomNumber is required.");
        }

        if (string.IsNullOrWhiteSpace(room.Status))
        {
            return BadRequest("Status is required.");
        }

        var existingRoom = await _context.Rooms.FindAsync(id);
        if (existingRoom == null)
        {
            return NotFound($"Room with ID {id} not found.");
        }

        existingRoom.Building_ID = room.Building_ID;
        existingRoom.FloorId = room.FloorId;
        existingRoom.RoomTypeId = room.RoomTypeId;
        existingRoom.RoomNumber = room.RoomNumber;
        existingRoom.Status = room.Status;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoomExists(id))
            {
                return NotFound($"Room with ID {id} not found.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Rooms/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool RoomExists(int id)
    {
        return _context.Rooms.Any(e => e.RoomId == id);
    }

    // POST: api/Rooms/occupied
    // Batch update occupied values (New Requirement)
    [HttpPost("occupied")]
    public async Task<IActionResult> UpdateOccupied([FromBody] List<RoomOccupiedUpdateDto> updates)
    {
        if (updates == null || !updates.Any())
        {
            return BadRequest("No updates provided.");
        }

        foreach (var update in updates)
        {
            var room = await _context.Rooms.FindAsync(update.RoomId);
            if (room != null)
            {
                if (update.Occupied < 0)
                {
                     return BadRequest($"Occupied value for RoomId {update.RoomId} cannot be negative.");
                }
                
                room.Occupied = update.Occupied;
                _context.Entry(room).State = EntityState.Modified;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Occupied values updated successfully." });
    }
    // POST: api/Rooms/update-types
    // Batch update room types
    [HttpPost("update-types")]
    public async Task<IActionResult> UpdateRoomTypes([FromBody] List<RoomTypeUpdateDto> updates)
    {
        if (updates == null || !updates.Any())
        {
            return BadRequest("No updates provided.");
        }

        foreach (var update in updates)
        {
            var room = await _context.Rooms.FindAsync(update.RoomId);
            if (room != null)
            {
                // Verify RoomTypeId exists
                var typeExists = await _context.RoomTypes.AnyAsync(rt => rt.RoomTypeId == update.RoomTypeId);
                if (!typeExists)
                {
                    return BadRequest($"RoomTypeId {update.RoomTypeId} does not exist.");
                }

                room.RoomTypeId = update.RoomTypeId;
                _context.Entry(room).State = EntityState.Modified;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Room types updated successfully." });
    }
}
