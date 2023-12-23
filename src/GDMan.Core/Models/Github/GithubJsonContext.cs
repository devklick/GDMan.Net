using System.Text.Json.Serialization;

namespace GDMan.Core.Models.Github;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IEnumerable<Release>))]
[JsonSerializable(typeof(Error))]
internal partial class GithubJsonContext : JsonSerializerContext
{ }