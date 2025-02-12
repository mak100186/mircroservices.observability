using Models;

namespace Microservice.Enrichment;

public static class Mappings
{
    public static CityDetailsResponse ToCityDetailsResponse(this City city) => new()
    {
        Name = city.Name,
        StateCode = city.StateCode,
        StateName = city.StateName,
        CountryCode = city.CountryCode,
        CountryName = city.CountryName,
        Latitude = city.Latitude,
        Longitude = city.Longitude
    };
}
