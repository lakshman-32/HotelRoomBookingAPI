using HotelRoomBookingAPI.Models.Web.DTOs;

namespace HotelRoomBookingAPI.Models.Web;

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
    public string BookingStatus { get; set; } = string.Empty;

    public Room? Room { get; set; }
    public User? User { get; set; }
    public ClientKind? ClientKind { get; set; }
    public List<BookingOccupantDto> Occupants { get; set; } = new List<BookingOccupantDto>();
    public List<RoomStatus> RoomStatuses { get; set; } = new List<RoomStatus>();
}
