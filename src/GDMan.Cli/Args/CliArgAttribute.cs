using System.Reflection;

using GDMan.Core.Attributes;
using GDMan.Core.Extensions;

namespace GDMan.Cli.Args;

[AttributeUsage(AttributeTargets.Property)]
public class CliArgAttribute : Attribute
{
    public string FullName { get; }
    public string ShortName { get; }
    public bool IsFlag { get; }

    public CliArgAttribute(string fullName, string shortName, bool isFlag = false)
    {
        FullName = fullName;
        ShortName = shortName;
        IsFlag = isFlag;

        if (!FullName.StartsWith("--")) FullName = $"--{FullName}";
        if (!ShortName.StartsWith('-')) ShortName = $"-{ShortName}";
    }

    public virtual CliArgValidation Validate(PropertyInfo propertyInfo, CliArgAttribute attr, object? value)
    {
        var type = propertyInfo.PropertyType;
        if (type == typeof(bool)) return ValidateBoolean(propertyInfo, attr, value);
        if (type.IsEnum) return ValidateEnum(propertyInfo, value);
        if (type == typeof(string)) return ValidateString(propertyInfo, value);
        throw new NotImplementedException($"Unsupported type ${type} for CliArgAttribute");
    }

    private static CliArgValidation ValidateString(PropertyInfo propertyInfo, object? value)
    {
        return CliArgValidation.Success(value?.ToString() ?? "");
    }

    private static CliArgValidation ValidateBoolean(PropertyInfo _, CliArgAttribute attr, object? value)
    {
        if (attr.IsFlag) return CliArgValidation.Success(true);

        if (!bool.TryParse((string)value!, out var boolValue))
        {
            return CliArgValidation.Failed();
        }
        return CliArgValidation.Success(boolValue);
    }

    private static CliArgValidation ValidateEnum(PropertyInfo propertyInfo, object? value)
    {
        if (value == null) return CliArgValidation.Failed();

        var values = Enum.GetValues(propertyInfo.PropertyType);

        foreach (var e in values)
        {
            // If the specified value matches the enum name, regardless of case
            if (e.ToString()?.ToLower() == value.ToString()?.ToLower())
                return CliArgValidation.Success(e);

            // If the specified value matches one of the enum aliases, regardless of case
            var aliasAttr = ((Enum)e).GetAttribute<AliasAttribute>();
            foreach (var alias in aliasAttr?.Aliases ?? [])
            {
                if (value.ToString()?.ToLower() == alias.ToLower())
                {
                    return CliArgValidation.Success(e);
                }
            }
        }

        return CliArgValidation.Failed();
    }

}