using System;
using System.Reflection;

namespace Hp2BaseMod;

public class FieldGetException : Exception
{
    public FieldGetException(FieldInfo fieldInfo, Type type)
    : base(MakeMessage(fieldInfo, type))
    {
    }

    public FieldGetException(FieldInfo fieldInfo, Type type, Exception inner)
    : base(MakeMessage(fieldInfo, type), inner)
    {
    }

    internal static string MakeMessage(FieldInfo fieldInfo, Type type)
        => $"Failed to get value of field {fieldInfo.DeclaringType}.{fieldInfo.Name} as {type.Name}";
}

public class FieldGetException<T> : Exception
{
    public FieldGetException(FieldInfo fieldInfo)
    : base(FieldGetException.MakeMessage(fieldInfo, typeof(T)))
    {
    }

    public FieldGetException(FieldInfo fieldInfo, Exception inner)
    : base(FieldGetException.MakeMessage(fieldInfo, typeof(T)), inner)
    {
    }
}
