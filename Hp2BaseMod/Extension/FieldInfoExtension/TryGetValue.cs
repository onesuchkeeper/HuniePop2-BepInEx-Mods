using System.Reflection;

namespace Hp2BaseMod.Extension;

public static partial class FieldInfoExtension
{
    /// <summary>
    /// Tries to gets a value cast to type T
    /// Allows null values for reference types
    /// </summary>
    public static bool TryGetValue<T>(this FieldInfo source, object obj, out T value)
    {
        var type = typeof(T);
        var gotValue = source.GetValue(obj);

        if (!(type.IsValueType && gotValue == null)
            && gotValue is T asT)
        {
            value = asT;
            return true;
        }

        value = default;
        return false;
    }
}
