using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

using static Constants.Constants;

namespace Microservices.Observability.ServiceDefaults;

public static class Extensions
{
    public static TBuilder AddWebDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services
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
            .AddOpenApi()
            .AddHttpLogging()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<WeatherMetrics>();

        return builder.AddSwagger();
    }

    public static TBuilder AddSwagger<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var httpsPort = builder.Configuration["HTTPS_PORT"];
        var baseUrl = $"https://localhost:{httpsPort}";

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "Version 1.0.0",
                Title = builder.Configuration.GetValueOrDefault("CustomSwaggerUi:DocTitle", builder.Environment.ApplicationName),
                Description = "Swagger UI Personalized using .Net 9",
                Contact = new OpenApiContact
                {
                    Name = "OpenApi schema",
                    Url = new Uri($"{baseUrl}/openapi/v1.json")
                },
                License = new OpenApiLicense
                {
                    Name = "Swagger schema",
                    Url = new Uri($"{baseUrl}/swagger/v1/swagger.json")
                }
            });

            foreach (var name in Directory.GetFiles(AppContext.BaseDirectory, "*.XML", SearchOption.TopDirectoryOnly))
            {
                c.IncludeXmlComments(filePath: name);
            }
        });

        return builder;
    }

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.ConfigureHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            http.AddStandardResilienceHandler();

            http.AddServiceDiscovery();
        });

        builder.AddWebDefaults();

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(ObservabilityMetrics.MeterName);
            })
            .WithTracing(tracing =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    tracing.SetSampler<AlwaysOnSampler>();
                }

                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    public static WebApplication UseWebDefaults(this WebApplication app)
    {
        app.UseCors("AnyOrigin");

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        return app;
    }

    public static WebApplication UseWebDefaultsWithOpenApi(this WebApplication app, string applicationName)
    {
        app.UseHttpLogging();

        app.UseCors("AnyOrigin");

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger UI Modified");
                if (bool.Parse(string.IsNullOrWhiteSpace(app.Configuration["CustomSwaggerUi:Personalised"]) ? "false" : app.Configuration["CustomSwaggerUi:Personalised"]!))
                {
                    options.DocumentTitle = app.Configuration["CustomSwaggerUi:DocTitle"];
                    options.HeadContent = app.Configuration["CustomSwaggerUi:HeaderImg"] ?? options.HeadContent;
                    options.InjectStylesheet(app.Configuration["CustomSwaggerUi:PathCss"]);
                }

                options.DisplayRequestDuration();

            });

            app.UseReDoc(options =>
            {
                options.SpecUrl = "/openapi/v1.json";
            });

            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        return app;
    }

    private static string GetValueOrDefault(this IConfiguration configuration, string key, string defaultValue = "")
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
