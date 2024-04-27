using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

using GDMan.Cli.Options;
using GDMan.Cli.Parsing;
using GDMan.Core.Services;
using GDMan.Core.Services.Github;
using GDMan.Core.Services.FileSystem;
using GDMan.Core.Infrastructure;

namespace GDMan.Cli;

class Program
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private static ServiceProvider _serviceProvider;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

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

        var parser = _serviceProvider.GetRequiredService<Parser>();

        await HandleParseResult(parser.Parse<InstallOptions>(args));
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
        _ => throw new InvalidOperationException()
    };

    private static async Task RunInstallAsync(InstallOptions command)
    {
        var logger = _serviceProvider.GetRequiredService<ConsoleLogger>();

        logger.LogInformation($"Processing install command");

        var godot = _serviceProvider.GetRequiredService<GodotService>();

        var result = await godot.InstallAsync(
            command.Version,
            command.Latest,
            command.Platform,
            command.Architecture,
            command.Flavour
        );

        logger.LogInformation("Done");
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