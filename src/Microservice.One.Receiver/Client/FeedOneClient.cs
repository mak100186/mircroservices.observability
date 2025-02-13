using System.Text.Json;

using Extensions;

using FluentResults;

using Microsoft.Extensions.Options;

using Models;

namespace Microservice.One.Receiver.Client;

public class FeedOneClient(HttpClient httpClient, IOptions<ClientOptions> clientOptions)
{
    private readonly string _baseUrl = clientOptions.Value.BaseUrl;

    public async Task<Result<CountryWeatherForecast>> GetWeatherForecast(CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/weatherforecast", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var countryWeatherForecast = JsonSerializer.Deserialize<CountryWeatherForecast>(content, SerializerOptions.DefaultSerializerOptions);

            return countryWeatherForecast == null || countryWeatherForecast.CitiesWeatherForecast.Length == 0
                ? (Result<CountryWeatherForecast>)Result.Fail("Failed to deserialize the response")
                : Result.Ok(countryWeatherForecast);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message).CausedBy(ex));
        }
    }

    public async Task<Result<CountryWeatherForecast>> GetWeatherReport(CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/weatherreport", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var countryWeatherForecast = JsonSerializer.Deserialize<CountryWeatherForecast>(content, SerializerOptions.DefaultSerializerOptions);

            return countryWeatherForecast == null || countryWeatherForecast.CitiesWeatherForecast.Length == 0
                ? (Result<CountryWeatherForecast>)Result.Fail("Failed to deserialize the response")
                : Result.Ok(countryWeatherForecast);
        }
        catch (Exception ex)
        {
            return Result.Fail(new Error(ex.Message).CausedBy(ex));
        }
    }
}
