using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using GDMan.Cli.Attributes;
using GDMan.Cli.Help;
using GDMan.Cli.Options;
using GDMan.Core.Attributes;
using GDMan.Core.Extensions;

using Semver;

namespace GDMan.Cli.Parsing;

public class Parser(ConsoleLogger logger)
{
    private readonly ConsoleLogger _logger = logger;

    public ParseResult Parse<T1>(params string[] args)
        where T1 : ICommandOptions, new()
    {
        _logger.LogTrace("Parsing args");

        var result = new ParseResult();

        var arg1 = args.First();
        if (!TryInitCommandOptions<T1>(arg1, out var options, out var helpInfo))
        {
            result.Errors.Add($"{arg1} is not a known command");
            result.HelpInfo = helpInfo;
            result.RequiresHelp = true;
            return result;
        }

        result.Options = options;

        var props = GetOptionProps(result.Options.GetType());

        result.HelpInfo = GetCommandHelpInfo(result.Options, props);

        if (args.Any(a => a == CliHelpInfo.FullName || a == CliHelpInfo.ShortName))
        {
            result.RequiresHelp = true;
            return result;
        }

        // skip first arg, we've already handled it
        // by determining the command to run
        var i = 1;
        while (i < args.Length)
        {
            var name = args[i];
            _logger.LogDebug($"Processing arg at position {i}: {name}");

            var (argProp, attr) = props.FirstOrDefault(a => a.attr.FullName == name || a.attr.ShortName == name);

            if (attr == null)
            {
                result.Errors.Add($"Unknown argument: {name}");
                return result;
            }

            string? value = null;

            if (attr.IsFlag)
            {
                _logger.LogDebug($"Arg at position {i} found to be a flag");
            }
            else
            {
                value = args.Length >= i + 2 ? args[i + 1] : null;
                _logger.LogDebug($"Arg at position {i} found to have value {value}");
            }


            var argValidation = attr.Validate(argProp, value);

            if (!argValidation.Valid)
            {
                result.Errors.Add($"Invalid value for argument {name}: {value}");
                return result;
            }

            argProp.SetValue(result.Options, argValidation.Value);

            i += attr.IsFlag ? 1 : 2;
        }

        var objectValidation = result.Options.Validate();

        if (!objectValidation.Valid)
        {
            result.Errors.AddRange(objectValidation.Messages);
        }

        return result;
    }

    private static bool TryInitCommandOptions<T1>(string? arg1, [NotNullWhen(true)] out ICommandOptions? options, out CliHelpInfo helpInfo)
        where T1 : ICommandOptions, new()
    {
        var attr = typeof(T1).GetCustomAttribute<CommandAttribute>()
            ?? throw new InvalidOperationException($"Type does not appear to represent a known command. {nameof(CommandAttribute)} expected");

        helpInfo = new AppHelpInfo
        {
            KnownCommands = [(attr.FullName, attr.ShortName, attr.Description)]
        };

        options = (arg1 == attr.FullName || arg1 == attr.ShortName)
            ? Activator.CreateInstance<T1>()
            : null;

        return options != null;
    }

    private static CommandHelpInfo GetCommandHelpInfo(ICommandOptions instance, IEnumerable<(PropertyInfo argProp, OptionAttribute attr)> argProps)
    {
        var helpInfo = new CommandHelpInfo();
        foreach (var (argProp, attr) in argProps)
        {
            var typeInfo = GetTypeInfo(argProp, attr);

            var cliArg = new CliOptionHelpInfo(attr.FullName, attr.ShortName, attr.Description)
            {
                Default = GetDefault(instance, argProp),
                Type = typeInfo.Type,
                Validation = typeInfo.Validation,
                AllowedValues = typeInfo.AllowedValues
            };

            helpInfo.Add(cliArg);
        }

        return helpInfo;
    }

    private static string? GetDefault(ICommandOptions instance, PropertyInfo prop)
        => prop.GetValue(instance)?.ToString();

    private static string? GetDescription(MemberInfo prop)
        => prop.GetCustomAttribute<DescriptionAttribute>()?.Description;

    private static OptionTypeDefinition GetTypeInfo(PropertyInfo argProp, OptionAttribute attr)
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

    private static OptionTypeDefinition GetSemVersionTypeInfo(PropertyInfo argProp)
    {
        return new OptionTypeDefinition("String", "Valid sematic version range");
    }

    private static OptionTypeDefinition GetBoolTypeInfo(PropertyInfo argProp, OptionAttribute attr)
        => attr.IsFlag ? new("Boolean Flag", "Present (True) or Omitted (False)") : new("Boolean", "True or False");

    private static OptionTypeDefinition GetStringTypeInfo(PropertyInfo argProp)
    {
        return new("String", "Anything");
    }

    [UnconditionalSuppressMessage("AssemblyLoadTrimming", "IL2075",
        Justification = "Unable to fix properly and seems to work as-is, so suppressing error")]
    private static OptionTypeDefinition GetEnumTypeInfo(PropertyInfo argProp)
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

    private static IEnumerable<(PropertyInfo argProp, OptionAttribute attr)> GetOptionProps(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
    {
        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

        return type.GetProperties(flags)
            .Select(argProp => (argProp, attr: argProp.GetCustomAttribute<OptionAttribute>()))
            .Where(value => value.attr != null)!;
    }
}