using System.Text;
using System.Text.Json;

using Confluent.Kafka;

namespace Extensions.Kafka;

public class KafkaMessageSerializer<T> : ISerializer<T>
{
    public byte[] Serialize(T data, SerializationContext context)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, SerializerOptions.DefaultSerializerOptions));
    }
}
