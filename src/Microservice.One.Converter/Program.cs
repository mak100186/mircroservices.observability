
using Confluent.Kafka;

using Extensions.Kafka;

using Microservices.Observability.ServiceDefaults;

using Models;

using static Constants.Constants;

namespace Microservice.One.Converter;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.AddKafkaConsumer<string, WeatherForecast>(Kafka.ConnectionName, options =>
        {
            options.Config.GroupId = "Microservice.One.Converter";
            options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
            options.Config.EnableAutoCommit = true;
            options.Config.AutoCommitIntervalMs = 5000;
        }, builder => builder.SetValueDeserializer(new KafkaMessageDeserializer<WeatherForecast>()));

        builder.AddKafkaProducer<string, AggregatedWeatherForecast>(Kafka.ConnectionName, config =>
        {
            config.SetValueSerializer(new KafkaMessageSerializer<AggregatedWeatherForecast>());
        });

        builder.Services.AddHostedService<ConverterHostedService>();

        var app = builder.Build();

        app.UseWebDefaults();

        app.Run();
    }
}
