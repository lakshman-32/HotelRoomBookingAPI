namespace HotelRoomBookingAPI.DTOs.Reports;

public class AvailabilityRowDto
{
    public DateTime Date { get; set; }
    public int TotalRooms { get; set; }
    public int BookedRooms { get; set; }
    public int AvailableRooms { get; set; }
}

public class AvailabilityDetailDto
{
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Available" or "Booked"
    public string? OccupantName { get; set; } // Only if booked
}
