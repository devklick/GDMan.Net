using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace GDMan.Core.Extensions;

public static class EnumExtensions
{
    private static readonly Dictionary<Enum, string?> enumDescriptions = [];

    /// <summary>
    /// Returns description for Enumeration if present, else enumeration name.
    /// </summary>
    public static string? GetDescription(this Enum value)
    {
        if (!enumDescriptions.TryGetValue(value, out var description))
        {
            description = value?.GetAttribute<DisplayAttribute>()?.Description;

            if (value != null)
            {
                enumDescriptions.Add(value, description);
            }
        }

        return description;
    }

    /// <summary>
    /// Gets any attribute from an enum value based on the specified type.
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static TAttribute? GetAttribute<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields)] TAttribute>(this Enum value) where TAttribute : Attribute
        => value.GetAttributes<TAttribute>().SingleOrDefault();

    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2072", Justification = "Cant fix error, seems to work OK for now")]
    public static IEnumerable<TAttribute> GetAttributes<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicFields)] TAttribute>(this Enum value) where TAttribute : Attribute
    {
        if (value == null) return [];

        var type = value.GetType();
        if (type == null) return [];

        var name = Enum.GetName(type, value);
        if (name == null) return [];

        return GetAttributes<TAttribute>(type, name);
    }

    private static IEnumerable<TAttribute> GetAttributes<TAttribute>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string name)
    {
        return type.GetField(name)?.GetCustomAttributes(false).OfType<TAttribute>() ?? [];
    }
}