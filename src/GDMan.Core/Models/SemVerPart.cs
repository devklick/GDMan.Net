namespace GDMan.Core.Models;

public enum SemVerValueType
{
    Absolute,
    Wildcard
}

public enum SemVerPartType
{
    Major,
    Minor,
    Patch,
    Suffix
}

public class SemVerPart
{
    public SemVerPartType Type { get; }
    public SemVerValueType ValueType { get; }
    public string StringValue { get; }
    public int? NumericValue { get; }

    public bool IsMatch(SemVerPart other)
        => Type == other.Type
            && (ValueType == SemVerValueType.Wildcard
            || other.ValueType == SemVerValueType.Wildcard
            || StringValue == other.StringValue);

    public SemVerPart(SemVerPartType type, string value)
    {
        Type = type;
        StringValue = value;

        if (int.TryParse(value, out var n))
        {
            NumericValue = n;
            ValueType = SemVerValueType.Absolute;
        }
        else if (value == "*")
        {
            ValueType = SemVerValueType.Wildcard;
        }
        else throw new FormatException($"Invalid {type} version part");

        if (Type == SemVerPartType.Major && !NumericValue.HasValue)
            throw new FormatException("Cannot use wildcard for Major version part");
    }
}