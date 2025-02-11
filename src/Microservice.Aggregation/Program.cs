using Aggregation.Persistence;

using Confluent.Kafka;

using Extensions.Kafka;

using Microservices.Observability.ServiceDefaults;

using Microsoft.EntityFrameworkCore;

using Models;

using Scalar.AspNetCore;

using static Constants.Constants;

namespace Microservice.Aggregation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services
            .AddCors(options =>
            {
                options.AddPolicy("AnyOrigin", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod();
                });
            })
            .AddOpenApi();

        builder.AddNpgsqlDbContext<AggregationContext>("postgresdb");

        builder.AddKafkaConsumer<string, AggregatedWeatherForecast>(Kafka.ConnectionName, options =>
        {
            options.Config.GroupId = "Microservice.Aggregator";
            options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
            options.Config.EnableAutoCommit = true;
            options.Config.AutoCommitIntervalMs = 5000;
        }, builder => builder.SetValueDeserializer(new KafkaMessageDeserializer<AggregatedWeatherForecast>()));
        builder.Services.AddHostedService<ConverterHostedService>();

        var app = builder.Build();

        app.UseCors("AnyOrigin");

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "Microservice Aggregation API");
            });

            app.UseReDoc(options =>
            {
                options.SpecUrl = "/openapi/v1.json";
            });

            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.MapGet("/weatherforecast", async (HttpContext httpContext, AggregationContext dbContext, string city, DateOnly date, CancellationToken cancellationToken) =>
        {
            var weatherForecast = await dbContext.WeatherForecasts
                .AsNoTracking()
                .Where(x => x.City == city && x.Date == date)
                .ToListAsync(cancellationToken: cancellationToken);

            if (weatherForecast == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(weatherForecast.ToAggregatedWeatherForecastResponse());
        })
        .WithName("GetWeatherForecast");

        app.Run();
    }
}
