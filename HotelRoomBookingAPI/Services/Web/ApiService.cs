using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HotelRoomBookingAPI.Services.Web;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthorizationHeader()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AddAuthorizationHeader();
        var response = await _httpClient.GetAsync(endpoint);
        if (response.IsSuccessStatusCode)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return await response.Content.ReadFromJsonAsync<T>(options);
        }
        return default;
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
    {
        AddAuthorizationHeader();
        return await _httpClient.PostAsJsonAsync(endpoint, data);
    }
    
    // Helper method to POST and get result back
    public async Task<TResult?> PostAsync<TInput, TResult>(string endpoint, TInput data)
    {
        AddAuthorizationHeader();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TResult>();
        }
        return default;
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
    {
        AddAuthorizationHeader();
        return await _httpClient.PutAsJsonAsync(endpoint, data);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        AddAuthorizationHeader();
        return await _httpClient.DeleteAsync(endpoint);
    }

    // Expose HttpClient for custom requests
    public HttpClient GetHttpClient()
    {
        AddAuthorizationHeader();
        return _httpClient;
    }
}
