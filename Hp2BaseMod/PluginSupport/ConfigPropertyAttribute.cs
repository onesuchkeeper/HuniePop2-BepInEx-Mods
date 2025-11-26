using System;

namespace Hp2BaseMod;

/// <summary>
/// Marks a property as a config property. It will be registered automatically.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class ConfigPropertyAttribute : Attribute
{
    public object DefaultValue;
    public string CategoryName;
    public string Description;

    public ConfigPropertyAttribute(object defaultValue, string description, string category = null)
    {
        DefaultValue = defaultValue;
        Description = description;
        CategoryName = category ?? Hp2BaseModPlugin.CONFIG_GENERAL;
    }
}
