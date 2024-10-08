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
    [GeneratedRegex(@"^(?<major>0|[1-9]\d*)(\.(?<minor>0|[1-9]\d*))?(\.(?<patch>0|[1-9]\d*))?(?:-(?<pre>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<meta>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$")]
    private static partial Regex SemverRegex();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text">The text that may or may not contain a <see cref="SemanticVersioning.Version"</param>
    /// <param name="ignoredPreReleases">
    /// An array of values which, if found in the pre-release section of a semver string, should not be treated as a a pre-release.
    /// 
    /// For example, if the `text` is `4.2.2-stable` and ignoredPreReleased is `["stable"]`, the output Version wil be for just "4.2.2".
    /// </param>
    /// <param name="version">An object representing the version, if one was found</param>
    /// <returns></returns>
    public static bool TryParseVersion(string text, string[] ignoredPreReleases, [NotNullWhen(true)] out SemanticVersioning.Version? version)
    {
        var match = SemverRegex().Match(text);

        if (match.Success)
        {
            // major is required
            var maj = match.Groups["major"].Value.IntOrNull();
            // minor is optional and substituted with 0
            // e.g. version 1 = 1.0.0
            var min = match.Groups["minor"].Value.IntOrNull() ?? 0;
            // minor is optional and substituted with 0
            // e.g. version 1.1 = 1.1.0
            var patch = match.Groups["patch"].Value.IntOrNull() ?? 0;
            var pre = match.Groups["pre"].Value.NullIfEmpty();
            var meta = match.Groups["meta"].Value.NullIfEmpty();

            var str = string.Join('.', new List<int?>([maj, min, patch]).Where(x => x != null));

            if (pre != null && (!ignoredPreReleases?.Contains(pre) ?? false))
            {
                str += '-' + pre;
            }

            if (meta != null)
            {
                str += '+' + meta;
            }

            if (SemanticVersioning.Version.TryParse(str, out version))
            {
                return true;
            }
        }

        version = null;
        return false;
    }
}