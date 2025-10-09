using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension;

public static partial class DictionaryExtension
{
    /// <summary>
    /// Returns the entry value for the given key, 
    /// if no entry exists first creates a new instance and adds it to the dictionary
    /// </summary>
    public static TValue GetOrNew<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key)
    where TValue : new()
    {
        if (!source.TryGetValue(key, out var value))
        {
            value = new();
            source[key] = value;
        }

        return value;
    }

    /// <summary>
    /// Returns the entry value for the given key, 
    /// if no entry exists first creates a new instance using factory and adds it to the dictionary
    /// </summary>
    public static TValue GetOrNew<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, Func<TValue> factory)
    {
        if (!source.TryGetValue(key, out var value))
        {
            value = factory();
            source[key] = value;
        }

        return value;
    }
}
