using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [ForeignKey("BuildingsMaster")]
    public int Building_ID { get; set; }

    public int FloorId { get; set; }

    public int? RoomTypeId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string RoomNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    // New property matching DB column
    public int? Occupied { get; set; }

    // Navigation properties
    public virtual BuildingsMaster? Building { get; set; }
    
    public virtual Floor? Floor { get; set; }

    // Removed [JsonIgnore] to allow serializing Room details with Type and Capacity
    public virtual RoomType? RoomType { get; set; }

    [JsonIgnore]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
