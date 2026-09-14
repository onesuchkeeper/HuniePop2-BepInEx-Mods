using System.Collections.Generic;

namespace Hp2BaseMod.Extension;


public static partial class IList_Ext
{
    /// <summary>
    /// Shuffles the content of the list in place by swapping indexes
    /// </summary>
    public static void Shuffle<T>(this IList<T> source)
    {
        System.Random random = new System.Random();
        int num = source.Count;
        while (num > 1)
        {
            num--;
            var index = random.Next(num + 1);
            var value = source[index];
            source[index] = source[num];
            source[num] = value;
        }
    }
}