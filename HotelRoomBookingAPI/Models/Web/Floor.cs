namespace HotelRoomBookingAPI.Models.Web;

public class Floor
{
    public int FloorId { get; set; }
    public int Building_ID { get; set; } // Changed from HotelId
    public int FloorNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public int No_of_rooms { get; set; } // New property

    public BuildingsMaster? BuildingsMaster { get; set; } // Changed from Hotel
}
