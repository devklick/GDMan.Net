using System.ComponentModel.DataAnnotations;

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
    public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        if (value == null)
        {
            return null;
        }

        var type = value.GetType();
        if (type == null) return null;

        var name = Enum.GetName(type, value);
        if (name == null) return null;

        return type.GetField(name)?.GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
    }
}