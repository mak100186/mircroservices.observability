using System.Text.Json.Serialization;

namespace Models;

public class CityDetailsResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("state_code")]
    public string StateCode { get; set; }

    [JsonPropertyName("state_name")]
    public string StateName { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }

    [JsonPropertyName("country_name")]
    public string CountryName { get; set; }

    [JsonPropertyName("latitude")]
    public string Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public string Longitude { get; set; }
}
