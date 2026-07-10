using System.Reflection;

namespace Hp2BaseMod.Extension;

public static partial class MethodInfoExtension
{
    /// <summary>
    /// Invokes a method on an object without parameters
    /// </summary>
    public static object Invoke(this MethodInfo source, object obj) => source.Invoke(obj, []);
}
