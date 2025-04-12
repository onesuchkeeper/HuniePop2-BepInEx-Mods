using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.Extension.IEnumerableExtension
{
    public static partial class IEnumerableExtension
    {
        /// <summary>
        /// Returns an empty IEnumerable if the IEnumerable is null. Otherwise returns the IEnumerable
        /// </summary>
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source) => source ?? Enumerable.Empty<T>();
    }
}
