using System;
using System.Collections.Generic;

namespace Hp2BaseMod.Extension.IEnumerableExtension
{
    public static partial class IEnumerableExtension
    {
        /// <summary>
        /// Selects many from non-null IEnumerables
        /// </summary>
        public static IEnumerable<Tb> SelectManyNN<Ta, Tb>(this IEnumerable<Ta> source, Func<Ta, IEnumerable<Tb>> getSelection)
        {
            foreach (var element in source)
            {
                if (element != null)
                {
                    var selection = getSelection(element);

                    if (selection != null)
                    {
                        foreach (var sel in selection)
                        {
                            yield return sel;
                        }
                    }
                }
            }
        }
    }
}
