using System.ComponentModel.DataAnnotations;

namespace Models;

public class ClientOptions
{
    public const string ClientsSectionName = "ClientOptions";

    [Required]
    public string Name { get; set; }

    [Required]
    [DataType(DataType.Url)]
    public string BaseUrl { get; set; }

    [Range(0, 10_000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public int PollingIntervalMs { get; set; }
}