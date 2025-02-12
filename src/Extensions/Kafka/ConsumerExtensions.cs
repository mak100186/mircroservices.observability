using Confluent.Kafka;

namespace Extensions.Kafka;

public static class ConsumerExtensions
{
    public static IList<ConsumeResult<TKey, TVal>> ConsumeBatch<TKey, TVal>(this IConsumer<TKey, TVal> consumer, TimeSpan maxWaitTime, int maxBatchSize, CancellationToken cts)
    {
        var waitBudgetRemaining = maxWaitTime;
        var deadline = DateTime.UtcNow + waitBudgetRemaining;
        var res = new List<ConsumeResult<TKey, TVal>>();
        var resSize = 0;

        while (waitBudgetRemaining > TimeSpan.Zero && DateTime.UtcNow < deadline && resSize < maxBatchSize)
        {
            cts.ThrowIfCancellationRequested();

            var msg = consumer.Consume(waitBudgetRemaining);

            if (msg != null && !msg.IsPartitionEOF)
            {
                res.Add(msg);
                resSize++;
            }

            waitBudgetRemaining = deadline - DateTime.UtcNow;
        }

        return res;
    }
}
