namespace Models;

public record AggregatedWeatherForecast(FeedProvider FeedProvider, string City, DateOnly Date, Temperature Temperature, string? Summary);

public record AggregatedWeatherForecastResponse(string City, CityDetails? CityDetails, DateOnly Date, FeedProviderForecast[] forecasts);

public record FeedProviderForecast(FeedProvider FeedProvider, Temperature[] Temperatures, string? Summary);

public record CityDetails(string StateCode, string StateName, string CountryCode, string CountryName, string Latitude, string Longitude);