using GDMan.Cli.Options;

namespace GDMan.Cli.Args;

public class ParseResult
{
    public ICommandOptions? Options { get; set; }
    public bool RequiresHelp { get; set; }
    public CliHelpInfo? HelpInfo { get; set; }
    public string HelpInfoString => HelpInfo?.ToString() ?? throw new NullReferenceException("Expected HelpInfo to have a value but found null");
    public List<string> Errors { get; set; } = [];
    public bool HasError => Errors.Count > 0;
}