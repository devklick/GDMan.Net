using System.Text.Json.Serialization;

namespace GDMan.Core.Models.Github;

public class Release
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    [JsonPropertyName("assets_url")]
    public required string AssetsUrl { get; set; }

    [JsonPropertyName("upload_url")]
    public required string UploadUrl { get; set; }

    [JsonPropertyName("html_url")]
    public required string HtmlUrl { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("author")]
    public required User Author { get; set; }

    [JsonPropertyName("node_id")]
    public required string NodeId { get; set; }

    [JsonPropertyName("tag_name")]
    public required string TagName { get; set; }

    [JsonPropertyName("target_commitish")]
    public required string TargetCommitish { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("assets")]
    public required List<Asset> Assets { get; set; }

    [JsonPropertyName("tarball_url")]
    public required string TarballUrl { get; set; }

    [JsonPropertyName("zipball_url")]
    public required string ZipballUrl { get; set; }

    [JsonPropertyName("body")]
    public required string Body { get; set; }

    [JsonPropertyName("reactions")]
    public required Reactions Reactions { get; set; }
}