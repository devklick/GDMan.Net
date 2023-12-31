using System.Data;

namespace GDMan.Cli.Args;

/// <summary>
/// Class representing the top-level help information for the CLI application. 
/// 
/// i.e. the help info after invoking with <c>gdman --help</c>
/// </summary>
public class AppHelpInfo : CliHelpInfo
{
    public static string AppName => "GDMan";
    public static string AppVersion =>
        typeof(CliHelpInfo).Assembly.GetName().Version?.ToString()
        ?? throw new VersionNotFoundException("Unable to determine the application version");
    public static string AppDescription => "Command line application for managing versions of Godot";
    public List<(string FullName, string ShortName, string Description)> KnownCommands { get; set; } = [];
    public override string ToString()
        => $"{AppName} (v{AppVersion})"
        + Environment.NewLine
        + AppDescription
        + Environment.NewLine + Environment.NewLine
        + "Usage: gdman [command] [command-options]"
        + Environment.NewLine + Environment.NewLine
        + "Commands:"
        + Environment.NewLine
        + string.Join(Environment.NewLine, KnownCommands.Select(c => $"  {c.FullName} | {c.ShortName} - {c.Description}"))
        + Environment.NewLine + Environment.NewLine
        + "Run 'gdman [command] --help' for more information on a command";
}