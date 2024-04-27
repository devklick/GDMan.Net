using System.Text;

using Semver;

using GDMan.Core.Models;
using GDMan.Core.Infrastructure;

namespace GDMan.Core.Services.FileSystem;

public class FS(ConsoleLogger logger, GDManBinDirectory bin, GDManVersionsDirectory versions)
{
    public static readonly KnownPaths Paths = new();

    private readonly ConsoleLogger _logger = logger;
    public GDManBinDirectory GDManBinDir { get; } = bin;
    public GDManVersionsDirectory GodotVersionsDir { get; } = versions;

    public void SetActive(GodotVersionDirectory version)
        => GDManBinDir.CreateOrUpdateGodotLink(version.ExecutablePath);

    public string GenerateVersionName(SemVersion version, Platform platform, Architecture architecture, Flavour flavour)
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
    public string GenerateAssetName(Platform platform, Architecture architecture, Flavour flavour)
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
}