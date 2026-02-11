using System.ComponentModel.DataAnnotations;

namespace HotelRoomBookingAPI.Models;

public class ClientKind
{
    [Key]
    public int ClientKindId { get; set; }

    [Required]
    [StringLength(50)]
    public string ClientKindName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
