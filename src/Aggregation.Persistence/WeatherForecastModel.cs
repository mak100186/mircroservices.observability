using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

using Models;

namespace Aggregation.Persistence;

[Index(nameof(City), nameof(Date), nameof(FeedProvider))]
public class WeatherForecastModel
{
    [Key]
    public Guid Id { get; set; }
    public required string City { get; set; }
    public FeedProvider FeedProvider { get; set; }
    public DateOnly Date { get; set; }
    public required Temperature Temperature { get; set; }
    public string? Summary { get; set; }
}
