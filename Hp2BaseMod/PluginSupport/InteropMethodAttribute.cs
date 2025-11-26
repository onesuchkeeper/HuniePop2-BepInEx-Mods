using System;

namespace Hp2BaseMod;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class InteropMethodAttribute : Attribute
{
    public InteropMethodAttribute()
    {

    }
}