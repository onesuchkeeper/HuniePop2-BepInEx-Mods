using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension.IEnumerableExtension;

public static partial class IEnumerableExtension
{
    public static bool TryGetFirst<T>(this IEnumerable<T> source, out T firstValue)
    {
        if (source != null)
        {
            foreach (var entry in source)
            {
                firstValue = entry;
                return true;
            }
        }

        firstValue = default;
        return false;
    }

    public static bool TryGetFirst<T>(this IEnumerable<T> source, Func<T, bool> condition, out T firstValue)
    {
        if (source != null)
        {
            foreach (var entry in source)
            {
                if (condition(entry))
                {
                    firstValue = entry;
                    return true;
                }
            }
        }

        firstValue = default;
        return false;
    }
}
