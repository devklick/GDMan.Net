using System.Diagnostics.CodeAnalysis;

namespace GDMan.Core.Extensions;

public static class TypeExtensions
{
    public static bool IsNullableEnum(this Type t) => t.IsNullableEnum(out var _);
    public static bool IsNullableEnum(this Type t, [NotNullWhen(true)] out Type? underlyingType)
    {
        underlyingType = Nullable.GetUnderlyingType(t);
        return underlyingType != null && underlyingType.IsEnum;
    }
}