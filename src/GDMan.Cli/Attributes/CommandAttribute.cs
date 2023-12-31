namespace GDMan.Cli.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute(string fullName, string shortName, string description) : Attribute
{
    public string FullName { get; } = fullName;
    public string ShortName { get; } = shortName;
    public string Description { get; } = description;
}