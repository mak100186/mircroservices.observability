namespace Models;

public record CountryWeatherForecast(CityWeatherForecast[] CitiesWeatherForecast);
public record CityWeatherForecast(string City, WeatherForecast[] Forecast);
public record WeatherForecast(DateOnly Date, Temperature Temperature, string? Summary);