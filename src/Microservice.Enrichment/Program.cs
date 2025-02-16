using Microservices.Observability.ServiceDefaults;

namespace Microservice.Enrichment;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaultsWithOpenApi();

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi();

        app.MapGet("/getCelsius", Endpoints.GetCelsius)
            .WithName("GetCelsius");

        app.MapGet("/getFahrenheit", Endpoints.GetFahrenheit)
            .WithName("GetFahrenheit");

        app.MapGet("/getCityDetails", Endpoints.GetCityDetails)
            .WithName("GetCityDetails");

        app.MapGet("/getWrongInputResponse/{input}", Endpoints.GetWrongInputResponse)
            .WithName("GetWrongInputResponse");

        //initialize the lazy loading of the cities
        CityRepository.GetCityByName("-");

        app.Run();
    }
}
