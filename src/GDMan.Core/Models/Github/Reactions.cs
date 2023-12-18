using System.Text.Json.Serialization;

namespace GDMan.Core.Models.Github;

public class Reactions
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("+1")]
    public int VoteUp { get; set; }

    [JsonPropertyName("-1")]
    public int VoteDown { get; set; }

    [JsonPropertyName("laugh")]
    public int Laugh { get; set; }

    [JsonPropertyName("hooray")]
    public int Hooray { get; set; }

    [JsonPropertyName("confused")]
    public int Confused { get; set; }

    [JsonPropertyName("heart")]
    public int Heart { get; set; }

    [JsonPropertyName("rocket")]
    public int Rocket { get; set; }

    [JsonPropertyName("eyes")]
    public int Eyes { get; set; }
}