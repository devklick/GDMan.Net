using System.Runtime.InteropServices;

namespace GDMan.Core.Models;

public class Platform : IEquatable<Platform>
{
    public static readonly Platform Linux = new("Linux", "linux", "lin", "l");
    public static readonly Platform Windows = new("Windows", "windows", "win", "w");
    public static readonly Platform MacOS = new("MacOS", "macos", "mac", "m");

    public string Identifier { get; }
    public IReadOnlyCollection<string> Aliases { get; }

    private Platform(string identifier, params string[] aliases)
    {
        Identifier = identifier;
        Aliases = aliases;
    }

    public static Platform FromSystem() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Windows
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Linux
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? MacOS
            : throw new Exception("Unsupported operating system");

    public bool Equals(Platform? other)
        => other != null && other.Identifier == Identifier;
}