using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension;

public static partial class IListExtension
{
    /// <summary>
    /// Inserts the given value at the given index by first expanding the list by adding
    /// as many defaultValues as needed to reach the given index
    /// </summary>
    public static void FillInsert<T>(this IList<T> source, int index, T value, T defaultValue)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        var delta = index - source.Count + 1;
        for (int i = 0; i < delta; i++) source.Add(defaultValue);
        source[index] = value;
    }

    /// <summary>
    /// Inserts a new value
    /// </summary>
    public static T FillInsert<T>(this IList<T> source, int index, T defaultValue)
        where T : new()
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        var delta = index - source.Count + 1;
        for (int i = 0; i < delta; i++) source.Add(defaultValue);
        var newValue = new T();
        source[index] = newValue;
        return newValue;
    }
}
