using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using GDMan.Cli.Args;
using GDMan.Core.Services;
using GDMan.Core.Services.Github;

namespace GDMan.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        var cliArgs = new CliArgs(args);

        await HandleArgs(cliArgs);
    }

    private static async Task HandleArgs(CliArgs cliArgs)
    {
        if (cliArgs.RequiresHelp)
        {
            HandleHelp(cliArgs);
        }
        if (cliArgs.HasError)
        {
            HandleError(cliArgs);
        }

        await RunAsync(cliArgs);
    }

    private static async Task RunAsync(CliArgs cliArgs)
    {
        var godot = new GodotService(
            new GithubApiService(),
            new FileSystemService(new HttpClient())
        );

        var result = await godot.ProcessAsync(
            cliArgs.Values.Version,
            cliArgs.Values.Latest,
            cliArgs.Values.Platform,
            cliArgs.Values.Architecture,
            cliArgs.Values.Flavour
        );
    }

    [DoesNotReturn]
    private static void HandleHelp(CliArgs cliArgs)
    {
        Console.WriteLine(cliArgs.HelpInfo.ToString());
        Environment.Exit(0);
    }

    [DoesNotReturn]
    private static void HandleError(CliArgs cliArgs)
    {
        Console.WriteLine(string.Join(Environment.NewLine, cliArgs.Errors));
        Console.WriteLine(Environment.NewLine);
        Console.WriteLine(cliArgs.HelpInfo.ToString());
        Environment.Exit(1);
    }
}