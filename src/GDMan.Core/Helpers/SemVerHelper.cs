using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using GDMan.Core.Extensions;

namespace GDMan.Core.Helpers;

/// <summary>
/// Godot versions do not follow strict semantic versioning rules, 
/// so this helper class is a bridge between Godots versioning and semantic versioning.
/// </summary>
public partial class SemVerHelper
{
    /// <summary>
    /// Modified version of the official regex provided by Semver. 
    /// See <see cref="https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string"/>
    /// The difference is that minor and patch versions are optional in this regex.
    /// </summary>
    [GeneratedRegex(@"^(?<major>0|[1-9]\d*)(\.(?<minor>0|[1-9]\d*))?(\.(?<patch>0|[1-9]\d*))?(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$")]
    private static partial Regex SemverRegex();

    public static bool TryParseVersion(string text, [NotNullWhen(true)] out SemanticVersioning.Version? version)
    {
        var match = SemverRegex().Match(text);

        if (match.Success)
        {
            var maj = match.Groups["major"].Value.IntOrNull();
            var min = match.Groups["minor"].Value.IntOrNull() ?? 0;
            var patch = match.Groups["patch"].Value.IntOrNull() ?? 0;
            var pre = match.Groups["prerelease"].Value.NullIfEmpty();
            var meta = match.Groups["buildmetadata"].Value.NullIfEmpty();
            var str = string.Join('.', new List<int?>([maj, min, patch]).Where(x => x != null));
            if (pre != null) str += '-' + pre;
            if (meta != null) str += '+' + meta;

            if (SemanticVersioning.Version.TryParse(str, out version))
            {
                return true;
            }
        }

        version = null;
        return false;
    }
}