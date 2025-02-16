using Extensions.Kafka;

using Microservice.Two.Receiver.Client;

using Microservices.Observability.ServiceDefaults;

using Models;

using static Constants.Constants;

namespace Microservice.Two.Receiver;
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

        builder.Services.AddOptions<ClientOptions>()
            .Bind(builder.Configuration.GetSection(ClientOptions.ClientsSectionName))
            .Configure(options =>
            {
                options.BaseUrl = builder.Configuration["services:feed-generator-two:http:0"]!;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<FeedTwoClient>();
        builder.Services.AddHttpClient<FeedTwoClient>();

        builder.Services.AddHostedService<PollingFeedGeneratorHostedService>();

        var app = builder.Build();

        app.UseWebDefaults();

        app.Run();
    }
}
