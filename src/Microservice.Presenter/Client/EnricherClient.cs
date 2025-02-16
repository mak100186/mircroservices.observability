using System.Text.Json;

using Extensions;
using Microsoft.Extensions.Options;

using Models;

namespace Microservice.Presenter.Client;

public class EnricherClient(HttpClient httpClient, ILogger<EnricherClient> logger, IOptions<ClientOptions> clientOptions)
{
    private readonly string _baseUrl = clientOptions.Value.BaseUrl;

    public async Task<CityDetailsResponse?> GetCityDetails(string cityName, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/GetCityDetails?city={cityName}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var cityDetailsResponse = JsonSerializer.Deserialize<CityDetailsResponse>(content, SerializerOptions.DefaultSerializerOptions);

            return cityDetailsResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get city details for {CityName}", cityName);
            return null;
        }
    }

    public async Task<int> GetCelsius(double fahrenheit, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/getCelsius?fahrenheit={fahrenheit}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var celsius = JsonSerializer.Deserialize<double>(content, SerializerOptions.DefaultSerializerOptions);

            return Convert.ToInt32(celsius);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get celsius for {Fahrenheit}", fahrenheit);
            return int.MinValue;
        }
    }

    public async Task<int> GetFahrenheit(double celsius, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/GetFahrenheit?celsius={celsius}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var fahrenheit = JsonSerializer.Deserialize<double>(content, SerializerOptions.DefaultSerializerOptions);

            return Convert.ToInt32(fahrenheit);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get fahrenheit for {Celsius}", celsius);
            return int.MinValue;
        }
    }
}
