
using System.Diagnostics;

using Aggregation.Persistence;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry.Trace;

namespace Microservice.Aggregation.MigrationRunner;

public class MigrationsRunner(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new(nameof(MigrationsRunner));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AggregationContext>();

            await RunMigrationAsync(dbContext, stoppingToken);
            //await SeedDataAsync(dbContext, stoppingToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(AggregationContext dbContext, CancellationToken cancellationToken) =>
        await dbContext.Database.MigrateAsync(cancellationToken);

    //private static async Task SeedDataAsync(AggregationContext dbContext, CancellationToken cancellationToken)
    //{
    //    var firstForecast = new WeatherForecastModel
    //    {
    //        City = "Seattle",
    //        FeedProvider = FeedProvider.FeedOne,
    //        Date = new DateOnly(2021, 10, 1),
    //        Temperature = new Temperature(20, TemperatureUnit.Celsius),
    //        Summary = "Cloudy"
    //    };

    //    await dbContext.WeatherForecasts.AddAsync(firstForecast, cancellationToken);
    //    await dbContext.SaveChangesAsync(cancellationToken);
    //}
}
