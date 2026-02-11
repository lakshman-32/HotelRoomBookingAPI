using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelRoomBookingAPI.Models.Web.ViewModels;

public class AvailabilitySearchVM
{
    // Search Criteria
    public int? BuildingId { get; set; }
    public int? FloorId { get; set; }  // NEW: Floor filter
    public DateTime? Date { get; set; }  // Check-in date
    public DateTime? CheckOutDate { get; set; }  // Check-out date (optional for multi-day bookings)
    public TimeSpan? FromTime { get; set; }
    public TimeSpan? ToTime { get; set; }
    
    // Results
    public int AvailableRoomsCount { get; set; }
    public List<AvailableRoomRow> AvailableRooms { get; set; } = new();
    
    // Dropdown Data
    public List<BuildingsMaster> Buildings { get; set; } = new();
    public SelectList? BuildingsSelectList { get; set; }
    
    public List<Floor> Floors { get; set; } = new();  // NEW: Floors list
    public SelectList? FloorsSelectList { get; set; }  // NEW: Floors dropdown
    
    // Helper property to check if search was performed
    public bool SearchPerformed { get; set; }
}
