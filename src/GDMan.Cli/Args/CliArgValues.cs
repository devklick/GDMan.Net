using System.ComponentModel;
using System.Runtime.InteropServices;

using Architecture = GDMan.Core.Models.Architecture;
using ProcessArchitecture = System.Runtime.InteropServices.Architecture;

using GDMan.Core.Models;

namespace GDMan.Cli.Args;

public class CliArgValues
{
    [CliArg("latest", "l", isFlag: true)]
    [Description("Whether or not the latest version should be fetched. "
        + "If used in conjunction with the Version argument and multiple matching "
        + "versions are found, the latest of the matches will be used. ")]
    public bool Latest { get; set; }

    [CliArg("version", "v")]
    [Description("The version to use, e.g. 1.2.3")]
    public string? Version { get; set; }

    [CliArg("platform", "p")]
    [Description("The platform or operating system to find a version for")]
    public Platform Platform { get; set; }

    [CliArg("architecture", "a")]
    [Description("The system architecture to find a version for")]
    public Architecture Architecture { get; set; }

    [CliArg("flavour", "f")]
    [Description("The \"flavour\" (for lack of a better name) of version to use")]
    public Flavour Flavour { get; set; }

    public static CliArgValues Default => new()
    {
        Version = null,
        Latest = false,
        Platform = GetCurrentPlatform(),
        Architecture = GetCurrentArchitecture(),
        Flavour = Flavour.Standard
    };

    private static Platform GetCurrentPlatform()
        => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Platform.Windows
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Platform.Linux
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Platform.MacOS
            : throw new Exception("Unsupported operating system");

    private static Architecture GetCurrentArchitecture() => RuntimeInformation.ProcessArchitecture switch
    {
        ProcessArchitecture.X86 => Architecture.X86,
        ProcessArchitecture.X64 => Architecture.X64,
        ProcessArchitecture.Arm => Architecture.Arm32,
        ProcessArchitecture.Arm64 => Architecture.Arm64,
        _ => throw new Exception("Unsupported system architecture"),
    };
}