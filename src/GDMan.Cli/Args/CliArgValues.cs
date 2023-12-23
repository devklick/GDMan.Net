using System.ComponentModel;

using Semver;

using GDMan.Core.Models;
using GDMan.Core.Services;
using GDMan.Core.Helpers;

namespace GDMan.Cli.Args;

public class CliArgValues
{
    [CliArg("latest", "l", CliArgDataType.Boolean, isFlag: true)]
    [Description("Whether or not the latest version should be fetched. "
        + "If used in conjunction with the Version argument and multiple matching "
        + "versions are found, the latest of the matches will be used. ")]
    public bool Latest { get; set; }

    [CliArg("version", "v", CliArgDataType.String)]
    [Description("The version to use, e.g. 1.2.3. Any valid semver range is supported")]
    public SemVersionRange? Version { get; set; }

    [CliArg("platform", "p", CliArgDataType.Enum)]
    [Description("The platform or operating system to find a version for")]
    public required Platform Platform { get; set; }

    [CliArg("architecture", "a", CliArgDataType.Enum)]
    [Description("The system architecture to find a version for")]
    public required Architecture Architecture { get; set; }

    [CliArg("flavour", "f", CliArgDataType.Enum)]
    [Description("The \"flavour\" (for lack of a better name) of version to use")]
    public Flavour Flavour { get; set; }

    [CliArg("directory", "d", CliArgDataType.String)]
    [Description("The directory where the downloaded version should be installed")]
    public string? Directory { get; set; }

    public static CliArgValues Default => new()
    {
        Version = null,
        Latest = false,
        Platform = GetCurrentPlatform(),
        Architecture = GetCurrentArchitecture(),
        Flavour = GetFlavour(),
        Directory = GetGodotVersionsDirectory()
    };

    private static Platform GetCurrentPlatform()
        => PlatformHelper.FromEnvVar() ?? PlatformHelper.FromSystem();

    private static Architecture GetCurrentArchitecture()
        => ArchitectureHelper.FromEnvVar() ?? ArchitectureHelper.FromSystem();

    private static string GetGodotVersionsDirectory()
        => FS.GodotVersionsDir.Path;

    private static Flavour GetFlavour()
        => FlavourHelper.FromEnvVar() ?? Flavour.Standard;

    public CliArgValidation Validate()
    {
        if (Version == null && !Latest)
        {
            return CliArgValidation.Failed("Either --version or --latest must be provided");
        }

        if (Platform == Platform.Windows && (Architecture == Architecture.Arm32 || Architecture == Architecture.Arm64))
        {
            return CliArgValidation.Failed($"Architecture {Architecture} not supported on Windows platform");
        }

        if (Platform == Platform.Linux && (Version?.Any(v => v.Start.Major < 4) ?? false))
        {
            return CliArgValidation.Failed("GDMan does not support Godot version < 4 on Linux");
        }

        return CliArgValidation.Success();
    }
}