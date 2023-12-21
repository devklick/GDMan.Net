using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using GDMan.Core.Attributes;
using GDMan.Core.Extensions;

namespace GDMan.Core.Models;

public class PlatformHelper
{
    private static readonly string ENV_VAR_NAME = "GDMAN_TARGET_PLATFORM";

    public static Platform? FromEnvVar()
    {
        var envVar = Environment.GetEnvironmentVariable(ENV_VAR_NAME);

        if (string.IsNullOrEmpty(envVar)) return null;

        if (TryParse(envVar, out var platform)) return platform;

        throw new FormatException($"Invalid value for {ENV_VAR_NAME}");

    }

    public static bool TryParse(string value, [NotNullWhen(true)] out Platform? platform)
    {
        foreach (var e in Enum.GetValues(typeof(Architecture)))
        {
            if (e.ToString()?.ToLower() == value.ToLower())
            {
                platform = (Platform)e;
                return true;
            }

            var aliases = ((Enum)e).GetAttribute<AliasAttribute>()?.Aliases ?? [];

            if (aliases.Any(alias => alias == e.ToString()))
            {
                platform = (Platform)e;
                return true;
            }
        }
        platform = null;
        return false;
    }

    public static Platform FromSystem() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Platform.Windows
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Platform.Linux
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Platform.MacOS
            : throw new Exception("Unsupported operating system");
}