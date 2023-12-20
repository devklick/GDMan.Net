using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

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

public partial class SemVerPart
{
    [GeneratedRegex("^[0-9A-Za-z-]*$")]
    private static partial Regex SuffixRegex();

    public SemVerPartType Type { get; }
    public SemVerValueType ValueType { get; }
    public string StringValue { get; }
    public int? NumericValue { get; }

    public SemVerPart(SemVerPartType type, string value)
    {
        Type = type;
        StringValue = value;

        if (int.TryParse(value, out var n) && n >= 0)
        {
            NumericValue = n;
            ValueType = SemVerValueType.Absolute;
        }
        else if (value == "*")
        {
            ValueType = SemVerValueType.Wildcard;
        }
        else if (Type == SemVerPartType.Suffix)
        {
            ValueType = SemVerValueType.Absolute;

            if (!SuffixRegex().IsMatch(value) || string.IsNullOrWhiteSpace(value))
                ThrowInvalidFormat(type);
        }
        else ThrowInvalidFormat(type);
    }

    [DoesNotReturn]
    private void ThrowInvalidFormat(SemVerPartType type)
        => throw new FormatException($"Invalid {type} version part");

    public bool IsMatch(SemVerPart other)
        => Type == other.Type
            && (ValueType == SemVerValueType.Wildcard
            || other.ValueType == SemVerValueType.Wildcard
            || StringValue == other.StringValue);
}