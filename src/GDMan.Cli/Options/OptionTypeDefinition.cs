namespace GDMan.Cli.Options;

public class OptionTypeDefinition(string type, string validation, List<(string name, string description)>? allowedValues = null)
{
    public string Type { get; set; } = type;
    public string Validation { get; set; } = validation;
    public List<(string name, string description)> AllowedValues { get; set; } = allowedValues ?? [];
}