using System.Collections.Generic;

namespace Hp2BaseMod.Extension;

public static partial class IListExtension
{
    public static T PopFirst<T>(this IList<T> source)
    {
        var item = source[0];
        source.RemoveAt(0);

        return item;
    }

    public static bool TryPopFirst<T>(this IList<T> source, out T item)
    {
        if (source == null || source.Count == 0)
        {
            item = default;
            return false;
        }

        item = PopFirst(source);
        return true;
    }
}