using System.Diagnostics.CodeAnalysis;

using GDMan.Cli.Options;
using GDMan.Cli.Parsing;
using GDMan.Core.Services;
using GDMan.Core.Services.Github;

namespace GDMan.Cli;

class Program
{
    static async Task Main(string[] args)
        => await HandleResult(Parser.Parse<InstallOptions>(args));

    private static async Task HandleResult(ParseResult cliArgs)
    {
        if (cliArgs.RequiresHelp)
        {
            HandleHelp(cliArgs);
        }

        if (cliArgs.HasError)
        {
            HandleError(cliArgs);
        }

        await RunAsync(cliArgs.Options
            ?? throw new NullReferenceException("Expected Options to have a value but found null"));
    }

    private static Task RunAsync(ICommandOptions command) => command switch
    {
        InstallOptions i => RunInstallAsync(i),
        _ => throw new InvalidOperationException()
    };

    private static async Task RunInstallAsync(InstallOptions command)
    {
        var godot = new GodotService(
            new GithubApiService()
        );

        var result = await godot.InstallAsync(
            command.Version,
            command.Latest,
            command.Platform,
            command.Architecture,
            command.Flavour
        );
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