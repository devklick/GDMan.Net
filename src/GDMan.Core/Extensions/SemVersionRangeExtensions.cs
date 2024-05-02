using System.Diagnostics.CodeAnalysis;

using SemanticVersioning;

namespace GDMan.Core.Extensions;

public static class SemVersionRangeExtensions
{
    public static bool IsExactVersion(this SemanticVersioning.Range range, [NotNullWhen(true)] out SemanticVersioning.Version? version)
    {
        // if (range.Count == 1 && SemanticVersioning.Version.TryParse(range[0].ToString(), SemVersionStyles.Any, out version))
        // {
        //     return true;
        // }

        version = null;
        return false;
    }
}