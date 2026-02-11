using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models;

public class OccupantDailyMeal
{
    [Key]
    public int Id { get; set; }

    public int BookingOccupantId { get; set; }

    public DateTime Date { get; set; }

    public bool HasBreakfast { get; set; }
    public bool IsBreakfastOnRequest { get; set; }
    public bool IsBreakfastCancelled { get; set; } // Track individual cancellation

    public bool HasLunch { get; set; }
    public bool IsLunchOnRequest { get; set; }
    public bool IsLunchCancelled { get; set; }

    public bool HasDinner { get; set; }
    public bool IsDinnerOnRequest { get; set; }
    public bool IsDinnerCancelled { get; set; }

    [JsonIgnore]
    public virtual BookingOccupant? BookingOccupant { get; set; }
}
