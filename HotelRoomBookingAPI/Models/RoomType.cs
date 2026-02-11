using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

public class RoomType
{
    public int RoomTypeId { get; set; }

    // Matches RoomTypeName column in the database
    public string RoomTypeName { get; set; } = string.Empty;

    // Matches Capacity column in the database
    public int Capacity { get; set; }

    // Navigation properties - ignored during JSON deserialization
    [JsonIgnore]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
