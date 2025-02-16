using Microservices.Observability.ServiceDefaults;

namespace Microservice.Enrichment;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaultsWithOpenApi();

        builder.AddRedisOutputCache("cache");

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi();

        app.UseOutputCache();

        app.MapGet("/getCelsius", Endpoints.GetCelsius)
            .WithName("GetCelsius");

        app.MapGet("/getFahrenheit", Endpoints.GetFahrenheit)
            .WithName("GetFahrenheit");

        app.MapGet("/getCityDetails", Endpoints.GetCityDetails)
            .WithName("GetCityDetails")
            .CacheOutput();

        app.MapGet("/getWrongInputResponse/{input}", Endpoints.GetWrongInputResponse)
            .WithName("GetWrongInputResponse");

        //initialize the lazy loading of the cities
        CityRepository.GetCityByName("-");

        app.Run();
    }
}
