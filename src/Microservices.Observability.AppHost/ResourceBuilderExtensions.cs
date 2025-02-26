using System.Diagnostics;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microservices.Observability.AppHost;

internal static class ResourceBuilderExtensions
{
    public static IResourceBuilder<T> WithScalar<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints =>
        builder.WithOpenApiDocs("scalar-docs", "Scalar UI Docs", "scalar/v1");

    public static IResourceBuilder<T> WithReDoc<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints =>
        builder.WithOpenApiDocs("redoc-docs", "ReDoc UI Docs", "api-docs");

    public static IResourceBuilder<T> WithSwaggerUI<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints =>
        builder.WithOpenApiDocs("swagger-iu-docs", "Swagger UI Docs", "swagger");

    private static IResourceBuilder<T> WithOpenApiDocs<T>(this IResourceBuilder<T> builder, string name, string displayName, string openApiUiPath) where T : IResourceWithEndpoints =>
        builder.WithCommand(name, displayName, executeCommand: async _ =>
        {
            try
            {
                var endpoint = builder.GetEndpoint("https");

                var url = $"{endpoint.Url}/{openApiUiPath}";

                Process.Start(new ProcessStartInfo(url)
                {
                    UseShellExecute = true
                });

                await Task.CompletedTask;

                return new ExecuteCommandResult { Success = true };
            }
            catch (Exception e)
            {

                return new ExecuteCommandResult { Success = false, ErrorMessage = e.Message };
            }
        },
        updateState: context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy ? ResourceCommandState.Enabled : ResourceCommandState.Disabled,
        iconName: "Document",
        iconVariant: IconVariant.Filled);
}
