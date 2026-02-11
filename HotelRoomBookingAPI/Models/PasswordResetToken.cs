using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelRoomBookingAPI.Models;

public class PasswordResetToken
{
    [Key]
    public int ResetTokenId { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(64)] // Store as byte[] or string? User asked for VARBINARY(32) in DB schema desc, but C# usually handles this slightly differently. 
                    // Wait, prompt said: "2) dbo.PasswordResetTokens table exists with columns: ResetTokenId (PK), UserId (FK), TokenHash VARBINARY(32)..."
                    // If DB has VARBINARY(32), EF Core mapping should be byte[].
    public byte[] TokenHash { get; set; } = Array.Empty<byte>();

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual User User { get; set; } = null!;
}
