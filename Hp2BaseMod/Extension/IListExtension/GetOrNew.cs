using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension;

public static partial class IList_Ext
{
    /// <summary>
    /// Gets the value at the given index. A new value at that index is created if it did not
    /// exists already. Other indexes are padded with the given default value.
    /// </summary>
    public static T GetOrNew<T>(this IList<T> source, int index, T defaultValue = default)
        where T : class, new()
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        var delta = index - source.Count + 1;
        for (int i = 0; i < delta; i++) source.Add(defaultValue);
        source[index] ??= new();
        return source[index];
    }

    /// <summary>
    /// Gets the value at the given index. A new value at that index is created if it did not
    /// exists already. Other indexes are padded with the given default value.
    /// </summary>
    public static T GetOrNew<T>(this IList<T> source, int index, Func<T> factory)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        var delta = index - source.Count + 1;
        for (int i = 0; i < delta; i++) source.Add(factory());
        source[index] ??= factory();
        return source[index];
    }
}
