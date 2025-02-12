
using Confluent.Kafka;
using Extensions.Kafka;
using Models;

using static Constants.Constants;

namespace Microservice.Two.Converter;

internal class ConverterHostedService(IConsumer<string, WeatherForecast> _consumer, ILogger<ConverterHostedService> logger, IProducer<string, AggregatedWeatherForecast> producer) : IHostedService
{
    public async Task ProcessMessage(ConsumeResult<string, WeatherForecast> deliveryResult, CancellationToken cancellationToken)
    {
        var city = deliveryResult.Message.Key;
        var weatherForecast = deliveryResult.Message.Value;

        if (string.IsNullOrWhiteSpace(city))
        {
            logger.LogWarning("Received empty city for weather forecast.");
            return;
        }

        if (weatherForecast is null)
        {
            logger.LogWarning("Received null weather forecast for city {City}.", city);
            return;
        }

        await producer.ProduceAsync(TopicNames.TwoConverterAggregator,
            new Message<string, AggregatedWeatherForecast>
            {
                Key = deliveryResult.Message.Key,
                Value = new AggregatedWeatherForecast
                (
                    FeedProvider.FeedTwo,
                    deliveryResult.Message.Key,
                    weatherForecast.Date,
                    weatherForecast.Temperature,
                    weatherForecast.Summary
                )
            }, cancellationToken);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(TopicNames.TwoReceiverConverter);
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
}
