using Aggregation.Persistence;
using Microservice.Presenter.Client;
using Microservices.Observability.ServiceDefaults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Microservice.Presenter;

public static class Endpoints
{
    /// <summary>
    /// Get the weather forecast for a city on a specific date.
    /// </summary>
    /// <param name="city">The name of the city to search for</param>
    /// <param name="date">The date of the weather</param>
    /// <returns></returns>
    public static async Task<IResult> GetWeatherForecast([FromServices] WeatherMetrics weatherMetrics, [FromServices] AggregationContext dbContext, [FromServices] EnricherClient enricherClient,
        string city, DateOnly date, CancellationToken cancellationToken)
    {
        using var _ = weatherMetrics.MeasurePresentationRequestDuration();
        try
        {
            var weatherForecast = await dbContext.WeatherForecasts
            .AsNoTracking()
            .Where(x => x.City == city && x.Date == date)
            .ToListAsync(cancellationToken: cancellationToken);

            if (weatherForecast.Count == 0)
            {
                return Results.NotFound();
            }

            return Results.Ok(await weatherForecast.ToAggregatedWeatherForecastResponse(enricherClient, cancellationToken));
        }
        finally
        {
            weatherMetrics.IncrementPresentationRequestCounter();
        }
    }
}

