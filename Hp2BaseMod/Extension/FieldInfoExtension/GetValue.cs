using System.Reflection;

namespace Hp2BaseMod.Extension;

public static partial class FieldInfoExtension
{
    /// <summary>
    /// Gets a value cast to Type T
    /// Allows null values for reference types
    /// </summary>
    public static T GetValue<T>(this FieldInfo source, object obj)
    {
        var type = typeof(T);
        var value = source.GetValue(obj);

        if (!type.IsValueType && value == null)
        {
            return default;
        }

        if (value is not T asT)
        {
            throw new FieldGetException(source, type);
        }

        return asT;
    }
}
