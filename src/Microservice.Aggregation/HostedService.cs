using Aggregation.Persistence;

using Confluent.Kafka;
using Extensions.Kafka;
using Models;

using static Constants.Constants;

namespace Microservice.Aggregation;
internal sealed class HostedService2(ILogger<HostedService> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("HostedService2 started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
}
internal sealed class HostedService(IConsumer<string, AggregatedWeatherForecast> consumer, IServiceProvider serviceProvider, ILogger<HostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("HostedService started");
        consumer.Subscribe([TopicNames.OneConverterAggregator, TopicNames.TwoConverterAggregator]);

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

    public async Task ProcessMessage(ConsumeResult<string, AggregatedWeatherForecast> deliveryResult, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AggregationContext>();

        var aggregatedWeatherForecast = deliveryResult.Message.Value;

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
