using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GDMan.Core.Models;

public partial class SemVer(SemVerPart major, SemVerPart minor, SemVerPart patch, SemVerPart suffix)
{
    public SemVerPart Major { get; } = major;
    public SemVerPart Minor { get; } = minor;
    public SemVerPart Patch { get; } = patch;
    public SemVerPart Suffix { get; } = suffix;

    public static SemVer Parse(string? version)
    {
        /*
        Expect:
            At least major
            + optional minor or wildcard
            + optional patch or wildcard (only when previous provided)
            + optional suffix or wildcard (only when previous provided)
        
        Valid examples:
            1
            1.2
            1.2.3
            1.2.3-alpha
            1.*
            1.2.*
            1.2.3-*
            1.2.*-alpha
            1.*.*-alpha
        */
        // TODO: Write test cases for this

        if (string.IsNullOrEmpty(version))
            throw new ArgumentNullException(nameof(version));

        var parts = version.Split('-');
        if (parts.Length > 2) throw new FormatException();

        var (major, minor, patch) = ParseVersion(parts.First());
        var suffix = ParseSuffix(parts.Length == 2 ? parts.Last() : null);

        return new SemVer(major, minor, patch, suffix);
    }

    public bool IsMatch(SemVer other)
        => Major.IsMatch(other.Major)
            && Minor.IsMatch(other.Minor)
            && Patch.IsMatch(other.Patch)
            && Suffix.IsMatch(other.Suffix);

    public static bool TryParse(string? version, [NotNullWhen(true)] out SemVer? semVer)
    {
        try
        {
            semVer = Parse(version);
            return true;
        }
        catch
        {
            semVer = null;
            return false;
        }
    }

    private static (SemVerPart Major, SemVerPart Minor, SemVerPart Patch) ParseVersion(string version)
    {
        var parts = version.Split('.');
        if (parts.Length > 3) throw new FormatException("Invalid version");

        var major = new SemVerPart(SemVerPartType.Major, parts[0]);
        var minor = new SemVerPart(SemVerPartType.Minor, parts.ElementAtOrDefault(1) ?? "*");
        var patch = new SemVerPart(SemVerPartType.Patch, parts.ElementAtOrDefault(2) ?? "*");

        return (major, minor, patch);
    }

    private static SemVerPart ParseSuffix(string? suffix)
        => new(SemVerPartType.Suffix, suffix ?? "*");
}