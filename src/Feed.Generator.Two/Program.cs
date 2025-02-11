using Microservices.Observability.ServiceDefaults;

using Models;

using Scalar.AspNetCore;

namespace Feed.Generator.Two;

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

        string[] cities =
        [
            "Sydney", "Melbourne", "Brisbane", "Perth", "Adelaide", "Gold Coast", "Canberra", "Newcastle", "Central Coast", "Sunshine Coast"
        ];

        string[] summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        app.MapGet("/weatherforecast", (HttpContext httpContext) =>
        {
            CountryWeatherForecast countryWeatherForecast = new([.. cities.Select(city =>
            {
                WeatherForecast[] forecast = [.. Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        new (Random.Shared.Next(-20, 55), TemperatureUnit.Celsius),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))];
                return new CityWeatherForecast(city, forecast);
            })]);

            return countryWeatherForecast;
        })
        .WithName("GetWeatherForecast");

        app.MapGet("/weatherreport", (HttpContext httpContext) =>
        {
            CountryWeatherForecast countryWeatherForecast = new([.. cities.Select(city =>
            {
                WeatherForecast[] forecast = [.. Enumerable.Range(0, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(-index)),
                        new (Random.Shared.Next(-20, 55), TemperatureUnit.Celsius),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))];
                return new CityWeatherForecast(city, forecast);
            })]);

            return countryWeatherForecast;
        })
        .WithName("GetWeatherReport");

        app.Run();
    }
}
