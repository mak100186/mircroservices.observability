
using System.Text.Json;
using System.Text.RegularExpressions;
using Extensions;
using Models;

namespace Microservice.Enrichment;

public static class CityRepository
{
    private static readonly Lazy<City[]> Cities = new(Initialize);

    private static City[] Initialize()
    {
        var json = File.ReadAllText("./Data/cities.json");

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<City[]>(json, SerializerOptions.DefaultSerializerOptions) ?? [];
    }

    public static City? GetCityByName(string cityName)
    {
        var regex = new Regex(cityName, RegexOptions.IgnoreCase);
        return Cities.Value.FirstOrDefault(city => regex.IsMatch(city.Name));
    }
}
