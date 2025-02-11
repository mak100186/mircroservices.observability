using Aggregation.Persistence;

using Models;

namespace Microservice.Aggregation;

public static class Mappings
{
    public static AggregatedWeatherForecastResponse ToAggregatedWeatherForecastResponse(this List<WeatherForecastModel> weatherForecastModels)
    {
        return new AggregatedWeatherForecastResponse(
            weatherForecastModels.First().City,
            weatherForecastModels.First().Date,
            [.. weatherForecastModels.Select(x => new FeedProviderForecast(x.FeedProvider, x.Temperature, x.Summary))]);
    }
    public static WeatherForecastModel ToWeatherForecastModel(this AggregatedWeatherForecast aggregatedWeatherForecast)
    {
        return new WeatherForecastModel
        {
            City = aggregatedWeatherForecast.City,
            FeedProvider = aggregatedWeatherForecast.FeedProvider,
            Date = aggregatedWeatherForecast.Date,
            Temperature = aggregatedWeatherForecast.Temperature,
            Summary = aggregatedWeatherForecast.Summary
        };
    }

    public static void UpdatePropertiesUsing(this WeatherForecastModel existingWeatherForecastModel, AggregatedWeatherForecast aggregatedWeatherForecast)
    {
        existingWeatherForecastModel.Temperature = aggregatedWeatherForecast.Temperature;
        existingWeatherForecastModel.Summary = aggregatedWeatherForecast.Summary;
    }
}
