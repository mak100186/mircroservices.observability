
using Aggregation.Persistence;

using Microservices.Observability.ServiceDefaults;

namespace Microservice.Aggregation.MigrationRunner;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();
        builder.Services.AddHostedService<Worker>();

        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

        builder.AddNpgsqlDbContext<AggregationContext>("postgresdb");

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.Run();
    }
}
