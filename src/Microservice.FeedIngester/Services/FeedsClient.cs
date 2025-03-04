using System.Text.Json;
using Extensions;
using FluentResults;
using Microsoft.Extensions.Options;
using Models;

namespace Microservice.FeedIngester.Services;

public class FeedsClient(HttpClient httpClient, IOptions<Clients> clients)
{
    public async Task<Result<List<CityWeatherForecast>>> GetWeatherForecast(CancellationToken cancellationToken)
    {
        try
        {
            var forecasts = new List<CityWeatherForecast>();
            foreach (var client in clients.Value.ClientList)
            {
                var baseUrl = client.Url;

                var response = await httpClient.GetAsync($"{baseUrl}/weatherforecast", cancellationToken);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var countryWeatherForecast = JsonSerializer.Deserialize<CountryWeatherForecast>(content, SerializerOptions.DefaultSerializerOptions);

                if (countryWeatherForecast is { CitiesWeatherForecast.Length: > 0 })
                {
                    forecasts.AddRange(countryWeatherForecast.CitiesWeatherForecast);
                }
            }

            return Result.Ok(forecasts);
        }
        catch (Exception ex)
        {
            return Result.Fail(new FluentResults.Error(ex.Message).CausedBy(ex));
        }
    }

    public async Task<Result<List<CityWeatherForecast>>> GetWeatherReport(CancellationToken cancellationToken)
    {
        try
        {
            var forecasts = new List<CityWeatherForecast>();
            foreach (var client in clients.Value.ClientList)
            {
                var baseUrl = client.Url;

                var response = await httpClient.GetAsync($"{baseUrl}/weatherreport", cancellationToken);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var countryWeatherForecast = JsonSerializer.Deserialize<CountryWeatherForecast>(content, SerializerOptions.DefaultSerializerOptions);

                if (countryWeatherForecast is { CitiesWeatherForecast.Length: > 0 })
                {
                    forecasts.AddRange(countryWeatherForecast.CitiesWeatherForecast);
                }
            }

            return Result.Ok(forecasts);
        }
        catch (Exception ex)
        {
            return Result.Fail(new FluentResults.Error(ex.Message).CausedBy(ex));
        }
    }
}
