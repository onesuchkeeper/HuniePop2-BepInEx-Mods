using System.Collections.Generic;

public static partial class IListExtension
{
    /// <summary>
    /// Inserts the given value at the given index by first expanding the list by adding
    /// as many defaultValues as needed to reach the given index
    /// </summary>
    public static void FillInsert<T>(this IList<T> source, int index, T value, T defaultValue)
    {
        var delta = index - (source.Count - 1);
        if (delta > 0)
        {
            for (int i = 0; i < delta; i++) source.Add(defaultValue);
            source.Add(value);
        }
        else
        {
            source.Insert(index, value);
        }
    }
}