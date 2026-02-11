namespace HotelRoomBookingAPI.Models.Web;

public class RoomStatus
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking? Booking { get; set; }
    
    public bool RoomCleaned { get; set; }
    public bool BathroomCleaned { get; set; }
    public bool BedCleaned { get; set; }
    public bool WaterBottlesProvided { get; set; }
    public bool BedsheetProvided { get; set; }
    public bool TowelProvided { get; set; }
    public bool DustbinCleaned { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }
}
