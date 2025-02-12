using Aggregation.Persistence;

using Confluent.Kafka;
using Extensions.Kafka;
using Models;

using static Constants.Constants;

namespace Microservice.Aggregation;

internal class ConverterHostedService(IConsumer<string, AggregatedWeatherForecast> _consumer, IServiceProvider serviceProvider, ILogger<ConverterHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe([TopicNames.OneConverterAggregator, TopicNames.TwoConverterAggregator]);
        try
        {
            while (true)
            {
                try
                {
                    await Task.Delay(1000, cancellationToken);

                    var consumedBatch = _consumer.ConsumeBatch(TimeSpan.FromSeconds(1), 10, cancellationToken);

                    foreach (var consumeResult in consumedBatch)
                    {
                        logger.LogInformation("RX: {TopicPartitionOffset}: {Value}", consumeResult.TopicPartitionOffset, consumeResult.Message.Value);
                        if (consumeResult.IsPartitionEOF)
                        {
                            logger.LogInformation("Reached end of topic {Topic}, {Partition}, {Offset}.", consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);
                            continue;
                        }

                        await ProcessMessage(consumeResult, cancellationToken);

                        cancellationToken.ThrowIfCancellationRequested();
                    }
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
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Close();
        await Task.CompletedTask;
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
