
using Aggregation.Persistence;
using Microservice.Presenter.Client;
using Microservices.Observability.ServiceDefaults;
using Models;
using static Constants.Constants;

namespace Microservice.Presenter;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaultsWithOpenApi();

        builder.AddNpgsqlDbContext<AggregationContext>(Postgres.ConnectionName);

        builder.Services.AddOptions<ClientOptions>()
            .Bind(builder.Configuration.GetSection(ClientOptions.ClientsSectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<EnricherClient>();
        builder.Services.AddHttpClient<EnricherClient>();

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi();

        app.MapGet("/weatherforecast", Endpoints.GetWeatherForecast)
            .WithName("GetWeatherForecast");

        app.Run();
    }
}
