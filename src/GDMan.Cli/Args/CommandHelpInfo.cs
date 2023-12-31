using System.Text;

namespace GDMan.Cli.Args;

public class CommandHelpInfo : CliHelpInfo
{
    public List<CliArgHelpInfo> ArgsInfo { get; } =
    [
        new CliArgHelpInfo(FullName, ShortName, "Shows this help information")
    ];

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Supported arguments:");

        foreach (var arg in ArgsInfo)
        {
            sb.AppendLine(arg.ToString());
        }

        return sb.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    public void Add(CliArgHelpInfo cliArg)
        => ArgsInfo.Add(cliArg);
}
