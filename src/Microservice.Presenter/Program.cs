
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
        Type[] typesForSchemaEndpoint = [typeof(AggregatedWeatherForecastResponse)];

        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaultsWithOpenApi(typesForSchemaEndpoint);

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

        app.MapSchemaEndpoints(typesForSchemaEndpoint);

        app.Run();
    }
}
