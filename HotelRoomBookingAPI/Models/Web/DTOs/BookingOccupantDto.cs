using System.ComponentModel.DataAnnotations;

namespace HotelRoomBookingAPI.Models.Web.DTOs;

public class BookingOccupantDto
{
    public int BookingOccupantId { get; set; }
    public int BookingId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AadhaarLast4 { get; set; } = string.Empty;
    public bool HasBreakfast { get; set; }
    public bool HasLunch { get; set; }
    public bool HasDinner { get; set; }
    public bool IsCheckedIn { get; set; }
    public DateTime? CheckInTime { get; set; }
    public bool IsCheckedOut { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public List<DailyMealDto> DailyMeals { get; set; } = new List<DailyMealDto>();
}

public class AddOccupantDto
{
    [Required]
    public int BookingId { get; set; }

    [Required(ErrorMessage = "Full Name is required.")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(@"^\+\d{1,4}\s\d{6,14}$", ErrorMessage = "Phone Number must start with country code + number.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Aadhaar Number is required.")]
    [RegularExpression(@"^\d{12}$", ErrorMessage = "Aadhaar Number must be 12 digits.")]
    public string AadhaarNumber { get; set; } = string.Empty;

    public bool HasBreakfast { get; set; }
    public bool HasLunch { get; set; }
    public bool HasDinner { get; set; }

    public List<DailyMealDto> DailyMeals { get; set; } = new List<DailyMealDto>();
}

public class DailyMealDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public bool HasBreakfast { get; set; }
    public bool IsBreakfastOnRequest { get; set; }
    public bool IsBreakfastCancelled { get; set; }
    public bool HasLunch { get; set; }
    public bool IsLunchOnRequest { get; set; }
    public bool IsLunchCancelled { get; set; }
    public bool HasDinner { get; set; }
    public bool IsDinnerOnRequest { get; set; }
    public bool IsDinnerCancelled { get; set; }
}

public class UpdateOccupantStatusDto
{
    public int OccupantId { get; set; }
    public bool IsCheckedIn { get; set; }
    public bool IsCheckedOut { get; set; }
}
