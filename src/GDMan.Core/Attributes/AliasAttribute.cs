namespace GDMan.Core.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class AliasAttribute(params string[] aliases) : Attribute
{
    public IReadOnlyList<string> Aliases { get; } = aliases.ToList();
}