using System.Collections.Generic;

namespace Hp2BaseMod.Save;

public static class CommonSaveMethods
{
    public delegate bool TryGetValueDelegate<TKey, TValue>(TKey a, out TValue input);

    public static void HandleIndex<TKey, TValue>(IEnumerable<TValue> indexOrderedValues,
        Dictionary<TKey, TValue> output,
        TryGetValueDelegate<int, TKey> tryGetOutputKey)
    {
        var i = 0;
        foreach (var value in indexOrderedValues)
        {
            if (tryGetOutputKey(i++, out var key))
            {
                output[key] = value;
            }
            else
            {
                ModInterface.Log.LogError($"Unregistered index {i}");
            }
        }
    }
}