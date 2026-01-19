using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension;

public static partial class IList_Ext
{
    public static T GetRandom<T>(this IList<T> source)
        => source[UnityEngine.Random.Range(0, source.Count)];

    public static bool TryGetRandom<T>(this IList<T> source, out T item)
    {
        if (source == null || source.Count == 0)
        {
            item = default;
            return false;
        }

        item = source.GetRandom();
        return true;
    }

    public static T GetRandom<T>(this IList<T> source, Random random)
        => source[random.Next() % source.Count];

    public static bool TryGetRandom<T>(this IList<T> source, Random random, out T item)
    {
        if (source == null || source.Count == 0)
        {
            item = default;
            return false;
        }

        item = GetRandom(source, random);
        return true;
    }
}