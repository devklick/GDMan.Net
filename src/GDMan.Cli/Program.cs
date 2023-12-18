using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using GDMan.Cli.Args;

namespace GDMan.Cli;

class Program
{
    static void Main(string[] args)
    {
        var cliArgs = new CliArgs(args);

        HandleArgs(cliArgs);

        Console.WriteLine(JsonSerializer.Serialize(cliArgs.Values));
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