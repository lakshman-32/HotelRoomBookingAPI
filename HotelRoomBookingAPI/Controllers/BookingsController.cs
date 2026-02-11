using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly Services.IAadhaarCryptoService _aadhaarCryptoService;

    public BookingsController(ApplicationDbContext context, Services.IAadhaarCryptoService aadhaarCryptoService)
    {
        _context = context;
        _aadhaarCryptoService = aadhaarCryptoService;
    }

    // GET: api/Bookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
    {
        return await _context.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r.Building)
            .Include(b => b.Room)
                .ThenInclude(r => r.Floor)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .Include(b => b.ClientKind) // Include ClientKind
            .Include(b => b.User)
            .Include(b => b.Occupants)
            .ToListAsync();
    }

    // GET: api/Bookings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Booking>> GetBooking(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r.Building)
            .Include(b => b.Room)
                .ThenInclude(r => r.Floor)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .Include(b => b.ClientKind) // Include ClientKind
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.BookingId == id);

        if (booking == null)
        {
            return NotFound();
        }

        return booking;
    }

    // GET: api/Bookings/user/5
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsByUser(int userId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Room)
                .ThenInclude(r => r.Building)
            .Include(b => b.Room)
                .ThenInclude(r => r.Floor)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .Include(b => b.ClientKind) // Include ClientKind
            .Include(b => b.User)
            .Include(b => b.User)
            .Include(b => b.Occupants)
            .Include(b => b.RoomStatuses)
            .Where(b => b.UserId == userId)
            .ToListAsync();

        return bookings;
    }

    // POST: api/Bookings
    [HttpPost]
    public async Task<ActionResult<Booking>> PostBooking([FromBody] Booking booking)
    {
        if (booking == null)
        {
            return BadRequest("Booking data is required.");
        }

        // Validate dates
        // Allow same-day bookings if using time-based booking (BookingStartDateTime and BookingEndDateTime)
        if (booking.BookingStartDateTime.HasValue && booking.BookingEndDateTime.HasValue)
        {
            // For time-based bookings, validate using DateTime
            if (booking.BookingStartDateTime >= booking.BookingEndDateTime)
            {
                return BadRequest("Booking end time must be after start time.");
            }
            
            if (booking.BookingStartDateTime < DateTime.Now)
            {
                return BadRequest("Booking start time cannot be in the past.");
            }
        }
        else
        {
            // For date-only bookings, use the old validation
            if (booking.CheckInDate >= booking.CheckOutDate)
            {
                return BadRequest("Check-out date must be after check-in date.");
            }
            
            if (booking.CheckInDate < DateTime.Today)
            {
                return BadRequest("Check-in date cannot be in the past.");
            }
        }

        // Check room availability
        var isRoomAvailable = await IsRoomAvailableAsync(
            booking.RoomId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.BookingId);

        if (!isRoomAvailable)
        {
            return BadRequest("Room is not available for the selected dates.");
        }

        // Set initial status (BookingStatus column)
        booking.BookingStatus = "Booked";

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Load related data for response
        await _context.Entry(booking)
            .Reference(b => b.Room)
            .LoadAsync();
        await _context.Entry(booking.Room).Reference(r => r.Building).LoadAsync();
        await _context.Entry(booking.Room).Reference(r => r.Floor).LoadAsync();
        await _context.Entry(booking.Room).Reference(r => r.RoomType).LoadAsync();
        await _context.Entry(booking)
            .Reference(b => b.User)
            .LoadAsync();

        return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, booking);
    }

    // PUT: api/Bookings/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBooking(int id, [FromBody] Booking booking)
    {
        if (booking == null)
        {
            return BadRequest("Booking data is required.");
        }

        // Check if booking exists
        var existingBooking = await _context.Bookings.FindAsync(id);
        if (existingBooking == null)
        {
            return NotFound($"Booking with ID {id} not found.");
        }

        // Validate dates
        // Validate dates
        if (booking.BookingStartDateTime.HasValue && booking.BookingEndDateTime.HasValue)
        {
             if (booking.BookingStartDateTime >= booking.BookingEndDateTime)
             {
                 return BadRequest("Booking end time must be after start time.");
             }
        }
        else if (booking.CheckInDate >= booking.CheckOutDate)
        {
            return BadRequest("Check-out date must be after check-in date.");
        }

        // Only validate "past date" if the date is actually being changed
        if (booking.CheckInDate != existingBooking.CheckInDate && booking.CheckInDate < DateTime.Today)
        {
            return BadRequest("Check-in date cannot be in the past.");
        }

        // Check room availability (excluding current booking)
        var isRoomAvailable = await IsRoomAvailableAsync(
            booking.RoomId,
            booking.CheckInDate,
            booking.CheckOutDate,
            id);

        if (!isRoomAvailable)
        {
            return BadRequest("Room is not available for the selected dates.");
        }

        // Update the existing booking with new values (use URL parameter ID, ignore BookingId from body)
        existingBooking.RoomId = booking.RoomId;
        existingBooking.UserId = booking.UserId;
        existingBooking.CheckInDate = booking.CheckInDate;
        existingBooking.CheckOutDate = booking.CheckOutDate;
        existingBooking.BookingStatus = booking.BookingStatus;
        existingBooking.ClientKindId = booking.ClientKindId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookingExists(id))
            {
                return NotFound($"Booking with ID {id} not found.");
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // PUT: api/Bookings/5/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] global::HotelRoomBookingAPI.DTOs.UpdateBookingStatusDto request)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        var validStatuses = new[] { "Booked", "CheckedIn", "CheckedOut", "Cancelled" };
        if (!validStatuses.Contains(request.Status))
        {
            return BadRequest($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");
        }

        booking.BookingStatus = request.Status;
        
        if (request.Status == "Cancelled" && !string.IsNullOrEmpty(request.Remarks))
        {
            booking.CancellationRemarks = request.Remarks;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Bookings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BookingExists(int id)
    {
        return _context.Bookings.Any(e => e.BookingId == id);
    }
    
    // GET: api/Bookings/5/occupants
    [HttpGet("{bookingId}/occupants")]
    public async Task<ActionResult<IEnumerable<HotelRoomBookingAPI.DTOs.BookingOccupantDto>>> GetBookingOccupants(int bookingId)
    {
        var occupants = await _context.BookingOccupants
            .Where(o => o.BookingId == bookingId)
            .Include(o => o.DailyMeals) // Include Daily Meals
            .OrderBy(o => o.CreatedAt)
            .Select(o => new global::HotelRoomBookingAPI.DTOs.BookingOccupantDto
            {
                BookingOccupantId = o.BookingOccupantId,
                BookingId = o.BookingId,
                FullName = o.FullName,
                PhoneNumber = o.PhoneNumber,
                AadhaarLast4 = o.AadhaarLast4,
                // If DailyMeals exist, check them for flags. Fallback to global flags.
                HasBreakfast = o.DailyMeals.Any() ? o.DailyMeals.Any(dm => dm.HasBreakfast) : o.HasBreakfast,
                HasLunch = o.DailyMeals.Any() ? o.DailyMeals.Any(dm => dm.HasLunch) : o.HasLunch,
                HasDinner = o.DailyMeals.Any() ? o.DailyMeals.Any(dm => dm.HasDinner) : o.HasDinner,
                IsCheckedIn = o.IsCheckedIn,
                CheckInTime = o.CheckInTime,
                IsCheckedOut = o.IsCheckedOut,
                CheckOutTime = o.CheckOutTime,
                DailyMeals = o.DailyMeals.Select(dm => new global::HotelRoomBookingAPI.DTOs.DailyMealDto {
                     Id = dm.Id,
                     Date = dm.Date,
                     HasBreakfast = dm.HasBreakfast,
                     IsBreakfastOnRequest = dm.IsBreakfastOnRequest,
                     IsBreakfastCancelled = dm.IsBreakfastCancelled,
                     HasLunch = dm.HasLunch,
                     IsLunchOnRequest = dm.IsLunchOnRequest,
                     IsLunchCancelled = dm.IsLunchCancelled,
                     HasDinner = dm.HasDinner,
                     IsDinnerOnRequest = dm.IsDinnerOnRequest,
                     IsDinnerCancelled = dm.IsDinnerCancelled
                }).ToList()
            })
            .ToListAsync();

        // Ensure DateTime properties are treated as UTC so they are serialized with 'Z'
        foreach (var occupant in occupants)
        {
            if (occupant.CheckInTime.HasValue)
            {
                occupant.CheckInTime = DateTime.SpecifyKind(occupant.CheckInTime.Value, DateTimeKind.Utc);
            }
            if (occupant.CheckOutTime.HasValue)
            {
                occupant.CheckOutTime = DateTime.SpecifyKind(occupant.CheckOutTime.Value, DateTimeKind.Utc);
            }
        }

        return occupants;
    }

    // GET: api/Bookings/occupants
    [HttpGet("occupants")]
    public async Task<ActionResult<IEnumerable<HotelRoomBookingAPI.DTOs.BookingOccupantDto>>> GetAllOccupants()
    {
        var occupants = await _context.BookingOccupants
            .Include(o => o.DailyMeals)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new global::HotelRoomBookingAPI.DTOs.BookingOccupantDto
            {
                BookingOccupantId = o.BookingOccupantId,
                BookingId = o.BookingId,
                FullName = o.FullName,
                PhoneNumber = o.PhoneNumber,
                AadhaarLast4 = o.AadhaarLast4,
                HasBreakfast = o.DailyMeals.Any() ? o.DailyMeals.Any(dm => dm.HasBreakfast) : o.HasBreakfast,
                HasLunch = o.DailyMeals.Any() ? o.DailyMeals.Any(dm => dm.HasLunch) : o.HasLunch,
                HasDinner = o.DailyMeals.Any() ? o.DailyMeals.Any(dm => dm.HasDinner) : o.HasDinner,
                IsCheckedIn = o.IsCheckedIn,
                CheckInTime = o.CheckInTime,
                IsCheckedOut = o.IsCheckedOut,
                CheckOutTime = o.CheckOutTime,
                DailyMeals = o.DailyMeals.Select(dm => new global::HotelRoomBookingAPI.DTOs.DailyMealDto {
                     Id = dm.Id,
                     Date = dm.Date,
                     HasBreakfast = dm.HasBreakfast,
                     IsBreakfastOnRequest = dm.IsBreakfastOnRequest,
                     HasLunch = dm.HasLunch,
                     IsLunchOnRequest = dm.IsLunchOnRequest,
                     HasDinner = dm.HasDinner,
                     IsDinnerOnRequest = dm.IsDinnerOnRequest
                }).ToList()
            })
            .ToListAsync();
            
        // Ensure UTC
        foreach (var occupant in occupants)
        {
            if (occupant.CheckInTime.HasValue) occupant.CheckInTime = DateTime.SpecifyKind(occupant.CheckInTime.Value, DateTimeKind.Utc);
            if (occupant.CheckOutTime.HasValue) occupant.CheckOutTime = DateTime.SpecifyKind(occupant.CheckOutTime.Value, DateTimeKind.Utc);
        }

        return occupants;
    }

    // POST: api/Bookings/occupants
    [HttpPost("occupants")]
    public async Task<ActionResult<global::HotelRoomBookingAPI.DTOs.BookingOccupantDto>> AddOccupant([FromBody] global::HotelRoomBookingAPI.DTOs.AddOccupantDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var booking = await _context.Bookings.FindAsync(request.BookingId);
        if (booking == null)
        {
            return NotFound($"Booking with ID {request.BookingId} not found.");
        }

        // Validate Aadhaar Uniqueness
        var aadhaarHash = _aadhaarCryptoService.Hash(request.AadhaarNumber);
        
        // Check if Aadhaar already exists (Global check based on constraints)
        // User requested handling UNIQUE constraint gracefully. Check first.
        var exists = await _context.BookingOccupants.AnyAsync(o => o.AadhaarHash == aadhaarHash);
        if (exists)
        {
            return Conflict(new { message = "An occupant with this Aadhaar number already exists in the system." });
        }

        // Create Occupant
        var occupant = new BookingOccupant
        {
            BookingId = request.BookingId,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            AadhaarEncrypted = _aadhaarCryptoService.Encrypt(request.AadhaarNumber),
            AadhaarHash = aadhaarHash,
            AadhaarLast4 = _aadhaarCryptoService.Mask(request.AadhaarNumber).Substring(_aadhaarCryptoService.Mask(request.AadhaarNumber).Length - 4),
            HasBreakfast = request.HasBreakfast,
            HasLunch = request.HasLunch,
            HasDinner = request.HasDinner,
            CreatedAt = DateTime.UtcNow
        };

        // Populate Daily Meals
        if (request.DailyMeals != null && request.DailyMeals.Any())
        {
            // Use provided daily meals
            foreach (var dailyMeal in request.DailyMeals)
            {
                occupant.DailyMeals.Add(new OccupantDailyMeal
                {
                    Date = dailyMeal.Date,
                    HasBreakfast = dailyMeal.HasBreakfast,
                    HasLunch = dailyMeal.HasLunch,
                    HasDinner = dailyMeal.HasDinner
                });
            }
            
            // Update global flags to true if ANY day has that meal selected (for summary display)
            occupant.HasBreakfast = request.DailyMeals.Any(dm => dm.HasBreakfast);
            occupant.HasLunch = request.DailyMeals.Any(dm => dm.HasLunch);
            occupant.HasDinner = request.DailyMeals.Any(dm => dm.HasDinner);
        }
        else
        {
            // Auto-generate based on booking dates and main flags (Legacy/Fast Entry Support)
            // Inclusive of CheckIn, Exclusive of CheckOut for "Staying Night"? 
            // Or Inclusive of CheckOut for meals? 
            // Generally hotels charge meals per day. 
            // Let's include CheckIn date through CheckOut date.
            // Logic: Iterate from CheckIn to CheckOut.
            
            for (var date = booking.CheckInDate.Date; date <= booking.CheckOutDate.Date; date = date.AddDays(1))
            {
                occupant.DailyMeals.Add(new OccupantDailyMeal
                {
                    Date = date,
                    HasBreakfast = request.HasBreakfast,
                    HasLunch = request.HasLunch,
                    HasDinner = request.HasDinner
                });
            }
            // Global flags are already set from request in initialization
        }

        _context.BookingOccupants.Add(occupant);
        await _context.SaveChangesAsync();

        var responseDto = new global::HotelRoomBookingAPI.DTOs.BookingOccupantDto
        {
            BookingOccupantId = occupant.BookingOccupantId,
            BookingId = occupant.BookingId,
            FullName = occupant.FullName,
            PhoneNumber = occupant.PhoneNumber,
            AadhaarLast4 = occupant.AadhaarLast4,
            HasBreakfast = occupant.HasBreakfast,
            HasLunch = occupant.HasLunch,
            HasDinner = occupant.HasDinner,
            DailyMeals = occupant.DailyMeals.Select(dm => new global::HotelRoomBookingAPI.DTOs.DailyMealDto {
                 Date = dm.Date,
                 HasBreakfast = dm.HasBreakfast,
                 HasLunch = dm.HasLunch,
                 HasDinner = dm.HasDinner
            }).ToList()
        };

        return CreatedAtAction(nameof(GetBookingOccupants), new { bookingId = occupant.BookingId }, responseDto);
    }

    // DELETE: api/Bookings/occupants/5
    [HttpDelete("occupants/{id}")]
    public async Task<IActionResult> RemoveOccupant(int id)
    {
        var occupant = await _context.BookingOccupants.FindAsync(id);
        if (occupant == null)
        {
            return NotFound();
        }

        _context.BookingOccupants.Remove(occupant);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkInDate, DateTime checkOutDate, int? excludeBookingId = null)
    {
        var query = _context.Bookings
            .Include(b => b.Occupants)
            .Where(b => b.RoomId == roomId &&
                       b.BookingStatus != "Cancelled" &&
                       ((b.CheckInDate <= checkInDate && b.CheckOutDate > checkInDate) ||
                        (b.CheckInDate < checkOutDate && b.CheckOutDate >= checkOutDate) ||
                        (b.CheckInDate >= checkInDate && b.CheckOutDate <= checkOutDate)));

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.BookingId != excludeBookingId.Value);
        }

        var conflictingBookings = await query.ToListAsync();
        
        // Filter out bookings where all occupants are checked out
        var activeConflicts = conflictingBookings.Where(b => 
            !b.Occupants.Any() || // No occupants yet, so booking is still active
            !b.Occupants.All(o => o.IsCheckedOut) // Not all occupants are checked out
        ).Any();
        
        return !activeConflicts;
    }

    // PUT: api/Bookings/occupants/5/status
    [HttpPut("occupants/{id}/status")]
    public async Task<IActionResult> UpdateOccupantStatus(int id, [FromBody] UpdateOccupantStatusDto request)
    {
        var occupant = await _context.BookingOccupants.FindAsync(id);
        if (occupant == null)
        {
            return NotFound();
        }

        // Check In
        if (!occupant.IsCheckedIn && request.IsCheckedIn)
        {
             occupant.CheckInTime = DateTime.UtcNow;
        }

        occupant.IsCheckedIn = request.IsCheckedIn;
        
        // Check Out
        if (!occupant.IsCheckedOut && request.IsCheckedOut)
        {
            occupant.CheckOutTime = DateTime.UtcNow;

            // Logic: Cancel future meals
            // We load DailyMeals to act on them
            await _context.Entry(occupant).Collection(o => o.DailyMeals).LoadAsync();
            
            var tomorrow = DateTime.Today.AddDays(1);
            // logic: Cancel all meals from tomorrow onwards
            // User request implies "remaining period".
            
            var futureMeals = occupant.DailyMeals.Where(dm => dm.Date >= tomorrow).ToList();
            foreach(var meal in futureMeals)
            {
                // Mark all as cancelled
                // Note: We don't change "HasBreakfast" flag because that indicates "Planned". 
                // We set "IsCancelled" to true.
                
                if (meal.HasBreakfast) meal.IsBreakfastCancelled = true;
                if (meal.HasLunch) meal.IsLunchCancelled = true;
                if (meal.HasDinner) meal.IsDinnerCancelled = true;
            }
            
            // Optional: Handle "Today's" remaining meals? 
            // If checking out at 10AM, lunch and dinner should be cancelled.
            // Let's implement that for completeness.
            var todayMeals = occupant.DailyMeals.FirstOrDefault(dm => dm.Date == DateTime.Today);
            if(todayMeals != null)
            {
                 var now = DateTime.Now.Hour;
                 // Breakfast ends ~10:30. If checkout > 10, breakfast is done/missed.
                 // Lunch starts ~12:00.
                 // Dinner starts ~19:00.
                 
                 // If checking out before 11:30 (11.5), cancel Lunch
                 if (now < 11.5 && todayMeals.HasLunch) todayMeals.IsLunchCancelled = true;
                 
                 // If checking out before 19:00 (19), cancel Dinner
                 if (now < 19 && todayMeals.HasDinner) todayMeals.IsDinnerCancelled = true;
            }
        }

        occupant.IsCheckedOut = request.IsCheckedOut;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT: api/Bookings/occupants/5/meals
    [HttpPut("occupants/{occupantId}/meals")]
    public async Task<IActionResult> UpdateOccupantMeals(int occupantId, [FromBody] List<global::HotelRoomBookingAPI.DTOs.UpdateDailyMealDto> request)
    {
        var occupant = await _context.BookingOccupants
            .Include(o => o.DailyMeals)
            .FirstOrDefaultAsync(o => o.BookingOccupantId == occupantId);

        if (occupant == null)
        {
            return NotFound();
        }

        Console.WriteLine($"UpdateOccupantMeals: Updating {occupantId}. Request Count: {request.Count}");

        foreach (var reqMeal in request)
        {
            Console.WriteLine($"Processing Meal ID: {reqMeal.Id}. B-Cancel: {reqMeal.IsBreakfastCancelled}");
            var meal = occupant.DailyMeals.FirstOrDefault(m => m.Id == reqMeal.Id);
            if (meal != null)
            {
                meal.HasBreakfast = reqMeal.HasBreakfast;
                meal.IsBreakfastOnRequest = reqMeal.IsBreakfastOnRequest;
                meal.IsBreakfastCancelled = reqMeal.IsBreakfastCancelled;
                
                meal.HasLunch = reqMeal.HasLunch;
                meal.IsLunchOnRequest = reqMeal.IsLunchOnRequest;
                meal.IsLunchCancelled = reqMeal.IsLunchCancelled;
                
                meal.HasDinner = reqMeal.HasDinner;
                meal.IsDinnerOnRequest = reqMeal.IsDinnerOnRequest;
                meal.IsDinnerCancelled = reqMeal.IsDinnerCancelled;
            }
        }
        
        // Recalculate global flags (HasBreakfast if ANY day has breakfast)
        occupant.HasBreakfast = occupant.DailyMeals.Any(dm => dm.HasBreakfast);
        occupant.HasLunch = occupant.DailyMeals.Any(dm => dm.HasLunch);
        occupant.HasDinner = occupant.DailyMeals.Any(dm => dm.HasDinner);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UpdateOccupantStatusDto
{
    public bool IsCheckedIn { get; set; }
    public bool IsCheckedOut { get; set; }
}


