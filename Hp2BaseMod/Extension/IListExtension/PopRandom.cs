using System;
using System.Collections.Generic;

public static partial class IListExtension
{
    public static T PopRandom<T>(this IList<T> source)
    {
        var index = UnityEngine.Random.Range(0, source.Count);
        var item = source[index];
        source.RemoveAt(index);

        return item;
    }

    public static bool TryPopRandom<T>(this IList<T> source, out T item)
    {
        if (source == null || source.Count == 0)
        {
            item = default;
            return false;
        }

        item = PopRandom(source);
        return true;
    }

    public static T PopRandom<T>(this IList<T> source, Random random)
    {
        var index = random.Next() % source.Count;
        var item = source[index];
        source.RemoveAt(index);

        return item;
    }

    public static bool TryPopRandom<T>(this IList<T> source, Random random, out T item)
    {
        if (source == null || source.Count == 0)
        {
            item = default;
            return false;
        }

        item = PopRandom(source, random);
        return true;
    }
}