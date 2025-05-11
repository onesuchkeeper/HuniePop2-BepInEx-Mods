using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension.IEnumerableExtension
{
    public static partial class IEnumerableExtension
    {
        /// <summary>
        /// Only allows the first count entries that pass the predicate
        /// </summary>
        public static IEnumerable<T> Limit<T>(this IEnumerable<T> source, int count, Func<T, bool> predicate)
        {
            int encounters = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                {
                    if (encounters++ < count)
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }
    }
}
