using System.Text.Json.Schema;
using Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microservices.Observability.ServiceDefaults;

public static class SchemaEndpointExtensions
{
    public static void MapSchemaEndpoints(this IEndpointRouteBuilder endpoints, Type[] types)
    {
        foreach (var type in types)
        {
            endpoints.MapGet($"/schema/{type.Name}", async context =>
            {
                var schema = SerializerOptions.DefaultSerializerOptions.GetJsonSchemaAsNode(type);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(schema.ToJsonString(SerializerOptions.DefaultSerializerOptions));
            }).WithName($"GetJsonSchemaFor{type.Name}");
        }
    }
}


public class DefaultEndpointFilter(Type[] additionalTypes) : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        //health endpoint
        {
            var operation = new OpenApiOperation
            {
                Description = "Returns the health status of this service",
                OperationId = "GetHealth"
            };
            operation.Tags.Add(new OpenApiTag { Name = "Health Check" });
            operation.Responses.Add("200", new()
            {
                Description = "OK"
            });

            var pathItem = new OpenApiPathItem();
            pathItem.AddOperation(OperationType.Get, operation);

            document.Paths.Add("/health", pathItem);
        }

        //schema endpoints
        foreach (var type in additionalTypes)
        {
            var operation = new OpenApiOperation
            {
                Description = $"Returns the schema for {type.Name}",
                OperationId = $"GetJsonSchemaFor{type.Name}"
            };
            operation.Tags.Add(new OpenApiTag { Name = "Schemas" });
            operation.Responses.Add("200", new()
            {
                Description = "OK"
            });

            var pathItem = new OpenApiPathItem();
            pathItem.AddOperation(OperationType.Get, operation);

            document.Paths.Add($"/schema/{type.Name}", pathItem);
        }
    }
}

public class DefaultEndpointTransformer(Type[] additionalTypes) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        //health endpoint
        {
            var operation = new OpenApiOperation
            {
                Description = "Returns the health status of this service",
                OperationId = "GetHealth"
            };
            operation.Tags.Add(new OpenApiTag { Name = "Health Check" });
            operation.Responses.Add("200", new()
            {
                Description = "Healthy"
            });

            var pathItem = new OpenApiPathItem();
            pathItem.AddOperation(OperationType.Get, operation);

            document.Paths.Add("/health", pathItem);
        }

        //schema endpoints
        foreach (var type in additionalTypes)
        {
            var operation = new OpenApiOperation
            {
                Description = $"Returns the schema for {type.Name}",
                OperationId = $"GetJsonSchemaFor{type.Name}"
            };
            operation.Tags.Add(new OpenApiTag { Name = "Schemas" });
            operation.Responses.Add("200", new()
            {
                Description = "OK"
            });

            var pathItem = new OpenApiPathItem();
            pathItem.AddOperation(OperationType.Get, operation);

            document.Paths.Add($"/schema/{type.Name}", pathItem);
        }

        await Task.CompletedTask;
    }
}
