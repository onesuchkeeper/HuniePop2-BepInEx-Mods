using System.Collections.Generic;

namespace Hp2BaseMod.Extension;

public static partial class DictionaryExtension
{
    public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key, TValue defaultValue = default)
        => source.TryGetValue(key, out var value) ? value : defaultValue;
}
