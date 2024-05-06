namespace GDMan.Core.Extensions;

public static class StringExtensions
{
    public static string EnsureTrailingChar(this string text, char trailingChar)
        => text.EndsWith(trailingChar) ? text : text + trailingChar;

    public static string? NullIfEmpty(this string text)
        => string.IsNullOrEmpty(text) ? null : text;

    public static int? IntOrNull(this string text)
        => int.TryParse(text, out var i) ? i : null;

    public static string TrimEnd(this string text, string textToRemove)
        => text.EndsWith(textToRemove)
            ? text[..text.LastIndexOf(textToRemove)]
            : text;
}