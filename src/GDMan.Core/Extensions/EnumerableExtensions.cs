namespace GDMan.Core.Extensions;

public static class EnumerableExtensions
{
    public static bool Multiple<T>(this IEnumerable<T> items)
        => items.Count() > 1;
}