using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Extensions;

public static class SerializerOptions
{
    public static JsonSerializerOptions DefaultSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new DateOnlyConverter(), new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        WriteIndented = true,
    };
}

internal class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        DateOnly.ParseExact(reader.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
