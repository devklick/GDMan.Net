using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using ProcessArchitecture = System.Runtime.InteropServices.Architecture;

namespace GDMan.Core.Models;

public class Architecture : IEquatable<Architecture>
{
    public static readonly Architecture Arm32 = new("Arm32");
    public static readonly Architecture Arm64 = new("Arm64");
    public static readonly Architecture X86 = new("X86");
    public static readonly Architecture X64 = new("X64");
    private static readonly IEnumerable<Architecture> All = [Arm32, Arm64, X86, X64];

    private static readonly string ENV_VAR_NAME = "GDMAN_TARGET_ARCHITECTURE";

    public string Identifier { get; }
    public IReadOnlyCollection<string> Aliases;

    private Architecture(string identifier, params string[] aliases)
    {
        Identifier = identifier;
        Aliases = aliases;
    }

    public static Architecture? FromEnvVar()
    {
        var envVar = Environment.GetEnvironmentVariable(ENV_VAR_NAME);

        if (string.IsNullOrEmpty(envVar)) return null;

        if (TryParse(envVar, out var arch)) return arch;

        throw new FormatException($"Invalid value for {ENV_VAR_NAME}");

    }

    public static bool TryParse(string identifier, [NotNullWhen(true)] out Architecture? architecture)
    {
        foreach (var arch in All)
        {
            if (arch.Identifier == identifier || arch.Aliases.Contains(identifier))
            {
                architecture = arch;
                return true;
            }
        }
        architecture = null;
        return false;
    }

    public static Architecture FromSystem() => RuntimeInformation.ProcessArchitecture switch
    {
        ProcessArchitecture.X86 => X86,
        ProcessArchitecture.X64 => X64,
        ProcessArchitecture.Arm => Arm32,
        ProcessArchitecture.Arm64 => Arm64,
        _ => throw new Exception("Unsupported system architecture"),
    };

    public bool Equals(Architecture? other)
        => other != null && other.Identifier == Identifier;
}