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

    public async Task<int> GetCelsius(double farenheit, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/getCelsius?farenheit={farenheit}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var celsius = JsonSerializer.Deserialize<double>(content, SerializerOptions.DefaultSerializerOptions);

            return Convert.ToInt32(celsius);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get celsius for {Farenheit}", farenheit);
            return int.MinValue;
        }
    }

    public async Task<int> GetFarenheit(double celsius, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/GetFarenheit?celsius={celsius}", cancellationToken);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var farenheit = JsonSerializer.Deserialize<double>(content, SerializerOptions.DefaultSerializerOptions);

            return Convert.ToInt32(farenheit);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get farenheit for {Celsius}", celsius);
            return int.MinValue;
        }
    }
}
