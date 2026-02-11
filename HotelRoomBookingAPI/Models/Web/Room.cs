namespace HotelRoomBookingAPI.Models.Web;

public class Room
{
    public int RoomId { get; set; }
    public int Building_ID { get; set; } // Changed from HotelId
    public int FloorId { get; set; }
    public int? RoomTypeId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public BuildingsMaster? Building { get; set; } // Changed to match API property name
    public Floor? Floor { get; set; }
    public RoomType? RoomType { get; set; }
}
