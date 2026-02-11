using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

public class Role
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;


    // Navigation properties - ignored during JSON deserialization
    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
