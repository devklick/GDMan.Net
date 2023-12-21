using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

using GDMan.Core.Attributes;
using GDMan.Core.Extensions;

using ProcessArchitecture = System.Runtime.InteropServices.Architecture;

namespace GDMan.Core.Models;

public class ArchitectureHelper
{
    private static readonly string ENV_VAR_NAME = "GDMAN_TARGET_ARCHITECTURE";

    public static Architecture? FromEnvVar()
    {
        var envVar = Environment.GetEnvironmentVariable(ENV_VAR_NAME);

        if (string.IsNullOrEmpty(envVar)) return null;

        if (TryParse(envVar, out var arch)) return arch;

        throw new FormatException($"Invalid value for {ENV_VAR_NAME}");

    }

    public static bool TryParse(string value, [NotNullWhen(true)] out Architecture? architecture)
    {
        foreach (var e in Enum.GetValues(typeof(Architecture)))
        {
            if (e.ToString()?.ToLower() == value.ToLower())
            {
                architecture = (Architecture)e;
                return true;
            }

            var aliases = ((Enum)e).GetAttribute<AliasAttribute>()?.Aliases ?? [];

            if (aliases.Any(alias => alias == e.ToString()))
            {
                architecture = (Architecture)e;
                return true;
            }
        }
        architecture = null;
        return false;
    }

    public static Architecture FromSystem() => RuntimeInformation.ProcessArchitecture switch
    {
        ProcessArchitecture.X86 => Architecture.X86,
        ProcessArchitecture.X64 => Architecture.X64,
        ProcessArchitecture.Arm => Architecture.Arm32,
        ProcessArchitecture.Arm64 => Architecture.Arm32,
        _ => throw new Exception("Unsupported system architecture"),
    };
}