using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hp2BaseMod.Extension.StringExtension;

public static partial class StringExtension
{
    /// <summary>
    /// Replaces the first of several patterns that matches
    /// </summary>
    public static string Replace(this string source, params IEnumerable<(string pattern, string value)> replaceGroups)
    {
        if (source == null) { return null; }

        var matchValuePairs = replaceGroups.SelectMany(group =>
            Regex.Matches(source, group.pattern)
            .OfType<Match>()
            .Where(x => x.Length != 0)
            .Select(match => (match, group.value)))
        .GroupBy(x => x.Item1.Index)
        .OrderBy(x => x.Key)
        .Select(x => x.First());

        var result = source;
        var offset = 0;
        foreach (var pair in matchValuePairs)
        {
            var index = offset + pair.Item1.Index;
            result = result.Remove(index, pair.match.Length).Insert(index, pair.value);
            offset += pair.value.Length - pair.match.Length;
        }

        return result;
    }
}
