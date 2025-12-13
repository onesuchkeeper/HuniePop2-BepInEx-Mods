using System.Collections.Specialized;

namespace Hp2BaseMod.Extension;

public static partial class OrderedDictionaryExtension
{
    public static bool TryGetValue<T>(this OrderedDictionary orderedDictionary, string key, out T value)
    {
        if (orderedDictionary.Contains(key) && orderedDictionary[key] is T valueEntry)
        {
            value = valueEntry;
            return true;
        }

        value = default;
        return false;
    }
}