using System.Collections.Generic;

public static partial class IListExtension
{
    /// <summary>
    /// Attempts to get the item at the given index
    /// </summary>
    public static bool TryGet<T>(this IList<T> source, int index, out T item)
    {
        if (index < 0
            || index >= source.Count)
        {
            item = default;
            return false;
        }

        item = source[index];
        return true;
    }
}