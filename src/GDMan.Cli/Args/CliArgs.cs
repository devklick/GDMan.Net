using System.ComponentModel;
using System.Reflection;

using GDMan.Core.Attributes;
using GDMan.Core.Extensions;

using Semver;

namespace GDMan.Cli.Args;

public class CliArgs
{
    public CliArgValues Values { get; set; } = CliArgValues.Default;

    public bool RequiresHelp { get; private set; }
    public CliHelpInfo HelpInfo { get; private set; }
    public List<string> Errors = [];
    public bool HasError => Errors.Count > 0;


    public CliArgs(string[] args)
    {
        var argProps = GetArgProps();

        HelpInfo = GetHelpInfo(argProps);

        if (args.Any(a => a == CliHelpInfo.FullName || a == CliHelpInfo.ShortName))
        {
            RequiresHelp = true;
            return;
        }

        var i = 0;
        while (i < args.Length)
        {
            var name = args[i];
            var (argProp, attr) = argProps.FirstOrDefault(a => a.attr.FullName == name || a.attr.ShortName == name);

            var value = attr.IsFlag ? null : args.Length >= i + 2 ? args[i + 1] : null;

            if (attr == null)
            {
                Errors.Add($"Unknown argument: {name}");
                return;
            }

            var argValidation = attr.Validate(argProp, value);

            if (!argValidation.Valid)
            {
                Errors.Add($"Invalid value for argument {name}: {value}");
                return;
            }

            argProp.SetValue(Values, argValidation.Value);

            i += attr.IsFlag ? 1 : 2;
        }

        var objectValidation = Values.Validate();

        if (!objectValidation.Valid)
        {
            Errors.AddRange(objectValidation.Messages);
        }
    }

    private CliHelpInfo GetHelpInfo(IEnumerable<(PropertyInfo argProp, CliArgAttribute attr)> argProps)
    {
        var helpInfo = new CliHelpInfo();
        foreach (var (argProp, attr) in argProps)
        {
            var description = GetDescription(argProp)
                ?? throw new InvalidDataException($"Property {argProp.Name} requires the DescriptionAttribute");

            var typeInfo = GetTypeInfo(argProp, attr);

            var cliArg = new CliArgHelpInfo(attr.FullName, attr.ShortName, description)
            {
                Default = GetDefault(argProp),
                Type = typeInfo.Type,
                Validation = typeInfo.Validation,
                AllowedValues = typeInfo.AllowedValues
            };

            helpInfo.Add(cliArg);
        }

        return helpInfo;
    }

    private string? GetDefault(PropertyInfo prop)
        => prop.GetValue(Values)?.ToString();

    private static string? GetDescription(MemberInfo prop)
        => prop.GetCustomAttribute<DescriptionAttribute>()?.Description;

    private static CliArgTypeDefinition GetTypeInfo(PropertyInfo argProp, CliArgAttribute attr)
    {
        if (argProp.PropertyType == typeof(SemVersionRange))
        {
            return GetSemVersionTypeInfo(argProp);
        }
        if (argProp.PropertyType.IsEnum)
        {
            return GetEnumTypeInfo(argProp);
        }
        if (argProp.PropertyType == typeof(bool))
        {
            return GetBoolTypeInfo(argProp, attr);
        }
        if (argProp.PropertyType == typeof(string))
        {
            return GetStringTypeInfo(argProp);
        }

        throw new NotImplementedException($"Unsupported type ${argProp.PropertyType} for CliArgAttribute");
    }

    private static CliArgTypeDefinition GetSemVersionTypeInfo(PropertyInfo argProp)
    {
        return new CliArgTypeDefinition("String", "Valid sematic version range");
    }

    private static CliArgTypeDefinition GetBoolTypeInfo(PropertyInfo argProp, CliArgAttribute attr)
        => attr.IsFlag ? new("Boolean Flag", "Present (True) or Omitted (False)") : new("Boolean", "True or False");

    private static CliArgTypeDefinition GetStringTypeInfo(PropertyInfo argProp)
    {
        return new("String", "Anything");
    }

    private static CliArgTypeDefinition GetEnumTypeInfo(PropertyInfo argProp)
    {
        var allowedValues = Enum.GetValues(argProp.PropertyType).Cast<Enum>().Select(e =>
        {
            var name = e.ToString();
            var members = argProp.PropertyType.GetMember(name);
            var description = GetDescription(members.First());
            var aliases = e.GetAttribute<AliasAttribute>()?.Aliases ?? [];
            if (aliases.Any()) name += $", {string.Join(", ", aliases)}";
            return (name, description ?? "");
        }).ToList();
        return new("Enum", "", allowedValues);
    }

    private static IEnumerable<(PropertyInfo argProp, CliArgAttribute attr)> GetArgProps()
    {
        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

        return typeof(CliArgValues).GetProperties(flags)
            .Select(argProp => (argProp, attr: argProp.GetCustomAttribute<CliArgAttribute>()))
            .Where(value => value.attr != null)!;
    }
}