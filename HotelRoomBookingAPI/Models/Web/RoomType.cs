namespace HotelRoomBookingAPI.Models.Web;

public class RoomType
{
    public int RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
