using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomStatusController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomStatusController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/RoomStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomStatus>>> GetRoomStatuses(DateTime? date = null)
        {
            var query = _context.RoomStatuses
                .Include(rs => rs.Booking)
                .ThenInclude(b => b.Room)
                .AsQueryable();

            if (date.HasValue)
            {
                // Filter by CreatedAt date
                query = query.Where(rs => rs.CreatedAt.Date == date.Value.Date);
            }
            
            return await query.OrderByDescending(rs => rs.CreatedAt).ToListAsync();
        }

        // GET: api/RoomStatus/booking/5
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<RoomStatus>> GetRoomStatusByBooking(int bookingId)
        {
            var roomStatus = await _context.RoomStatuses.FirstOrDefaultAsync(rs => rs.BookingId == bookingId);

            if (roomStatus == null)
            {
                // Return 404 if not found, or maybe 204 No Content? 
                // Let's return 404 for now so frontend knows to show "Add" form instead of "View"
                return NotFound();
            }

            return roomStatus;
        }

        // POST: api/RoomStatus
        [HttpPost]
        public async Task<ActionResult<RoomStatus>> PostRoomStatus(RoomStatus roomStatus)
        {
            roomStatus.CreatedAt = DateTime.Now;
            _context.RoomStatuses.Add(roomStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoomStatusByBooking", new { bookingId = roomStatus.BookingId }, roomStatus);
        }
    }
}
