using HotelRoomBookingAPI.Models.Web;
using HotelRoomBookingAPI.Models.Web.DTOs;

namespace HotelRoomBookingAPI.Models.Web.ViewModels;

public class ConfirmBookingViewModel
{
    public Booking Booking { get; set; } = new();
    public List<BookingOccupantDto> Occupants { get; set; } = new();
    public AddOccupantDto NewOccupant { get; set; } = new();
    public List<ClientKind> ClientKinds { get; set; } = new();
}
