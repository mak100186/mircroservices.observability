using Models;
using static Microservice.Three.Receiver.Client.FeedOpenWeatherClient;

namespace Microservice.Three.Receiver.Client;

public partial class FeedOpenWeatherClient
{
    public class OpenApiWeatherData
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Timezone { get; set; }
        public int TimezoneOffset { get; set; }
        public CurrentWeather Current { get; set; }
    }

    public class CurrentWeather
    {
        public long Dt { get; set; }
        public long Sunrise { get; set; }
        public long Sunset { get; set; }
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public double DewPoint { get; set; }
        public double Uvi { get; set; }
        public int Clouds { get; set; }
        public int Visibility { get; set; }
        public double WindSpeed { get; set; }
        public int WindDeg { get; set; }
        public double WindGust { get; set; }
        public List<WeatherCondition> Weather { get; set; }
    }

    public class WeatherCondition
    {
        public int Id { get; set; }
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }
}

public static class Extensions
{
    public static WeatherForecast ToWeatherForecast(this OpenApiWeatherData openApiWeatherData, DateOnly date)
    {

        var temperatureInC = new Temperature(Convert.ToInt32(openApiWeatherData.Current.Temp), TemperatureUnit.Celsius);
        var summary = openApiWeatherData.Current.Weather.FirstOrDefault()?.Description ?? "Unknown";

        return new WeatherForecast(date, temperatureInC, summary);
    }
}
