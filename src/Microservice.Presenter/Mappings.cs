using Aggregation.Persistence;
using Microservice.Presenter.Client;

using Models;

namespace Microservice.Presenter;

public static class Mappings
{
    public static async Task<Temperature[]> GetAllScales(this Temperature temperature, EnricherClient enricher, CancellationToken cancellationToken)
    {
        if (temperature.Unit == TemperatureUnit.Celsius)
        {
            return [temperature, new Temperature(await enricher.GetFahrenheit(temperature.Value, cancellationToken), TemperatureUnit.Fahrenheit)];
        }
        else
        {
            return [new Temperature(await enricher.GetCelsius(temperature.Value, cancellationToken), TemperatureUnit.Celsius), temperature];
        }
    }
    public static async Task<AggregatedWeatherForecastResponse> ToAggregatedWeatherForecastResponse(this List<WeatherForecastModel> weatherForecastModels, EnricherClient enricherClient, CancellationToken cancellationToken)
    {
        var city = weatherForecastModels.First().City;
        var date = weatherForecastModels.First().Date;

        var cityDetailResponse = await enricherClient.GetCityDetails(weatherForecastModels.First().City, cancellationToken);

        var forecasts = await Task.WhenAll(weatherForecastModels.Select(async x =>
            new FeedProviderForecast(x.FeedProvider, await x.Temperature.GetAllScales(enricherClient, cancellationToken), x.Summary)));


        return new AggregatedWeatherForecastResponse(city, cityDetailResponse.ToCityDetails(), date, forecasts);
    }

    public static CityDetails? ToCityDetails(this CityDetailsResponse? cityDetailsResponse) =>
        cityDetailsResponse is null
        ? null
        : new(cityDetailsResponse.StateCode, cityDetailsResponse.StateName, cityDetailsResponse.CountryCode, cityDetailsResponse.CountryName, cityDetailsResponse.Latitude, cityDetailsResponse.Longitude);
}
