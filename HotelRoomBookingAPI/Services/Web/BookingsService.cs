using HotelRoomBookingAPI.Models.Web;
using HotelRoomBookingAPI.Models.Web.DTOs;

namespace HotelRoomBookingAPI.Services.Web;

public interface IBookingsService
{
    Task<Booking?> GetBookingAsync(int id);
    Task<List<BookingOccupantDto>> GetOccupantsAsync(int bookingId);
    Task<bool> AddOccupantAsync(AddOccupantDto occupant);
    Task<bool> RemoveOccupantAsync(int occupantId);
    Task<List<ClientKind>> GetClientKindsAsync();
    Task<(bool Success, string Message)> UpdateBookingAsync(Booking booking);
}

public class BookingsService : IBookingsService
{
    private readonly ApiService _apiService;

    public BookingsService(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<Booking?> GetBookingAsync(int id)
    {
        return await _apiService.GetAsync<Booking>($"api/bookings/{id}");
    }

    public async Task<List<BookingOccupantDto>> GetOccupantsAsync(int bookingId)
    {
        var occupants = await _apiService.GetAsync<List<BookingOccupantDto>>($"api/bookings/{bookingId}/occupants");
        return occupants ?? new List<BookingOccupantDto>();
    }

    public async Task<bool> AddOccupantAsync(AddOccupantDto occupant)
    {
        var response = await _apiService.PostAsync("api/bookings/occupants", occupant);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveOccupantAsync(int occupantId)
    {
        var response = await _apiService.DeleteAsync($"api/bookings/occupants/{occupantId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ClientKind>> GetClientKindsAsync()
    {
        var clientKinds = await _apiService.GetAsync<List<ClientKind>>("api/ClientKinds");
        return clientKinds ?? new List<ClientKind>();
    }

    public async Task<(bool Success, string Message)> UpdateBookingAsync(Booking booking)
    {
        var response = await _apiService.PutAsync($"api/bookings/{booking.BookingId}", booking);
        if (response.IsSuccessStatusCode)
        {
            return (true, string.Empty);
        }
        var error = await response.Content.ReadAsStringAsync();
        return (false, error);
    }
}
