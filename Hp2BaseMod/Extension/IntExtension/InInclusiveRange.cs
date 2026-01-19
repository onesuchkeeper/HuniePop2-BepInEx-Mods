namespace Hp2BaseMod.Extension;

public static partial class IntExtension
{
    /// <param name="source">Tested value</param>
    /// <param name="max">Inclusive max value</param>
    /// <returns>True if the tested value is in the set [0, max]. False otherwise.</returns>
    public static bool InInclusiveRange(this int source, int max)
        => InInclusiveRange(source, 0, max);

    /// <param name="source">Tested value</param>
    /// <param name="min">Inclusive min value</param>
    /// <param name="max">Inclusive max value</param>
    /// <returns>True if the tested value is in the set. False otherwise.</returns>
    public static bool InInclusiveRange(this int source, int min, int max)
    {
        if (min > max)
        {
            var hold = min;
            min = max;
            max = hold;
        }

        return source >= min && source <= max;
    }
}