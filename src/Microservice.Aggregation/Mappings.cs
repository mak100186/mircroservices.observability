using Aggregation.Persistence;

using Models;

namespace Microservice.Aggregation;

public static class Mappings
{
    public static WeatherForecastModel ToWeatherForecastModel(this AggregatedWeatherForecast aggregatedWeatherForecast) =>
        new()
        {
            City = aggregatedWeatherForecast.City,
            FeedProvider = aggregatedWeatherForecast.FeedProvider,
            Date = aggregatedWeatherForecast.Date,
            Temperature = aggregatedWeatherForecast.Temperature,
            Summary = aggregatedWeatherForecast.Summary
        };

    public static void UpdatePropertiesUsing(this WeatherForecastModel existingWeatherForecastModel, AggregatedWeatherForecast aggregatedWeatherForecast)
    {
        existingWeatherForecastModel.Temperature = aggregatedWeatherForecast.Temperature;
        existingWeatherForecastModel.Summary = aggregatedWeatherForecast.Summary;
    }
}
