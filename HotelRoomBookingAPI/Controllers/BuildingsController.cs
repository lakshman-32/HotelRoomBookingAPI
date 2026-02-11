using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BuildingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BuildingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Buildings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BuildingsMaster>>> GetBuildings()
    {
        return await _context.BuildingsMasters.ToListAsync();
    }

    // GET: api/Buildings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BuildingsMaster>> GetBuilding(int id)
    {
        var building = await _context.BuildingsMasters.FindAsync(id);

        if (building == null)
        {
            return NotFound();
        }

        return building;
    }

    // POST: api/Buildings
    [HttpPost]
    public async Task<ActionResult<BuildingsMaster>> PostBuilding([FromBody] BuildingsMaster building)
    {
        if (building == null)
        {
            return BadRequest("Building data is required.");
        }

        if (string.IsNullOrWhiteSpace(building.Building_Name))
        {
            return BadRequest("Building_Name is required.");
        }

        _context.BuildingsMasters.Add(building);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBuilding), new { id = building.Building_ID }, building);
    }

    // PUT: api/Buildings/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBuilding(int id, [FromBody] BuildingsMaster building)
    {
        if (id != building.Building_ID)
        {
            return BadRequest("ID mismatch");
        }

        if (string.IsNullOrWhiteSpace(building.Building_Name))
        {
             return BadRequest("Building_Name is required.");
        }

        _context.Entry(building).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BuildingExists(id))
            {
                return NotFound($"Building with ID {id} not found.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Buildings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBuilding(int id)
    {
        var building = await _context.BuildingsMasters.FindAsync(id);
        if (building == null)
        {
            return NotFound();
        }

        _context.BuildingsMasters.Remove(building);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BuildingExists(int id)
    {
        return _context.BuildingsMasters.Any(e => e.Building_ID == id);
    }
}
