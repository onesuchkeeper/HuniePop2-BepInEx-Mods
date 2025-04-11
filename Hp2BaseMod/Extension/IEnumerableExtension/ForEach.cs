using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension.IEnumerableExtension
{
    public static partial class IEnumerableExtension
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var x in source)
            {
                action(x);
            }
        }
    }
}
