using System.Text;

namespace GDMan.Cli.Help;

public class CommandHelpInfo(string fullName, string shortName, string description) : CliHelpInfo
{
    public new string FullName { get; } = fullName;
    public new string ShortName { get; } = shortName;
    public string Description { get; set; } = description;
    public List<CliOptionHelpInfo> ArgsInfo { get; } = [];

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"GDMan {FullName} | {ShortName}");
        sb.AppendLine(Description);
        sb.AppendLine();
        sb.AppendLine("Supported options:");
        sb.AppendLine();

        foreach (var arg in ArgsInfo)
        {
            sb.AppendLine(arg.ToString());
        }

        return sb.ToString().TrimEnd('\n').TrimEnd('\r');
    }

    public void Add(CliOptionHelpInfo optionHelp)
        => ArgsInfo.Add(optionHelp);
}
