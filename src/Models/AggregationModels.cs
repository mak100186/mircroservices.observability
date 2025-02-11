namespace Models;

public record AggregatedWeatherForecast(FeedProvider FeedProvider, string City, DateOnly Date, Temperature Temperature, string? Summary);

public record AggregatedWeatherForecastResponse(string City, DateOnly Date, FeedProviderForecast[] forecasts);

public record FeedProviderForecast(FeedProvider FeedProvider, Temperature Temperature, string? Summary);
