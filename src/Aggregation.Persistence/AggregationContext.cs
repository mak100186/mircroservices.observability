using System.Text.Json;

using Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Models;

namespace Aggregation.Persistence;

public class AggregationContext(DbContextOptions<AggregationContext> options) : DbContext(options)
{
    public DbSet<WeatherForecastModel> WeatherForecasts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<WeatherForecastModel>()
            .Property(e => e.FeedProvider)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<FeedProvider>(v));

        modelBuilder
            .Entity<WeatherForecastModel>()
            .Property(e => e.Date)
            .HasConversion(new DateOnlyToStringConverter());

        var converter = new TemperatureConverter();
        modelBuilder
            .Entity<WeatherForecastModel>()
            .Property(e => e.Temperature)
            .HasConversion<TemperatureConverter>();
    }
}

public class TemperatureConverter : ValueConverter<Temperature, string>
{
    public TemperatureConverter()
        : base(
            v => JsonSerializer.Serialize(v, SerializerOptions.DefaultSerializerOptions),
            v => JsonSerializer.Deserialize<Temperature>(v, SerializerOptions.DefaultSerializerOptions)!)
    {
    }
}
