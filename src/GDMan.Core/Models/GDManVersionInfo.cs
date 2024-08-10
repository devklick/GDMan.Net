
namespace GDMan.Core.Models;

public class GDManVersionInfo(SemanticVersioning.Version version, string releaseUrl)
{
    public SemanticVersioning.Version Version { get; set; } = version;
    public string ReleaseUrl { get; set; } = releaseUrl;
}