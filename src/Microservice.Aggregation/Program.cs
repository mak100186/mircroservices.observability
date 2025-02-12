using Aggregation.Persistence;

using Confluent.Kafka;

using Extensions.Kafka;

using Microservices.Observability.ServiceDefaults;

using Models;

using static Constants.Constants;

namespace Microservice.Aggregation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.AddNpgsqlDbContext<AggregationContext>("postgresdb");

        builder.AddKafkaConsumer<string, AggregatedWeatherForecast>(Kafka.ConnectionName, options =>
        {
            options.Config.GroupId = "Microservice.Aggregator";
            options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
            options.Config.EnableAutoCommit = true;
            options.Config.AutoCommitIntervalMs = 5000;
        }, builder => builder.SetValueDeserializer(new KafkaMessageDeserializer<AggregatedWeatherForecast>()));

        builder.Services.AddHostedService<ConverterHostedService>();

        var app = builder.Build();

        app.UseWebDefaults();

        app.Run();
    }
}
