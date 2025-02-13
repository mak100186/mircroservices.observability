using System.Text;
using System.Text.Json;

using Confluent.Kafka;

namespace Extensions.Kafka;

public class KafkaMessageDeserializer<T> : IDeserializer<T>
{
    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) => JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data), SerializerOptions.DefaultSerializerOptions)!;
}
