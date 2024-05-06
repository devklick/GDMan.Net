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

    public static Platform FromSystem() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Platform.Windows
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Platform.Linux
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Platform.MacOS
            : throw new Exception("Unsupported operating system");
}