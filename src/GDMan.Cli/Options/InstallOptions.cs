using GDMan.Cli.Attributes;
using GDMan.Core.Helpers;
using GDMan.Core.Models;
using GDMan.Core.Services.FileSystem;

using Semver;

namespace GDMan.Cli.Options;

[Command("install", "i", "Installs the specified version of Godot")]
public class InstallOptions : BaseOptions
{
    [Option("latest", "l", "Whether or not the latest version should be fetched. "
        + "If used in conjunction with the Version argument and multiple matching "
        + "versions are found, the latest of the matches will be used. ", OptionDataType.Boolean, isFlag: true)]
    public bool Latest { get; set; } = false;

    [Option("version", "v", "The version to use, e.g. 1.2.3. Any valid semver range is supported", OptionDataType.String)]
    public SemVersionRange? Version { get; set; } = null;

    [Option("platform", "p", "The platform or operating system to find a version for", OptionDataType.Enum)]
    public Platform Platform { get; set; } = PlatformHelper.FromEnvVar() ?? PlatformHelper.FromSystem();

    [Option("architecture", "a", "The system architecture to find a version for", OptionDataType.Enum)]
    public Architecture Architecture { get; set; } = ArchitectureHelper.FromEnvVar() ?? ArchitectureHelper.FromSystem();

    [Option("flavour", "f", "The \"flavour\" (for lack of a better name) of version to use", OptionDataType.Enum)]
    public Flavour Flavour { get; set; } = FlavourHelper.FromEnvVar() ?? Flavour.Standard;

    [Option("directory", "d", "The directory where the downloaded version should be installed", OptionDataType.String)]
    public string? Directory { get; set; } = KnownPaths.GDManVersionsPath;

    public override OptionValidation Validate()
    {
        if (Version == null && !Latest)
        {
            return OptionValidation.Failed("Either --version or --latest must be provided");
        }

        if (Platform == Platform.Windows && (Architecture == Architecture.Arm32 || Architecture == Architecture.Arm64))
        {
            return OptionValidation.Failed($"Architecture {Architecture} not supported on Windows platform");
        }

        if (Platform == Platform.Linux && (Version?.Any(v => v.Start.Major < 4) ?? false))
        {
            return OptionValidation.Failed("GDMan does not support Godot version < 4 on Linux");
        }

        return OptionValidation.Success();
    }
}