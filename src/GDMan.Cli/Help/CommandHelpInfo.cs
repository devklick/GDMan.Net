using System.Text;

namespace GDMan.Cli.Help;

public class CommandHelpInfo : CliHelpInfo
{
    public List<CliOptionHelpInfo> ArgsInfo { get; } = [];

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

    public void Add(CliOptionHelpInfo optionHelp)
        => ArgsInfo.Add(optionHelp);
}
