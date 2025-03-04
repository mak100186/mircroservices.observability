
using Extensions.Kafka;
using Microservice.Three.Receiver.Client;
using Microservices.Observability.ServiceDefaults;
using Models;
using static Constants.Constants;

namespace Microservice.Three.Receiver;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        builder.AddKafkaProducer<string, WeatherForecast>(Kafka.ConnectionName, config =>
        {
            config.SetValueSerializer(new KafkaMessageSerializer<WeatherForecast>());
        });

        builder.Services.AddOptions<OpenWeatherClientOptions>()
            .Bind(builder.Configuration.GetSection(ClientOptions.ClientsSectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<FeedOpenWeatherClient>();
        builder.Services.AddHttpClient<FeedOpenWeatherClient>();

        builder.Services.AddHostedService<PollingOpenWeatherHostedService>();

        var app = builder.Build();

        app.UseWebDefaults();

        app.Run();
    }
}
