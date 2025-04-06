using Aggregation.Persistence;
using Confluent.Kafka;
using Models;

namespace Microservice.Aggregation;

public abstract class BaseHostedService(IServiceProvider serviceProvider)
{
    protected async Task ProcessMessage(ConsumeResult<string, AggregatedWeatherForecast> deliveryResult, CancellationToken cancellationToken)
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
