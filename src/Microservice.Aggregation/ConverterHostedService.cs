using Aggregation.Persistence;

using Confluent.Kafka;

using Models;

using static Constants.Constants;

namespace Microservice.Aggregation;

internal class ConverterHostedService(IConsumer<string, AggregatedWeatherForecast> _consumer, IServiceProvider serviceProvider, ILogger<ConverterHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Timed Hosted Service running.");

        List<string> topics = [TopicNames.OneConverterAggregator, TopicNames.TwoConverterAggregator];

        // When the timer should have no due-time, then do the work once now.
        await PollClients(topics, cancellationToken);

        using PeriodicTimer timer = new(TimeSpan.FromMilliseconds(2000));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await PollClients(topics, cancellationToken);
            }
        }
        catch (OperationCanceledException e)
        {
            logger.LogInformation(e, "Timed Hosted Service is stopping.");
        }
    }

    private async Task PollClients(List<string> topics, CancellationToken cancellationToken)
    {
        foreach (var topic in topics)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _consumer.Subscribe(TopicNames.OneReceiverConverter);

            try
            {
                while (true)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(cancellationToken);
                        logger.LogInformation("RX: {TopicPartitionOffset}: {Value}", consumeResult.TopicPartitionOffset, consumeResult.Message.Value);

                        if (consumeResult.IsPartitionEOF)
                        {
                            logger.LogInformation("Reached end of topic {Topic}, {Partition}, {Offset}.", consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);

                            continue;
                        }

                        await ProcessMessage(consumeResult, cancellationToken);
                    }
                    catch (ConsumeException e)
                    {
                        logger.LogError(e, "Consume error: {Reason}", e.Error.Reason);
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                logger.LogError(e, "Closing consumer.");
                _consumer.Close();
            }
        }
    }

    public async Task ProcessMessage(ConsumeResult<string, AggregatedWeatherForecast> deliveryResult, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AggregationContext>();

        var aggregatedWeatherForecast = deliveryResult.Message.Value;

        if (aggregatedWeatherForecast is null)
        {
            logger.LogWarning("Received null weather forecast.");
            return;
        }

        if (string.IsNullOrWhiteSpace(aggregatedWeatherForecast.City))
        {
            logger.LogWarning("Received empty city for weather forecast.");
            return;
        }

        var existingWeatherForecast = dbContext.WeatherForecasts
                                        .FirstOrDefault(x =>
                                            x.City == aggregatedWeatherForecast.City &&
                                            x.Date == aggregatedWeatherForecast.Date &&
                                            x.FeedProvider == aggregatedWeatherForecast.FeedProvider);

        if (existingWeatherForecast == null)
        {
            await dbContext.WeatherForecasts.AddAsync(aggregatedWeatherForecast.ToWeatherForecastModel(), cancellationToken);
        }
        else
        {
            existingWeatherForecast.UpdatePropertiesUsing(aggregatedWeatherForecast);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
