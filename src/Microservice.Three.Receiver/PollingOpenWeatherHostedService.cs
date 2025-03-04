using System.ComponentModel.DataAnnotations;
using Confluent.Kafka;

using Extensions;

using FluentResults;
using Microservice.Three.Receiver.Client;
using Microsoft.Extensions.Options;

using Models;

using static Constants.Constants;

namespace Microservice.Three.Receiver;

public sealed class OpenWeatherClientOptions : ClientOptions
{
    [Required]
    public string ApiKey { get; set; }
    [Required]
    public string LocationBaseUrl { get; set; }
}

internal sealed class PollingOpenWeatherHostedService(FeedOpenWeatherClient feedClient, IOptions<OpenWeatherClientOptions> clientOptions, IProducer<string, WeatherForecast> producer, ILogger<PollingOpenWeatherHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Timed Hosted Service running.");

        // When the timer should have no due-time, then do the work once now.
        await PollClient(cancellationToken);

        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(clientOptions.Value.PollingIntervalMs));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await PollClient(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Timed Hosted Service is stopping.");
        }
    }

    private async Task PollClient(CancellationToken cancellationToken)
    {
        var weatherReport = feedClient.GetWeatherReport(cancellationToken);

        var weatherForecast = feedClient.GetWeatherForecast(cancellationToken);

        var results = await Task.WhenAll(weatherReport, weatherForecast);

        var mergedResult = results.Merge();

        if (mergedResult.IsFailed)
        {
            logger.LogError("Failed to get data from the client. {Errors}", mergedResult.GetErrors());

            return;
        }

        var combinedWeathers = mergedResult.Value.SelectMany(x => x.CitiesWeatherForecast).ToList();

        foreach (var cityWeather in combinedWeathers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(cityWeather.City))
            {
                logger.LogWarning("Received empty city for weather forecast.");
                continue;
            }

            foreach (var forecast in cityWeather.Forecast)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //send the data to the next microservice
                await producer.ProduceAsync(TopicNames.ThreeReceiverConverter,
                    new Message<string, WeatherForecast>
                    {
                        Key = cityWeather.City,
                        Value = forecast
                    }, cancellationToken);
            }
        }
    }
}
