using GDMan.Cli.Help;
using GDMan.Cli.Options;

namespace GDMan.Cli.Parsing;

public class ParseResult
{
    public BaseOptions? Options { get; set; }
    public bool RequiresHelp { get; set; }
    public bool RequiresVersion { get; set; }
    public CliHelpInfo? HelpInfo { get; set; }
    public string HelpInfoString => HelpInfo?.ToString() ?? throw new NullReferenceException("Expected HelpInfo to have a value but found null");
    public List<string> Errors { get; set; } = [];
    public bool HasError => Errors.Count > 0;
}