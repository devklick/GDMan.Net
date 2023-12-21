using System.Diagnostics.CodeAnalysis;

using Semver;

namespace GDMan.Core.Extensions;

public static class SemVersionRangeExtensions
{
    public static bool IsExactVersion(this SemVersionRange range, [NotNullWhen(true)] out SemVersion? version)
    {
        if (range.Count == 1 && SemVersion.TryParse(range[0].ToString(), SemVersionStyles.Any, out version))
        {
            return true;
        }

        version = null;
        return false;
    }
}