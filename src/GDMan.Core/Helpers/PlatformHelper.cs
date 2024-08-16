using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using GDMan.Core.Config;
using GDMan.Core.Models;

namespace GDMan.Core.Helpers;

public class PlatformHelper : EnumHelper
{
    public static Platform? FromEnvVar()
    {
        var envVar = EnvVars.TargetPlatform.Value;

        if (string.IsNullOrEmpty(envVar)) return null;

        if (TryParse<Platform>(envVar, out var platform)) return platform;

        throw new FormatException($"Invalid value for {EnvVars.TargetPlatform.Name}");
    }

    public static bool TryParse(string value, [NotNullWhen(true)] out Platform? enumMember)
        => EnumHelper.TryParse(value, out enumMember);

    public static Platform Parse(string value)
        => TryParse(value, out var platform)
            ? platform.Value
            : throw new FormatException($"Invalid value for Platform: {value}");

    public static Platform FromSystem() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Platform.Windows
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Platform.Linux
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Platform.MacOS
            : throw new Exception("Unsupported operating system");
}