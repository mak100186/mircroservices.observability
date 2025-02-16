using Extensions.Endpoints;
using Microservices.Observability.ServiceDefaults;

namespace Feed.Generator.Two;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaultsWithOpenApi();

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi();

        app.MapGet("/weatherforecast", Endpoints.GetWeatherForecast)
            .WithName("GetWeatherForecast");

        app.MapGet("/weatherreport", Endpoints.GetWeatherReport)
            .WithName("GetWeatherReport");

        app.Run();
    }
}
