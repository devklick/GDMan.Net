using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

using GDMan.Cli.Options;
using GDMan.Cli.Parsing;
using GDMan.Core.Services;
using GDMan.Core.Services.Github;
using GDMan.Core.Services.FileSystem;
using GDMan.Core.Infrastructure;
using Microsoft.VisualBasic;

namespace GDMan.Cli;

class Program
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
    private static ServiceProvider _serviceProvider;
    private static GodotService _godot;
    private static ConsoleLogger _logger;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore IDE1006 // Naming Styles

    static async Task Main(string[] args)
    {
        _serviceProvider = new ServiceCollection()
            .AddSingleton(new ConsoleLogger(args.Contains("--verbose") || args.Contains("-vl") ? Core.Infrastructure.LogLevel.Trace : Core.Infrastructure.LogLevel.Information))
            .AddSingleton<GithubApiService>()
            .AddSingleton<GodotService>()
            .AddSingleton<Parser>()
            .AddSingleton<FS>()
            .AddSingleton<GDManBinDirectory>()
            .AddSingleton<GDManVersionsDirectory>()
            .AddSingleton<KnownPaths>()
            .BuildServiceProvider();

        _logger = _serviceProvider.GetRequiredService<ConsoleLogger>();
        _godot = _serviceProvider.GetRequiredService<GodotService>();

        var parser = _serviceProvider.GetRequiredService<Parser>();

        await HandleParseResult(parser.Parse(args));
    }

    private static async Task HandleParseResult(ParseResult cliArgs)
    {
        if (cliArgs.RequiresHelp)
        {
            HandleHelp(cliArgs);
        }

        if (cliArgs.HasError)
        {
            HandleError(cliArgs);
        }

        if (cliArgs.Options == null)
        {
            throw new NullReferenceException("Expected Options to have a value but found null");
        }

        await RunAsync(cliArgs.Options);
    }

    private static Task RunAsync(ICommandOptions command) => command switch
    {
        InstallOptions i => RunInstallAsync(i),
        ListOptions i => RunListAsync(i),
        CurrentOptions i => RunCurrentAsync(i),
        _ => throw new InvalidOperationException()
    };

    private static async Task RunInstallAsync(InstallOptions command)
    {
        _logger.LogInformation($"Processing install command");

        var result = await _godot.InstallAsync(
            command.Version,
            command.Latest,
            command.Platform,
            command.Architecture,
            command.Flavour
        );

        _logger.LogInformation("Done");
    }

    private static async Task RunListAsync(ListOptions command)
    {
        _logger.LogInformation($"Processing list command");

        var result = await _godot.ListAsync();

        foreach (var version in result.Value ?? [])
        {
            _logger.LogInformation(version);
        }

        _logger.LogInformation("Done");
    }

    private static async Task RunCurrentAsync(CurrentOptions command)
    {
        var current = await _godot.GetCurrentVersion();

        _logger.LogInformation(current.Value!);
    }


    [DoesNotReturn]
    private static void HandleHelp(ParseResult cliArgs)
    {
        Console.WriteLine(cliArgs.HelpInfoString);
        Environment.Exit(0);
    }

    [DoesNotReturn]
    private static void HandleError(ParseResult cliArgs)
    {
        Console.WriteLine(string.Join(Environment.NewLine, cliArgs.Errors));
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(cliArgs.HelpInfoString);
        Environment.Exit(1);
    }
}