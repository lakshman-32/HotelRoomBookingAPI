namespace HotelRoomBookingAPI.Models.Web.ViewModels.Reports;

public class MealDetailVM
{
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string BuildingName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    
    public bool IsBreakfast { get; set; }
    public bool IsLunch { get; set; }
    public bool IsDinner { get; set; }
    
    // Additional properties if kept from previous version
    public DateTime Date { get; set; }
    public int BookingId { get; set; }
}
