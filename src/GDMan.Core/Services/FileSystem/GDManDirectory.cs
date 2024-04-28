using System.Text;

using Semver;

using GDMan.Core.Models;
using GDMan.Core.Infrastructure;

namespace GDMan.Core.Services.FileSystem;

public class GDManDirectory(ConsoleLogger logger, GDManVersionsDirectory versions)
{
    public string Path { get; } = KnownPaths.GDManPath;

    /// <summary>
    /// Whether or not the symlink that points to the currently-active
    /// version of Godot actually exists. 
    /// 
    /// This will generally be false if the install command has not yet been executed.
    /// </summary>
    public static bool GodotLinkExists => File.Exists(KnownPaths.GodotLinkPath);

    /// <summary>
    /// The version of Godot that the symlink points to (if it exists).
    /// 
    /// This will generally only be null when <see cref="GodotLinkExists"/> is false.
    /// </summary>
    public static string? GodotLinkTargetVersion => GodotLinkExists
        ? new DirectoryInfo(new FileInfo(KnownPaths.GodotLinkPath).LinkTarget!).Name
        : null;

    /// <summary>
    /// An object representing the directory where versions of Godot are installed to.
    /// </summary>
    public GDManVersionsDirectory GDManVersionsDirectory { get; } = versions;

    private readonly ConsoleLogger _logger = logger;

    /// <summary>
    /// Sets the specified version as active, 
    /// updating the <see cref="KnownPaths.GodotLinkPath"/> to point to the specified version.
    /// </summary>
    /// <param name="version"></param>
    public static void SetActive(GodotVersionDirectory version)
        => CreateOrUpdateGodotLink(version.ExecutablePath);

    /// <summary>
    /// Generates the name that the published Godot version is expected to match
    /// based on the specified input parameters. 
    /// 
    /// This works by reverse engineering how the Godot versions are named, so 
    /// could break at any time if Godot changes their naming convention.
    /// </summary>
    public static string GenerateVersionName(SemVersion version, Platform platform, Architecture architecture, Flavour flavour)
    {
        var sb = new StringBuilder();
        sb.Append("Godot_v");
        sb.Append(version.ToString());
        sb.Append('_');

        return sb.Append(GenerateAssetName(platform, architecture, flavour)).ToString();
    }

    /// <summary>
    /// Generates the expected name of the asset based on the provided parameters. 
    /// This should match the name of the assets as per the <see cref="https://github.com/godotengine/godot/releases/">Godot repo</see>, 
    /// excluding the application name "Godot" and excluding the version, e.g. v4.2.1-stable.
    /// 
    /// Note that this will not for for versions < 4 for the Linux platform. 
    /// The assets appear to follow a different naming convention.
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="architecture"></param>
    /// <param name="flavour"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception> <summary>
    /// 
    /// </summary>
    /// <param name="platform"></param>
    /// <param name="architecture"></param>
    /// <param name="flavour"></param>
    /// <returns></returns>
    public static string GenerateAssetName(Platform platform, Architecture architecture, Flavour flavour)
    {
        var sb = new StringBuilder();
        if (flavour == Flavour.Mono) sb.Append("mono_");

        if (platform == Platform.Windows)
        {
            sb.Append("win");
            if (architecture == Architecture.X64) sb.Append("64");
            else if (architecture == Architecture.X86) sb.Append("32");
            else throw new InvalidOperationException($"Architecture {architecture} not supported on {platform} platform");

            if (flavour != Flavour.Mono) sb.Append(".exe");
        }
        else if (platform == Platform.Linux)
        {
            sb.Append("linux");
            sb.Append(flavour == Flavour.Mono ? '_' : '.');

            // TODO: This will almost definitely need work
            if (architecture == Architecture.Arm32) sb.Append("arm32");
            else if (architecture == Architecture.Arm64) sb.Append("arm64");
            else if (architecture == Architecture.X64) sb.Append("x86_64");
            else if (architecture == Architecture.X86) sb.Append("x86_32");
        }
        else if (platform == Platform.MacOS)
        {
            sb.Append("macos.universal");
        }
        else throw new InvalidOperationException("Unsupported platform");

        return sb.ToString();
    }

    /// <summary>
    /// Creates a symlink at <see cref="KnownPaths.GodotLinkPath"/> pointing to 
    /// the specified <paramref name="targetPath"/> if it does not yet exist, 
    /// or updates it if it already exists.
    /// </summary>
    public static void CreateOrUpdateGodotLink(string targetPath)
    {
        // Delete the current symlink if it exists
        if (File.Exists(KnownPaths.GodotLinkPath)) File.Delete(KnownPaths.GodotLinkPath);

        // Create the symlink pointing to the exe file
        File.CreateSymbolicLink(KnownPaths.GodotLinkPath, targetPath);
    }
}