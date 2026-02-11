using System.ComponentModel.DataAnnotations;

namespace HotelRoomBookingAPI.Models.Web.DTOs;

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    
    [Required(ErrorMessage = "Company Name is required")]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required")]
    [Display(Name = "Phone Number")]
    // Optional regex validation if desired on client side
    [RegularExpression(@"^\+?\d{10,15}$", ErrorMessage = "Phone number must be 10-15 digits")]
    public string? PhoneNumber { get; set; }
}
