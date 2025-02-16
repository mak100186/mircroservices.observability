using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

using static Constants.Constants;

namespace Microservices.Observability.ServiceDefaults;

public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services
            .AddServiceDiscovery()
            .AddProblemDetails()
            .ConfigureHttpClientDefaults(http =>
            {
                http.ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

                http.AddStandardResilienceHandler();

                http.AddServiceDiscovery();
            })
            .ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddCors(options =>
            {
                options.AddPolicy(Cors.AnyOriginPolicy, builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod();
                });
            })
            .AddHttpLogging(options =>
            {
                options.CombineLogs = true;
                options.LoggingFields = HttpLoggingFields.RequestScheme | HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
            })
            .AddSingleton(TimeProvider.System)
            .AddSingleton<WeatherMetrics>();

        return builder
            .ConfigureOpenTelemetry()
            .AddDefaultHealthChecks();
    }

    public static TBuilder AddServiceDefaultsWithOpenApi<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder
            .ConfigureCustomisedSwagger();

        builder.Services
            .AddOpenApi();

        return builder.AddServiceDefaults();
    }

    public static WebApplication UseWebDefaults(this WebApplication app)
    {
        app.UseExceptionHandler(e =>
        {
            e.Run(async context =>
            {
                var writer = context.RequestServices.GetRequiredService<IProblemDetailsService>();
                var exception = context.Features.GetRequiredFeature<IExceptionHandlerFeature>()!.Error;

                var problem = exception.ToProblemDetails();
                if (app.Configuration.GetValue<bool>("ShowExceptions"))
                {
                    problem.EnrichWithExceptionDetails(exception);
                }
                context.Response.StatusCode = problem.Status ?? 500;

                await writer.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problem
                });
            });
        });

        app.UseHttpLogging();

        app.UseCors(Cors.AnyOriginPolicy);

        app.MapDefaultEndpoints();

        return app;
    }

    public static WebApplication UseWebDefaultsWithOpenApi(this WebApplication app)
    {
        app.UseWebDefaults();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseCustomisedSwagger();

            app.UseReDoc(options =>
            {
                options.SpecUrl = "/openapi/v1.json";
            });

            app.MapScalarApiReference();
        }

        app.UseStaticFiles();

        return app;
    }

}
