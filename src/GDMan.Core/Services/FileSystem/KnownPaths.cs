namespace GDMan.Core.Services.FileSystem;

/// <summary>
/// The paths that are known to and used by the application.
/// </summary>
public static class KnownPaths
{
    /// <summary>
    /// The path where the application is installed
    /// </summary>
    public static readonly string GDManPath;

    /// <summary>
    /// The path where versions of Godot are installed to
    /// </summary>
    public static readonly string GDManVersionsPath;

    /// <summary>
    /// The path of the symlink that points to the currently-active version of Godot
    /// </summary>
    public static readonly string GodotLinkPath;

    static KnownPaths()
    {
        GDManPath = AppDomain.CurrentDomain.BaseDirectory;
        GDManVersionsPath = Path.Join(GDManPath, "versions");
        GodotLinkPath = Path.Combine(GDManPath, "godot");
    }
}