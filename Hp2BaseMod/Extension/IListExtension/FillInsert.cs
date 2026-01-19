using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension;

public static partial class IList_Ext
{
    /// <summary>
    /// Set the value to the list item at the given index. 
    /// If list does not include the given index, pads the list with new values until it does.
    /// </summary>
    public static void FillSet<T>(this IList<T> source, int index, T value, T defaultValue = default)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        var delta = index - source.Count + 1;

        if (delta > 0)
        {
            delta--;
            for (int i = 0; i < delta; i++) source.Add(defaultValue);
            source.Add(value);
        }
        else
        {
            source[index] = value;
        }
    }

    /// <summary>
    /// Set the value to the list item at the given index.
    /// If list does not include the given index, pads the list with new values until it does.
    /// </summary>
    public static void FillSet<T>(this IList<T> source, int index, T value, Func<T> fillValueFactory)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        var delta = index - source.Count + 1;

        if (delta > 0)
        {
            delta--;
            for (int i = 0; i < delta; i++) source.Add(fillValueFactory());
            source.Add(value);
        }
        else
        {
            source[index] = value;
        }
    }
}
