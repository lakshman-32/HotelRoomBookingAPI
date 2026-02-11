using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientKindsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientKindsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/ClientKinds
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientKind>>> GetClientKinds()
    {
        return await _context.ClientKinds.ToListAsync();
    }

    // GET: api/ClientKinds/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ClientKind>> GetClientKind(int id)
    {
        var clientKind = await _context.ClientKinds.FindAsync(id);

        if (clientKind == null)
        {
            return NotFound();
        }

        return clientKind;
    }

    // POST: api/ClientKinds
    [HttpPost]
    public async Task<ActionResult<ClientKind>> PostClientKind(ClientKind clientKind)
    {
        _context.ClientKinds.Add(clientKind);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetClientKind", new { id = clientKind.ClientKindId }, clientKind);
    }

    // PUT: api/ClientKinds/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutClientKind(int id, ClientKind clientKind)
    {
        if (id != clientKind.ClientKindId)
        {
            return BadRequest();
        }

        _context.Entry(clientKind).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClientKindExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/ClientKinds/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClientKind(int id)
    {
        var clientKind = await _context.ClientKinds.FindAsync(id);
        if (clientKind == null)
        {
            return NotFound();
        }

        _context.ClientKinds.Remove(clientKind);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ClientKindExists(int id)
    {
        return _context.ClientKinds.Any(e => e.ClientKindId == id);
    }
}
