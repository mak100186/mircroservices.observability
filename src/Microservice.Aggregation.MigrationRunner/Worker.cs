
using System.Diagnostics;

using Aggregation.Persistence;

using Microsoft.EntityFrameworkCore;

using Models;

using OpenTelemetry.Trace;

namespace Microservice.Aggregation.MigrationRunner;

public class Worker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AggregationContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
            //await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(AggregationContext dbContext, CancellationToken cancellationToken)
    {
        // Run migration in a transaction to avoid partial migration if it fails.
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private static async Task SeedDataAsync(AggregationContext dbContext, CancellationToken cancellationToken)
    {
        var firstForecast = new WeatherForecastModel
        {
            City = "Seattle",
            FeedProvider = FeedProvider.FeedOne,
            Date = new DateOnly(2021, 10, 1),
            Temperature = new Temperature(20, TemperatureUnit.Celsius),
            Summary = "Cloudy"
        };

        await dbContext.WeatherForecasts.AddAsync(firstForecast, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
