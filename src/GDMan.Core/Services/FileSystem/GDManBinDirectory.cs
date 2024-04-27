using GDMan.Core.Infrastructure;

namespace GDMan.Core.Services.FileSystem;

/// <summary>
/// Class representing the `.gdman/bin` folder.
/// Encapsulates the logic to be executed against this directory.
/// </summary>
public class GDManBinDirectory
{
    public string Path { get; }
    public string GodotLinkPath { get; }
    private readonly ConsoleLogger _logger;

    public GDManBinDirectory(KnownPaths paths, ConsoleLogger logger)
    {
        _logger = logger;
        Path = paths.Bin;
        GodotLinkPath = System.IO.Path.Combine(Path, "godot");
        Directory.CreateDirectory(Path);
    }

    public void CreateOrUpdateGodotLink(string targetPath)
    {
        // Delete the current symlink if it exists
        if (File.Exists(GodotLinkPath)) File.Delete(GodotLinkPath);

        // Create the symlink pointing to the exe file
        File.CreateSymbolicLink(GodotLinkPath, targetPath);
    }
}