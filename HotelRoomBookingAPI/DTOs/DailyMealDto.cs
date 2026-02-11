namespace HotelRoomBookingAPI.DTOs;

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
