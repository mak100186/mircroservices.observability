using Microservices.Observability.ServiceDefaults;

namespace Microservice.Enrichment;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi();

        app.MapGet("/getCelsius", Endpoints.GetCelsius)
            .WithName("GetCelsius");

        app.MapGet("/getFarenheit", Endpoints.GetFarenheit)
            .WithName("GetFarenheit");

        app.MapGet("/getCityDetails", Endpoints.GetCityDetails)
            .WithName("GetCityDetails");

        //initialize the lazy loading of the cities
        CityRepository.GetCityByName("-");

        app.Run();
    }
}
