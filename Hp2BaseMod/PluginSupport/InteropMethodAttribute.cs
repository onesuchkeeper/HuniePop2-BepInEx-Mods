using System;

namespace Hp2BaseMod;

/// <summary>
/// Automatically registers the method as an interop value for the plugin.
/// May only be used in a <see cref="Hp2BaseModPlugin"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class InteropMethodAttribute : Attribute { }