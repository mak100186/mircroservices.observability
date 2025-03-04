using System.Globalization;
using System.Text.Json;
using Extensions;
using FluentResults;
using Microservice.Three.Receiver.Client.Models;
using Microsoft.Extensions.Options;
using Models;

using static Constants.Constants;

namespace Microservice.Three.Receiver.Client;

public partial class FeedOpenWeatherClient(HttpClient httpClient, IOptions<OpenWeatherClientOptions> clientOptions, ILogger<FeedOpenWeatherClient> logger)
{
    private static Lazy<LocationDto[]>? _locationData;

    private async Task InitializeLocationData(CancellationToken cancellationToken)
    {
        if (_locationData == null || !_locationData.IsValueCreated)
        {
            _locationData = new Lazy<LocationDto[]>(() => GetLocationDataInternal(cancellationToken));

            LocationDto[] GetLocationDataInternal(CancellationToken cancellationToken)
            {
                var result = GetLocationData(cancellationToken).GetAwaiter().GetResult();
                if (result.IsFailed)
                {
                    throw new InvalidOperationException(result.GetErrors());
                }
                return result.Value;
            }
        }
    }

    private async Task<Result<WeatherForecast>> GetWeatherForecastForLocation(LocationDto location, DateOnly date, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"{clientOptions.Value.BaseUrl}?lat={location.Lat}&lon={location.Lon}&date={date}&units=metric&exclude=minutely,hourly,daily&appid={clientOptions.Value.ApiKey}", cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var openApiWeatherData = JsonSerializer.Deserialize<OpenApiWeatherData>(content, SerializerOptions.DefaultSerializerOptions);
        if (openApiWeatherData is null)
        {
            logger.LogError("Failed to get weather forecast for {City}, {State}", location.Name, location.State);
            return Result.Fail<WeatherForecast>($"Failed to get weather forecast for {location.Name}, {location.State}");
        }

        return Result.Ok(openApiWeatherData.ToWeatherForecast(date));
    }

    public async Task<Result<CountryWeatherForecast>> GetWeatherForecast(CancellationToken cancellationToken)
    {
        await InitializeLocationData(cancellationToken);

        var cityWeatherForcasts = new CityWeatherForecast[_locationData.Value.Length];
        for (var j = 0; j < _locationData.Value.Length; j++)
        {
            var cityLocationData = _locationData.Value[j];

            var weathers = new WeatherForecast[NumberOfDaysOfWeatherForecast];
            for (var i = 1; i <= NumberOfDaysOfWeatherForecast; i++)
            {
                var date = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
                var result = await GetWeatherForecastForLocation(cityLocationData, date, cancellationToken);
                if (result.IsFailed)
                {
                    throw new InvalidOperationException(result.GetErrors());
                }
                weathers[i - 1] = result.Value;
            }

            var cityResult = new CityWeatherForecast(cityLocationData.Name, weathers);
            cityWeatherForcasts[j] = cityResult;
        }

        return Result.Ok(new CountryWeatherForecast(cityWeatherForcasts));
    }

    public async Task<Result<CountryWeatherForecast>> GetWeatherReport(CancellationToken cancellationToken)
    {
        await InitializeLocationData(cancellationToken);

        var cityWeatherForcasts = new CityWeatherForecast[_locationData.Value.Length];
        for (var j = 0; j < _locationData.Value.Length; j++)
        {
            var cityLocationData = _locationData.Value[j];

            var weathers = new WeatherForecast[NumberOfDaysOfWeatherForecast];
            for (var i = 1 - NumberOfDaysOfWeatherForecast; i <= 0; i++)
            {
                var date = DateOnly.FromDateTime(DateTime.Now.AddDays(i));
                var result = await GetWeatherForecastForLocation(cityLocationData, date, cancellationToken);
                if (result.IsFailed)
                {
                    throw new InvalidOperationException(result.GetErrors());
                }
                weathers[i - 1 + NumberOfDaysOfWeatherForecast] = result.Value;
            }

            var cityResult = new CityWeatherForecast(cityLocationData.Name, weathers);
            cityWeatherForcasts[j] = cityResult;
        }

        return Result.Ok(new CountryWeatherForecast(cityWeatherForcasts));
    }

    private static readonly string[] CitiesAndStates =
    [
        "Sydney,NSW", "Melbourne,VIC", "Brisbane,QLD", "Perth,WA", "Adelaide,NT", "Gold Coast,QLD", "Canberra,ACT", "Newcastle,NSW", "Central Coast,NSW", "Sunshine Coast,QLD"
    ];

    public async Task<Result<LocationDto[]>> GetLocationData(CancellationToken cancellationToken)
    {
        const string CountryName = "Australia";
        var countryCode = CountryCodeHelper.GetCountryCode(CountryName);
        var tasks = CitiesAndStates
            .Select(cityAndState => GetLocationDataForCityAndState(cityAndState, countryCode, cancellationToken))
            .ToArray();

        var locationDataResults = await Task.WhenAll(tasks);

        var failedResult = locationDataResults.FirstOrDefault(result => !result.IsSuccess);
        if (failedResult != null)
        {
            return Result.Fail<LocationDto[]>(failedResult.GetErrors());
        }

        return Result.Ok(locationDataResults.Select(result => result.Value).ToArray());
    }

    private async Task<Result<LocationDto>> GetLocationDataForCityAndState(string cityAndState, string countryCode, CancellationToken cancellationToken)
    {
        var cityAndStateSplit = cityAndState.Split(',');
        var city = cityAndStateSplit[0];
        var state = cityAndStateSplit[1];

        var response = await httpClient.GetAsync($"{clientOptions.Value.LocationBaseUrl}?q={city},{state},{countryCode}&limit=5&appid={clientOptions.Value.ApiKey}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var locationData = JsonSerializer.Deserialize<LocationDto[]>(content, SerializerOptions.DefaultSerializerOptions);

        if (locationData is null or { Length: 0 })
        {
            logger.LogError("Failed to get location data for {City}, {State}", city, state);
            return Result.Fail<LocationDto>($"Failed to get location data for {city}, {state}");
        }

        var locationDataSelected = locationData[0];
        return Result.Ok(new LocationDto(city, locationDataSelected.Lat, locationDataSelected.Lon, countryCode, state));
    }
}


public static class CountryCodeHelper
{
    public static string GetCountryCode(string countryName)
    {
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            var region = new RegionInfo(culture.Name);
            if (region.EnglishName.Equals(countryName, StringComparison.OrdinalIgnoreCase))
            {
                return region.TwoLetterISORegionName;
            }
        }
        return null; // Return null if the country name is not found
    }
}
