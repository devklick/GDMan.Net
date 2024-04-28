using System.Runtime.InteropServices;

using GDMan.Core.Config;

using ProcessArchitecture = System.Runtime.InteropServices.Architecture;
using GDManArchitecture = GDMan.Core.Models.Architecture;

namespace GDMan.Core.Helpers;

public class ArchitectureHelper : EnumHelper
{
    public static GDManArchitecture? FromEnvVar()
    {
        var envVar = EnvVars.TargetArchitecture.Value;

        if (string.IsNullOrEmpty(envVar)) return null;

        if (TryParse<GDManArchitecture>(envVar, out var arch)) return arch;

        throw new FormatException($"Invalid value for {EnvVars.TargetArchitecture.Name}");
    }

    public static GDManArchitecture FromSystem() => RuntimeInformation.ProcessArchitecture switch
    {
        ProcessArchitecture.X86 => GDManArchitecture.X86,
        ProcessArchitecture.X64 => GDManArchitecture.X64,
        ProcessArchitecture.Arm => GDManArchitecture.Arm32,
        ProcessArchitecture.Arm64 => GDManArchitecture.Arm32,
        _ => throw new Exception("Unsupported system architecture"),
    };
}