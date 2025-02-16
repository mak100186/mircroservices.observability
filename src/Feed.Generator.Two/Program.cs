using Extensions.Endpoints;
using Microservices.Observability.ServiceDefaults;
using Models;

namespace Feed.Generator.Two;

public class Program
{
    public static void Main(string[] args)
    {
        Type[] typesForSchemaEndpoint = [typeof(CountryWeatherForecast)];

        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaultsWithOpenApi(typesForSchemaEndpoint);

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi();

        app.MapGet("/weatherForecast", Endpoints.GetWeatherForecast)
            .WithName("GetWeatherForecast");

        app.MapGet("/weatherReport", Endpoints.GetWeatherReport)
            .WithName("GetWeatherReport");

        app.MapSchemaEndpoints(typesForSchemaEndpoint);

        app.Run();
    }
}
