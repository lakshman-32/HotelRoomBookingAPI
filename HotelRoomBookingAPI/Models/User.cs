using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

public class User
{
    public int UserId { get; set; }

    // Matches FullName column in the database
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    // Added via ALTER TABLE Users ADD PasswordHash VARCHAR(255);
    // PasswordHash is never returned in API responses for security
    [JsonIgnore]
    public string? PasswordHash { get; set; }

    // Navigation properties - ignored during JSON deserialization
    [JsonIgnore]
    public virtual Role Role { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
