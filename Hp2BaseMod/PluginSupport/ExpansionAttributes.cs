// Hp2BaseMod 2026, By OneSuchKeeper

using System;

namespace Hp2BaseMod;

/// <summary>
/// Marks this class as the Expansion type for <paramref name="baseType"/>, per the
/// Expansion pattern. Enables the analyzer to validate the class's naming and generate its
/// shared Get/Destroy implementation, and lets members declare replacements for deprecated or
/// repurposed members of <paramref name="baseType"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ExpansionAttribute : Attribute
{
    public Type BaseType { get; }

    /// <summary>
    /// When true, this Expansion's lookup dictionary is keyed by <see cref="RelativeId"/>
    /// instead of the core object reference (per the RelativeId guidance for persistent game
    /// data): the generator emits a 'ModId' extension method on <see cref="BaseType"/> (via
    /// ModInterface.Data.GetDataId) and computes that id from "_core" to key the dictionary.
    /// </summary>
    public bool HasModId { get; set; }

    public ExpansionAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}

/// <summary>
/// Declares that a named member of the containing Expansion's base type is deprecated.
/// The containing class must carry <see cref="ExpansionAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
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
/// Declares that a same-named base-game member's contract or behavior has changed. The base
/// member still exists. May be placed on the replacing member itself, or on the Expansion class
/// when there's no single member that replaces it (see <see cref="DeprecatesAttribute"/> for the
/// same distinction).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
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
/// Declares that a base-game field's original purpose has been repurposed by this expansion. May
/// be placed on the repurposing member itself, or on the Expansion class when there's no single
/// member that repurposes it (see <see cref="DeprecatesAttribute"/> for the same distinction).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
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