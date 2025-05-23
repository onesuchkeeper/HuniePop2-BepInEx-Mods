using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Extension.IEnumerableExtension
{
    public static partial class IEnumerableExtension
    {
        /// <summary>
        /// Appends the element only if it isn't null
        /// </summary>
        public static IEnumerable<T> AppendNN<T>(this IEnumerable<T> source, T element) =>
            element == null
                ? source
                : source.Append(element);
    }
}
