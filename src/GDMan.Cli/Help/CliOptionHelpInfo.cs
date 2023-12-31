using System.Text;

namespace GDMan.Cli.Help;

public class CliOptionHelpInfo(string fullName, string shortName, string description)
{
    private readonly string _ = "  ";
    public string FullName { get; } = fullName;
    public string ShortName { get; } = shortName;
    public string Description { get; } = description;
    public string? Type { get; set; }
    public string? Validation { get; set; }
    public List<(string Value, string Description)> AllowedValues { get; set; } = [];
    public string? Default { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{_}{FullName}, {ShortName}");
        sb.AppendLine($"{_}{_}{Description}");

        AddTypeAndValidation(sb);
        AddAllowedValues(sb);
        AddDefaultValue(sb);

        return sb.ToString();
    }

    private void AddTypeAndValidation(StringBuilder sb)
    {
        if (!string.IsNullOrEmpty(Type))
        {
            sb.Append($"{_}{_}Type: {Type}");
            if (!string.IsNullOrEmpty(Validation))
            {
                sb.Append($" ({Validation})");
            }
            sb.AppendLine();
        }
    }

    private void AddAllowedValues(StringBuilder sb)
    {
        if (AllowedValues.Count > 0)
        {
            foreach (var (value, description) in AllowedValues)
            {
                sb.Append($"{_}{_}{_}{value}");
                if (!string.IsNullOrEmpty(description))
                {
                    sb.Append($": {description}");
                }
                sb.AppendLine();
            }
        }
    }

    private void AddDefaultValue(StringBuilder sb)
    {
        if (!string.IsNullOrEmpty(Default))
        {
            sb.AppendLine($"{_}{_}Default: {Default}");
        }
    }
}