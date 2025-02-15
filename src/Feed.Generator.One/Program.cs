using Extensions.Endpoints;
using Microservices.Observability.ServiceDefaults;

namespace Feed.Generator.One;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi("Feed.Generator.One");

        app.MapGet("/weatherforecast", Endpoints.GetWeatherForecast)
            .WithName("GetWeatherForecast");

        app.MapGet("/weatherreport", Endpoints.GetWeatherReport)
            .WithName("GetWeatherReport");

        app.Run();
    }
}
