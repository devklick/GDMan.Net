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

        HandleArgs(cliArgs);

        var godot = new GodotService(new GithubApiService(), new FileSystemService());

        await godot.FindDownloadAssetAsync("", true, Core.Models.Platform.Linux, Core.Models.Architecture.Arm32, Core.Models.Flavour.Mono);
    }

    private static void HandleArgs(CliArgs cliArgs)
    {
        if (cliArgs.RequiresHelp)
        {
            HandleHelp(cliArgs);
        }
        if (cliArgs.HasError)
        {
            HandleError(cliArgs);
        }
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