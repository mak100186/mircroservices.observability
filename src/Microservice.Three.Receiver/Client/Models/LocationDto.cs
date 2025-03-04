using System.Text.Json.Serialization;

namespace Microservice.Three.Receiver.Client.Models;

public record LocationDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lon")] double Lon,
    [property: JsonPropertyName("country")] string Country,
    [property: JsonPropertyName("state")] string State);
