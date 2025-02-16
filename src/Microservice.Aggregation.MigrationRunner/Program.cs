
using Aggregation.Persistence;

using Microservices.Observability.ServiceDefaults;
using static Constants.Constants;

namespace Microservice.Aggregation.MigrationRunner;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaultsWithOpenApi();

        builder.Services.AddHostedService<MigrationsRunner>();

        //do i need this line? its already getting done in service defaults
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(nameof(MigrationsRunner)));

        builder.AddNpgsqlDbContext<AggregationContext>(Postgres.ConnectionName);

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.Run();
    }
}
