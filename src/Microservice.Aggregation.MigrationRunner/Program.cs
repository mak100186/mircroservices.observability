
using Aggregation.Persistence;

using Microservices.Observability.ServiceDefaults;
using static Constants.Constants;

namespace Microservice.Aggregation.MigrationRunner;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.Services.AddHostedService<MigrationsRunner>();

        builder.AddNpgsqlDbContext<AggregationContext>(Postgres.ConnectionName);

        var app = builder.Build();

        app.Run();
    }
}
