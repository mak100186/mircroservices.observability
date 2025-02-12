
using Aggregation.Persistence;
using Microservice.Two.Receiver.Client;
using Microservices.Observability.ServiceDefaults;
using Models;

namespace Microservice.Presenter;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.AddNpgsqlDbContext<AggregationContext>("postgresdb");

        builder.Services.AddOptions<ClientOptions>()
            .Bind(builder.Configuration.GetSection(ClientOptions.ClientsSectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<EnricherClient>();
        builder.Services.AddHttpClient<EnricherClient>();

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi(false);

        app.MapGet("/weatherforecast", Endpoints.GetWeatherForecast)
            .WithName("GetWeatherForecast");

        app.Run();
    }
}
