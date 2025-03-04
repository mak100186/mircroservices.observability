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

    [Range(0, 100_000_000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
    public long PollingIntervalMs { get; set; }
}

public class Client
{
    public const string ClientsSectionName = "Clients";
    public string Name { get; set; }
    public string Url { get; set; }
}

public class Clients
{
    public List<Client> ClientList { get; set; }
}
