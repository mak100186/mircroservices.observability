using Microsoft.Extensions.Configuration;

namespace Microservices.Observability.ServiceDefaults;

public static class ConfigurationExtensions
{
    public static string GetValueOrDefault(this IConfiguration configuration, string key, string defaultValue = "")
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }
}
