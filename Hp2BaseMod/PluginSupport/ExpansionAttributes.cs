using System;

namespace Hp2BaseMod;

/// <summary>
/// Marks this class as the Expansion type for <paramref name="baseType"/>, per the project's
/// Expansion pattern. Enables the analyzer to validate the class's naming and generate its
/// shared Get/Destroy implementation, and lets members declare replacements for deprecated or
/// repurposed members of <paramref name="baseType"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ExpansionAttribute : Attribute
{
    public Type BaseType { get; }

    /// <summary>
    /// Base-type field names to cache as <see cref="System.Reflection.FieldInfo"/> via
    /// AccessTools. The generator emits a static readonly field named
    /// '{FIELD_NAME_IN_SCREAMING_SNAKE_CASE}_FIELD' for each entry.
    /// </summary>
    public string[] Fields { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Base-type method names to cache as <see cref="System.Reflection.MethodInfo"/> via
    /// AccessTools. The generator emits a static readonly field named
    /// '{METHOD_NAME_IN_SCREAMING_SNAKE_CASE}_METHOD' for each entry.
    /// </summary>
    public string[] Methods { get; set; } = Array.Empty<string>();

    /// <summary>
    /// When true, this Expansion's lookup dictionary is keyed by <see cref="RelativeId"/>
    /// instead of the core object reference (per the RelativeId guidance for persistent game
    /// data): the generator emits a 'ModId' extension method on <see cref="BaseType"/> (via
    /// ModInterface.Data.GetDataId) and computes that id from "_core" to key the dictionary.
    /// "_core" and the private constructor are still generated either way.
    /// </summary>
    public bool HasModId { get; set; }

    public ExpansionAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}

/// <summary>
/// Declares that this member is the replacement for a deprecated member on the containing
/// Expansion's base type. Must be placed on a member of a class carrying <see cref="ExpansionAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class DeprecatesAttribute : Attribute
{
    public string MemberName { get; }

    public string Note { get; }

    public DeprecatesAttribute(string memberName, string note = null)
    {
        MemberName = memberName;
        Note = note;
    }
}

/// <summary>
/// Declares that this member's presence changes the meaning of a same-named base-game member.
/// The base member still exists, but its contract or behavior has changed.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class OverwritesAttribute : Attribute
{
    public string MemberName { get; }

    public string Note { get; }

    public OverwritesAttribute(string memberName, string note = null)
    {
        MemberName = memberName;
        Note = note;
    }
}

/// <summary>
/// Declares that a base-game field's original purpose has been repurposed by this expansion.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class RepurposesAttribute : Attribute
{
    public string MemberName { get; }

    public string Note { get; }

    public RepurposesAttribute(string memberName, string note = null)
    {
        MemberName = memberName;
        Note = note;
    }
}