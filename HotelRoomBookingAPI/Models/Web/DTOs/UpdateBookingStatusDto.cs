namespace HotelRoomBookingAPI.Models.Web.DTOs;

public class UpdateBookingStatusDto
{
    public int OccupantId { get; set; } // Required for occupant status updates if reused, but for Booking status we generally use ID in URL.
    // However, the previous API Controller had two endpoints:
    // 1. PUT api/Bookings/{id}/status (Booking Status) - takes string
    // 2. PUT api/Bookings/occupants/{id}/status (Occupant Status) - takes UpdateOccupantStatusDto
    
    // We are changing the first one. Let's make sure the naming is clear.
    // The implementation plan said UpdateBookingStatusDto.
    
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
