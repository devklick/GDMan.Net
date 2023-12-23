using System.Diagnostics.CodeAnalysis;

using GDMan.Core.Attributes;
using GDMan.Core.Extensions;

namespace GDMan.Core.Helpers;

public class EnumHelper
{
    public static bool TryParse<TEnum>(string value, [NotNullWhen(true)] out TEnum? enumMember) where TEnum : struct, Enum
    {
        foreach (var e in Enum.GetValues<TEnum>())
        {
            if (e.ToString()?.ToLower() == value.ToLower())
            {
                enumMember = e;
                return true;
            }

            var aliases = e.GetAttribute<AliasAttribute>()?.Aliases ?? [];

            if (aliases.Any(alias => alias == e.ToString()))
            {
                enumMember = e;
                return true;
            }
        }
        enumMember = null;
        return false;
    }
}