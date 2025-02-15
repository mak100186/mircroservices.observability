using UnitsNet;
using UnitsNet.Units;

namespace Microservice.Enrichment;

public static class Endpoints
{
    /// <summary>
    /// Converts a temperature in Fahrenheit to Celsius.
    /// </summary>
    /// <param name="farenheit">the temperature value in Farenheit</param>
    /// <response code="200">The response with message</response>
    public static IResult GetCelsius(int farenheit, CancellationToken cancellationToken) =>
        Results.Ok(new Temperature(farenheit, TemperatureUnit.DegreeFahrenheit).DegreesCelsius);

    /// <summary>
    /// Converts a temperature in Celsius to Fahrenheit.
    /// </summary>
    /// <param name="celsius">the temperature value in Celsius</param>
    /// <response code="200">The response with message</response>
    public static IResult GetFarenheit(int celsius, CancellationToken cancellationToken) =>
        Results.Ok(new Temperature(celsius, TemperatureUnit.DegreeCelsius).DegreesFahrenheit);

    /// <summary>
    /// Get the details of a city.
    /// </summary>
    /// <param name="city">name of the city for the full string match</param>
    /// <response code="200">The response with message</response>
    /// <response code="404">The response when the city is not found</response>
    public static IResult GetCityDetails(string city, CancellationToken cancellationToken)
    {
        var cityData = CityRepository.GetCityByName(city);

        if (cityData == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(cityData.ToCityDetailsResponse());
    }
}
