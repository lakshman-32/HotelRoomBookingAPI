using System.Text.Json.Serialization;

namespace HotelRoomBookingAPI.Models.Web;

public class BuildingsMaster
{
    // Mapped from backend Building_ID
    [JsonPropertyName("building_ID")]
    public int Building_ID { get; set; }

    [JsonPropertyName("building_Name")]
    public string Building_Name { get; set; } = string.Empty;

    [JsonPropertyName("building_Location")]
    public string Building_Location { get; set; } = string.Empty;

    public int TotalFloors { get; set; }
    public string Status { get; set; } = string.Empty;
}
