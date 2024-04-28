using GDMan.Core.Config;

namespace GDMan.Core.Services.FileSystem;

public class KnownPaths
{
    public string GDMan { get; }
    public string Versions { get; }
    public string Bin { get; set; }

    public KnownPaths()
    {
        GDMan = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "gdman");
        Bin = Path.Join(GDMan, "bin");
        Versions = EnvVars.VersionsDir.Value ?? Path.Join(GDMan, "versions");
    }
}