using System.ComponentModel;

using Semver;

using GDMan.Core.Models;
using GDMan.Core.Services;



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
    [Description("The directory the downloaded version should be installed")]
    public string? Directory { get; set; }

    public static CliArgValues Default => new()
    {
        Version = null,
        Latest = false,
        Platform = GetCurrentPlatform(),
        Architecture = GetCurrentArchitecture(),
        Flavour = Flavour.Standard,
        Directory = GetGodotInstallDirectory()
    };

    private static Platform GetCurrentPlatform()
        => PlatformHelper.FromEnvVar() ?? PlatformHelper.FromSystem();

    private static Architecture GetCurrentArchitecture()
        => ArchitectureHelper.FromEnvVar() ?? ArchitectureHelper.FromSystem();

    private static string GetGodotInstallDirectory()
        => FileSystemService.GDManDirectory;

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

        return CliArgValidation.Success();
    }
}