using System.Data.Common;
using System.Text;

using GDMan.Core.Models;

using Semver;

namespace GDMan.Core.Services;

/*
Structure:
    .gdman/
        bin/
            godot <- symlink to a specific version
            gdman <- symlink to this apps executable
        versions/ <- The godot versions that have been downloaded
            godot_1.2.3-stable/
            godot_3.4.5-alpha/
        GDMan/
            ...contents of this app
*/

public class FileSystemService
{
    public static readonly string GDManDirectory = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ".gdman"
    );

    public static readonly string GDManVersionDirectory =
        Environment.GetEnvironmentVariable("GDMAN_VERSIONS_DIR")
        ?? Path.Join(GDManDirectory, "versions");

    public static readonly string GDManBinDirectory = Path.Join(
        GDManDirectory,
        "bin"
    );

    public static bool AlreadyInstalled(SemVersionRange range, Platform platform, Architecture architecture, Flavour flavour)
    {
        if (range.Count != 1) return false;

        if (!SemVersion.TryParse(range[0].ToString(), SemVersionStyles.Any, out var version))
            return false;

        var name = GenerateName(version, platform, architecture, flavour);

        return Directory.Exists(Path.Join(GDManVersionDirectory, name));
    }

    public static string GenerateName(SemVersion version, Platform platform, Architecture architecture, Flavour flavour)
    {
        var sb = new StringBuilder();
        sb.Append("Godot_v");
        sb.Append(version.ToString());
        sb.Append('_');

        if (flavour == Flavour.Mono) sb.Append("mono_");

        if (platform == Platform.Windows)
        {
            sb.Append("win");
            sb.Append(architecture == Architecture.X64 ? "64" : "32");

            if (flavour != Flavour.Mono) sb.Append(".exe");
        }
        else if (platform == Platform.Linux)
        {
            sb.Append("linux_");

            // TODO: This will almost definitely need work
            if (architecture == Architecture.Arm32) sb.Append(".arm32");
            else if (architecture == Architecture.Arm64) sb.Append(".arm65");
            else if (architecture == Architecture.X64) sb.Append("_x84_64");
            else if (architecture == Architecture.X86) sb.Append("_x84_32");
        }
        else if (platform == Platform.MacOS)
        {
            sb.Append("macos.universal");
        }
        else throw new InvalidOperationException("Unsupported platform");

        return sb.ToString();
    }
}