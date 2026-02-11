using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

public class BookingOccupant
{
    public int BookingOccupantId { get; set; }
    
    public int BookingId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(15)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    // Encrypted Aadhaar number (stored as bytes)
    public byte[] AadhaarEncrypted { get; set; } = Array.Empty<byte>();
    
    // Last 4 digits for display purposes
    [MaxLength(4)]
    public string AadhaarLast4 { get; set; } = string.Empty;
    
    // Meal Options
    public bool HasBreakfast { get; set; }
    public bool HasLunch { get; set; }
    public bool HasDinner { get; set; }

    // Status Flags
    public bool IsCheckedIn { get; set; }
    public DateTime? CheckInTime { get; set; }
    
    public bool IsCheckedOut { get; set; }
    public DateTime? CheckOutTime { get; set; }
    
    // Hashed Aadhaar for uniqueness checks (SHA-256)
    public byte[] AadhaarHash { get; set; } = Array.Empty<byte>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonIgnore]
    public virtual Booking? Booking { get; set; }

    public virtual ICollection<OccupantDailyMeal> DailyMeals { get; set; } = new List<OccupantDailyMeal>();
}
