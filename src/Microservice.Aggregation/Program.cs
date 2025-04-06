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

        builder.AddNpgsqlDbContext<AggregationContext>(Postgres.ConnectionName);

        builder.AddKeyedKafkaConsumer<string, AggregatedWeatherForecast>(Kafka.SubscriberOne, options =>
        {
            options.Config.BootstrapServers = builder.Configuration.GetConnectionString(Kafka.ConnectionName);
            options.Config.GroupId = $"Microservice.Aggregator-{nameof(ClusterOneSubscriber)}";
            options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
            options.Config.EnableAutoCommit = true;
            options.Config.AutoCommitIntervalMs = 5000;
        }, builder => builder.SetValueDeserializer(new KafkaMessageDeserializer<AggregatedWeatherForecast>()));

        builder.AddKeyedKafkaConsumer<string, AggregatedWeatherForecast>(Kafka.SubscriberTwo, options =>
        {
            options.Config.BootstrapServers = builder.Configuration.GetConnectionString(Kafka.ConnectionName);
            options.Config.GroupId = $"Microservice.Aggregator-{nameof(ClusterTwoSubscriber)}";
            options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
            options.Config.EnableAutoCommit = true;
            options.Config.AutoCommitIntervalMs = 5000;
        }, builder => builder.SetValueDeserializer(new KafkaMessageDeserializer<AggregatedWeatherForecast>()));

        builder.AddKeyedKafkaConsumer<string, AggregatedWeatherForecast>(Kafka.SubscriberThree, options =>
        {
            options.Config.BootstrapServers = builder.Configuration.GetConnectionString(Kafka.ConnectionName);
            options.Config.GroupId = $"Microservice.Aggregator-{nameof(ClusterThreeSubscriber)}";
            options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
            options.Config.EnableAutoCommit = true;
            options.Config.AutoCommitIntervalMs = 5000;
        }, builder => builder.SetValueDeserializer(new KafkaMessageDeserializer<AggregatedWeatherForecast>()));

        builder.Services.Configure<HostOptions>(options =>
        {
            options.ServicesStopConcurrently = options.ServicesStartConcurrently = true;
        });

        builder.Services.AddHostedService<ClusterOneSubscriber>();
        builder.Services.AddHostedService<ClusterTwoSubscriber>();
        builder.Services.AddHostedService<ClusterThreeSubscriber>();

        var app = builder.Build();

        app.UseWebDefaults();

        app.Run();
    }
}
