using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Extension.IEnumerableExtension
{
    public static partial class IEnumerableExtension
    {
        public static IEnumerable<T> ConcatNN<T>(this IEnumerable<T> source, IEnumerable<T> second) =>
            second == null
                ? source
                : source.Concat(second);
    }
}
