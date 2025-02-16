using Microservices.Observability.ServiceDefaults;
using Models;

namespace Microservice.Enrichment;

public class Program
{
    public static void Main(string[] args)
    {
        Type[] typesForSchemaEndpoint = [typeof(CityDetailsResponse)];

        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaultsWithOpenApi(typesForSchemaEndpoint);

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

        app.MapSchemaEndpoints(typesForSchemaEndpoint);

        //initialize the lazy loading of the cities
        CityRepository.GetCityByName("-");

        app.Run();
    }
}
