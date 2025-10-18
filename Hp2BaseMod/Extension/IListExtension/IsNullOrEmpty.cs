using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;

public static partial class IListExtension
{
    /// <summary>
    /// Inserts the given value at the given index by first expanding the list by adding
    /// as many defaultValues as needed to reach the given index
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IList<T> source) => source == null || !source.Any();
}