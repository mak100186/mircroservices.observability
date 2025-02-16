using Microservices.Observability.ServiceDefaults;
using Microsoft.AspNetCore.Mvc;
using UnitsNet;
using UnitsNet.Units;

namespace Microservice.Enrichment;

public static class Endpoints
{
    /// <summary>
    /// Converts a temperature in Fahrenheit to Celsius.
    /// </summary>
    /// <param name="fahrenheit">the temperature value in Fahrenheit</param>
    /// <response code="200">The response with message</response>
    public static IResult GetCelsius([FromServices] WeatherMetrics weatherMetrics, int fahrenheit) =>
        Results.Ok(new Temperature(fahrenheit, TemperatureUnit.DegreeFahrenheit).DegreesCelsius);

    /// <summary>
    /// Converts a temperature in Celsius to Fahrenheit.
    /// </summary>
    /// <param name="celsius">the temperature value in Celsius</param>
    /// <response code="200">The response with message</response>
    public static IResult GetFahrenheit([FromServices] WeatherMetrics weatherMetrics, int celsius) =>
        Results.Ok(new Temperature(celsius, TemperatureUnit.DegreeCelsius).DegreesFahrenheit);

    /// <summary>
    /// Get the details of a city.
    /// </summary>
    /// <param name="city">name of the city for the full string match</param>
    /// <response code="200">The response with message</response>
    /// <response code="404">The response when the city is not found</response>
    public static IResult GetCityDetails([FromServices] WeatherMetrics weatherMetrics, string city)
    {
        using var _ = weatherMetrics.MeasureEnricherRequestDuration();
        try
        {
            var cityData = CityRepository.GetCityByName(city);

            if (cityData == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(cityData.ToCityDetailsResponse());
        }
        finally
        {
            weatherMetrics.IncrementEnricherRequestCounter();
        }
    }
}
