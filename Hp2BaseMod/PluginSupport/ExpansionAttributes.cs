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
/// Declares that a named member of the containing Expansion's base type is deprecated.
/// <para>
/// Can be placed two ways:
/// </para>
/// <list type="bullet">
/// <item>
/// <description>
/// On a <em>member</em> of the Expansion class: that member is specifically the replacement
/// for <paramref name="memberName"/>, and the generated diagnostic points at it by name
/// ("Use ExpandedFoo.Bar instead.") unless overridden by <paramref name="note"/>.
/// </description>
/// </item>
/// <item>
/// <description>
/// On the Expansion <em>class</em> itself: there is no single replacement member to point
/// at - useful when a deprecated member's behavior was split across several members, folded
/// into existing ones, or simply removed with no direct equivalent. The diagnostic falls back
/// to naming just the Expansion type, or - preferably - <paramref name="note"/> should explain
/// what to do instead. Multiple class-level attributes may be stacked to cover several
/// deprecated members from the same base type.
/// </description>
/// </item>
/// </list>
/// Either way, the containing class must carry <see cref="ExpansionAttribute"/>.
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