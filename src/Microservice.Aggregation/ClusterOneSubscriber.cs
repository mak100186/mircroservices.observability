using Confluent.Kafka;
using Extensions.Kafka;
using Models;

using static Constants.Constants;

namespace Microservice.Aggregation;
internal sealed class ClusterOneSubscriber([FromKeyedServices(Kafka.SubscriberOne)] IConsumer<string, AggregatedWeatherForecast> consumer, IServiceProvider serviceProvider, ILogger<ClusterOneSubscriber> logger)
    : BaseHostedService(serviceProvider), IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(ClusterOneSubscriber)} started");
        consumer.Subscribe(TopicNames.OneConverterAggregator);

        try
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    var consumedBatch = consumer.ConsumeBatch(TimeSpan.FromSeconds(1), 10, cancellationToken);

                    foreach (var consumeResult in consumedBatch)
                    {
                        logger.LogInformation("RX: {TopicPartitionOffset}: {Value}", consumeResult.TopicPartitionOffset, consumeResult.Message.Value);
                        if (consumeResult.IsPartitionEOF)
                        {
                            logger.LogInformation("EOF: {Topic}, {Partition}, {Offset}.", consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);
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
            consumer.Close();
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        consumer.Close();
        await Task.CompletedTask;
    }

}
