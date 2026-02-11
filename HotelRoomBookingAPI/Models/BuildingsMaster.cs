using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

[Table("buildings_master")]
public class BuildingsMaster
{
    [Key]
    public int Building_ID { get; set; }

    [Required]
    [MaxLength(100)]
    public string Building_Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Building_Location { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    public int TotalFloors { get; set; }

    // Navigation property
    [JsonIgnore]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
