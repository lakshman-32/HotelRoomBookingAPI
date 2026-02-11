using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

public class Booking
{
    public int BookingId { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public int? ClientKindId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public DateTime? BookingStartDateTime { get; set; }
    public DateTime? BookingEndDateTime { get; set; }
    
    public string? CancellationRemarks { get; set; }

    // Matches BookingStatus column in the database
    public string BookingStatus { get; set; } = string.Empty;

    public virtual Room? Room { get; set; }
    public virtual User? User { get; set; }
    public virtual ClientKind? ClientKind { get; set; }
    public virtual ICollection<BookingOccupant> Occupants { get; set; } = new List<BookingOccupant>();
    public virtual ICollection<RoomStatus> RoomStatuses { get; set; } = new List<RoomStatus>();
}
