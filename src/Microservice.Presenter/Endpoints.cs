using Aggregation.Persistence;
using Microservice.Presenter.Client;
using Microsoft.EntityFrameworkCore;

namespace Microservice.Presenter;

public static class Endpoints
{
    public static Delegate GetWeatherForecast => async (HttpContext httpContext, AggregationContext dbContext, string city, DateOnly date, EnricherClient enricherClient, CancellationToken cancellationToken) =>
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
    };
}
