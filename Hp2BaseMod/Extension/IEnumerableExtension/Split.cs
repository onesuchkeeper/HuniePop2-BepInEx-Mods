using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension.IEnumerableExtension
{
    public static partial class IEnumerableExtension
    {
        /// <summary>
        /// Preforms the given action on each element of the IEnumerable
        /// </summary>
        public static void Split<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var x in source)
            {
                action(x);
            }
        }
    }
}
