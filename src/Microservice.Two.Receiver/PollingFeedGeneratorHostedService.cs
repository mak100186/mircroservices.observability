
using Confluent.Kafka;

using Extensions;

using FluentResults;

using Microservice.Two.Receiver.Client;

using Microsoft.Extensions.Options;

using Models;

using static Constants.Constants;

namespace Microservice.Two.Receiver;

internal sealed class PollingFeedGeneratorHostedService(FeedTwoClient feedTwoClient, IOptions<ClientOptions> clientOptions, IProducer<string, WeatherForecast> producer, ILogger<PollingFeedGeneratorHostedService> logger) : BackgroundService
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
        var weatherReport = feedTwoClient.GetWeatherReport(cancellationToken);

        var weatherForecast = feedTwoClient.GetWeatherForecast(cancellationToken);

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
            foreach (var forecast in cityWeather.Forecast)
            {
                //send the data to the next microservice
                await producer.ProduceAsync(TopicNames.TwoReceiverConverter, new Message<string, WeatherForecast>
                {
                    Key = cityWeather.City,
                    Value = forecast
                }, cancellationToken);
            }
        }
    }
}
