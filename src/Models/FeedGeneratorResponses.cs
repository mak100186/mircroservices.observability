namespace Models;

public record CountryWeatherForecast(CityWeatherForecast[] CitiesWeatherForecast);
public record CityWeatherForecast(string City, WeatherForecast[] Forecast);
public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public Temperature Temperature { get; set; }
    public string? Summary { get; set; }

    public WeatherForecast(DateOnly date, Temperature temperature, string? summary)
    {
        Date = date;
        Temperature = temperature;
        Summary = summary;
    }
}