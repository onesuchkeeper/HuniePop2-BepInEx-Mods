using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Extension
{
    public static partial class IEnumerableExtension
    {
        /// <summary>
        /// Concatenates the IEnumerable only if it isn't null
        /// </summary>
        public static IEnumerable<T> ConcatNN<T>(this IEnumerable<T> source, IEnumerable<T> second) =>
            second == null
                ? source
                : source.Concat(second);
    }
}
