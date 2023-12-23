using System.Text.Json.Serialization;

namespace GDMan.Core.Models.Github;

public class Error
{
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("documentation_url")]
    public required string DocumentationUrl { get; set; }
}