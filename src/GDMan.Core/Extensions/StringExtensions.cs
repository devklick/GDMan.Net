namespace GDMan.Core.Extensions;

public static class StringExtensions
{
    public static string EnsureTrailingChar(this string text, char trailingChar)
        => text.EndsWith(trailingChar) ? text : text + trailingChar;
}