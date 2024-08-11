using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using GDMan.Cli.Attributes;
using GDMan.Cli.Help;
using GDMan.Cli.Options;
using GDMan.Cli.Version;
using GDMan.Core.Attributes;
using GDMan.Core.Extensions;
using GDMan.Core.Infrastructure;

namespace GDMan.Cli.Parsing;

public class Parser(ConsoleLogger logger)
{
    private readonly ConsoleLogger _logger = logger;
    private readonly string[] _globalOptions = [.. CliHelpInfo.Names, .. CliVersionInfo.Names];

    public ParseResult Parse(params string[] args)
    {
        _logger.LogTrace("Parsing args");

        var result = new ParseResult();

        var arg1 = args.FirstOrDefault();
        if (!TryInitCommandOptions(arg1, out var options, out var helpInfo))
        {
            if (!string.IsNullOrEmpty(arg1) && !_globalOptions.Contains(arg1))
            {
                result.Errors.Add($"{arg1} is not a known command");
            }
        }

        result.HelpInfo = helpInfo;
        result.Options = options;

        if (CliVersionInfo.Names.Contains(arg1))
        {
            result.RequiresVersion = true;
            return result;
        }

        if (result.Options == null || CliHelpInfo.Names.Contains(arg1))
        {
            result.RequiresHelp = true;
            return result;
        }

        var props = GetOptionProps(result.Options.GetType());

        result.HelpInfo = GetCommandHelpInfo(result.Options, props);

        if (args.Any(a => a == CliHelpInfo.FullName || a == CliHelpInfo.ShortName))
        {
            result.RequiresHelp = true;
            return result;
        }

        ProcessArgs(args, props, result);

        if (result.Errors.Count != 0) return result;

        var objectValidation = result.Options.Validate();

        if (!objectValidation.Valid)
        {
            result.Errors.AddRange(objectValidation.Messages);
        }

        return result;
    }

    private void ProcessArgs(string[] args, IEnumerable<(PropertyInfo argProp, OptionAttribute attr)> props, ParseResult result)
    {
        // skip first arg, we've already handled it by determining the command to run
        var i = 1;
        while (i < args.Length)
        {
            var name = args[i];
            _logger.LogDebug($"Processing arg at position {i}: {name}");

            var (argProp, attr) = props.FirstOrDefault(a => a.attr.FullName == name || a.attr.ShortName == name);

            if (attr == null)
            {
                result.Errors.Add($"Unknown argument: {name}");
                return;
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
                return;
            }

            argProp.SetValue(result.Options, argValidation.Value);

            i += attr.IsFlag ? 1 : 2;
        }
    }

    private static bool TryInitCommandOptions(string? arg1, [NotNullWhen(true)] out BaseOptions? options, out AppHelpInfo helpInfo)
    {
        BaseOptions? returnOptions = null;
        helpInfo = new AppHelpInfo();

        if (TryInitCommandOptions<InstallOptions>(arg1, out var install, ref helpInfo))
            returnOptions = install;

        if (TryInitCommandOptions<ListOptions>(arg1, out var list, ref helpInfo))
            returnOptions = list;

        if (TryInitCommandOptions<CurrentOptions>(arg1, out var current, ref helpInfo))
            returnOptions = current;

        if (TryInitCommandOptions<UninstallOptions>(arg1, out var uninstall, ref helpInfo))
            returnOptions = uninstall;

        if (TryInitCommandOptions<UpdateOptions>(arg1, out var update, ref helpInfo))
            returnOptions = update;

        options = returnOptions;

        return options != null;
    }

    private static bool TryInitCommandOptions<T1>(string? arg1, [NotNullWhen(true)] out BaseOptions? options, ref AppHelpInfo helpInfo)
        where T1 : BaseOptions, new()
    {
        var attr = typeof(T1).GetCustomAttribute<CommandAttribute>()
            ?? throw new InvalidOperationException($"Type does not appear to represent a known command. {nameof(CommandAttribute)} expected");

        helpInfo.KnownCommands.Add((attr.FullName, attr.ShortName, attr.Description));

        options = (arg1 == attr.FullName || arg1 == attr.ShortName)
            ? Activator.CreateInstance<T1>()
            : null;

        return options != null;
    }

    private static CommandHelpInfo GetCommandHelpInfo(BaseOptions instance, IEnumerable<(PropertyInfo argProp, OptionAttribute attr)> argProps)
    {
        var commandAttr = instance.GetType().GetCustomAttribute<CommandAttribute>()
            ?? throw new InvalidOperationException($"Type does not appear to represent a known command. {nameof(CommandAttribute)} expected");

        var helpInfo = new CommandHelpInfo(commandAttr.FullName, commandAttr.ShortName, commandAttr.Description);

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

    private static string? GetDefault(BaseOptions instance, PropertyInfo prop)
        => prop.GetValue(instance)?.ToString();

    private static string? GetDescription(MemberInfo prop)
        => prop.GetCustomAttribute<DescriptionAttribute>()?.Description;

    private static OptionTypeDefinition GetTypeInfo(PropertyInfo argProp, OptionAttribute attr)
    {
        if (argProp.PropertyType == typeof(SemanticVersioning.Range))
        {
            return GetSemVersionTypeInfo(argProp);
        }
        if (argProp.PropertyType.IsEnum || argProp.PropertyType.IsNullableEnum())
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
        if (!argProp.PropertyType.IsNullableEnum(out var type))
            type = argProp.PropertyType;

        var allowedValues = Enum.GetValues(type).Cast<Enum>().Select(e =>
        {
            var name = e.ToString();
            var members = type.GetMember(name);
            var description = GetDescription(members.First());
            var aliases = e.GetAttribute<AliasAttribute>()?.Aliases ?? [];
            if (aliases.Any()) name += $", {string.Join(", ", aliases)}";
            return (name, description ?? "");
        }).ToList();
        return new("Enum", "", allowedValues);
    }

    private static IEnumerable<(PropertyInfo argProp, OptionAttribute attr)> GetOptionProps(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods)] Type type)
    {
        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty;

        return type.GetProperties(flags)
            .Select(argProp => (argProp, attr: argProp.GetCustomAttribute<OptionAttribute>()))
            .Where(value => value.attr != null)!;
    }
}