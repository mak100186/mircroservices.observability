using Models;

namespace Extensions.Endpoints;

public static class Endpoints
{
    public static Delegate GetWeatherForecast => () => GetWeatherForecastForRange(1, 5);

    public static Delegate GetWeatherReport => () => GetWeatherForecastForRange(0, 5);


    private static readonly string[] Cities =
    [
        "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Gold Coast", "Canberra", "Newcastle", "Central Coast", "Sunshine Coast"
    ];

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private static CountryWeatherForecast GetWeatherForecastForRange(int start, int count)
    {
        CountryWeatherForecast countryWeatherForecast = new([.. Cities.Select(city =>
            {
                WeatherForecast[] forecast = [.. Enumerable.Range(start, count).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        new (Random.Shared.Next(-20, 55), TemperatureUnit.Celsius),
                        null
                    ))];
                return new CityWeatherForecast(city, forecast);
            })]);

        return countryWeatherForecast.PopulateSummaries();
    }

    public static CountryWeatherForecast PopulateSummaries(this CountryWeatherForecast countryWeatherForecast)
    {
        foreach (var weatherForecast in countryWeatherForecast.CitiesWeatherForecast.SelectMany(x => x.Forecast))
        {
            weatherForecast.Summary = weatherForecast.Temperature.Value switch
            {
                < -10 => Summaries[0],
                < 0 => Summaries[1],
                < 10 => Summaries[2],
                < 20 => Summaries[3],
                < 25 => Summaries[4],
                < 30 => Summaries[5],
                < 35 => Summaries[6],
                < 40 => Summaries[7],
                < 45 => Summaries[8],
                _ => Summaries[9]
            };
        }
        return countryWeatherForecast;
    }
}
