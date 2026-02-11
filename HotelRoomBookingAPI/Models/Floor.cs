using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

public class Floor
{
    [Key]
    public int FloorId { get; set; }

    [ForeignKey("BuildingsMaster")]
    public int Building_ID { get; set; }

    public int FloorNumber { get; set; }
    
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    // New column as per requirements
    public int No_of_rooms { get; set; }

    // Navigation properties
    // [JsonIgnore] - Removed to allow serialization
    public virtual BuildingsMaster? BuildingsMaster { get; set; }

    [JsonIgnore]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
