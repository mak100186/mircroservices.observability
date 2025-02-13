using UnitsNet;
using UnitsNet.Units;

namespace Microservice.Enrichment;

public static class Endpoints
{
    public static Delegate GetCelsius => (int farenheit, CancellationToken cancellationToken) =>
        Results.Ok(new Temperature(farenheit, TemperatureUnit.DegreeFahrenheit).DegreesCelsius);

    public static Delegate GetFarenheit => (int celsius, CancellationToken cancellationToken) =>
        Results.Ok(new Temperature(celsius, TemperatureUnit.DegreeCelsius).DegreesFahrenheit);

    public static Delegate GetCityDetails => (string city, CancellationToken cancellationToken) =>
    {
        var cityData = CityRepository.GetCityByName(city);

        if (cityData == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(cityData.ToCityDetailsResponse());
    };
}
