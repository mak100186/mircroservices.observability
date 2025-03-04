
using System.Net;
using Extensions;
using Microservice.FeedIngester.Services;
using Microservices.Observability.ServiceDefaults;
using Microsoft.Extensions.Options;
using Models;

namespace Microservice.FeedIngester;

public class Program
{
    public static void Main(string[] args)
    {
        Type[] typesForSchemaEndpoint = [typeof(CountryWeatherForecast)];

        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaultsWithOpenApi(typesForSchemaEndpoint);

        var clients = builder.Configuration
            .GetSection(Client.ClientsSectionName)
            .Get<List<Client>>();

        //override values if this is getting run as a service from aspire
        foreach (var client in clients)
        {
            var aspireServiceAddress = builder.Configuration[$"services:{client.Name}:http:0"];

            if (!string.IsNullOrWhiteSpace(aspireServiceAddress))
            {
                client.Url = aspireServiceAddress;
            }
            else
            {
                if (builder.Configuration["ASPNETCORE_ENVIRONMENT"] != "Development")
                {
                    client.Url = $"http://{client.Name}";
                }
            }
        }

        builder.Services.Configure<Clients>(options =>
        {
            options.ClientList = clients;
        });

        builder.Services.AddTransient<FeedsClient>();
        builder.Services.AddHttpClient<FeedsClient>(o =>
        {
            o.Timeout = TimeSpan.FromSeconds(15);
        });

        var app = builder.Build();

        app.UseWebDefaultsWithOpenApi();

        app.MapGet("/forecast", async (FeedsClient client, ILogger<Program> logger, CancellationToken cancellationToken) =>
        {

            var result = await client.GetWeatherForecast(cancellationToken);

            if (result.IsFailed)
            {
                logger.LogError("Failed to get data from the client. {Errors}", result.GetErrors());

                Results.Problem(result.GetErrors(), statusCode: (int)HttpStatusCode.InternalServerError);
            }

            return Results.Ok(result.Value);
        }).WithName("GetWeatherForecast");

        app.MapGet("/report", async (FeedsClient client, ILogger<Program> logger, CancellationToken cancellationToken) =>
        {
            var result = await client.GetWeatherReport(cancellationToken);

            if (result.IsFailed)
            {
                logger.LogError("Failed to get data from the client. {Errors}", result.GetErrors());

                Results.Problem(result.GetErrors(), statusCode: (int)HttpStatusCode.InternalServerError);
            }

            return Results.Ok(result.Value);
        }).WithName("GetWeatherReport");

        app.MapGet("/conf", (HttpContext httpContext, IOptions<Clients> clients) =>
        {
            return Results.Ok(clients.Value.ClientList);
        })
        .WithName("Configs");

        app.Run();
    }
}
