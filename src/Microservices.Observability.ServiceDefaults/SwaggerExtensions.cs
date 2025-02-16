using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Microservices.Observability.ServiceDefaults;

public static class SwaggerExtensions
{
    public static TBuilder ConfigureCustomisedSwagger<TBuilder>(this TBuilder builder, Type[] additionalTypes) where TBuilder : IHostApplicationBuilder
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

            c.AddDocumentFilterInstance(new DefaultEndpointFilter(additionalTypes));

            foreach (var name in Directory.GetFiles(AppContext.BaseDirectory, "*.XML", SearchOption.TopDirectoryOnly))
            {
                c.IncludeXmlComments(filePath: name);
            }
        });

        return builder;
    }

    public static void UseCustomisedSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            if (bool.Parse(app.Configuration.GetValueOrDefault("CustomSwaggerUi:Personalised", "false")))
            {
                options.DocumentTitle = app.Configuration["CustomSwaggerUi:DocTitle"];
                options.HeadContent = app.Configuration["CustomSwaggerUi:HeaderImg"] ?? options.HeadContent;
                options.InjectStylesheet(app.Configuration["CustomSwaggerUi:PathCss"]);
            }

            options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
            options.EnableTryItOutByDefault();
            options.DisplayRequestDuration();

        });
    }
}
