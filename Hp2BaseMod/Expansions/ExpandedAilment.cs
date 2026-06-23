using System.Collections.Generic;

namespace Hp2BaseMod;

public static class Ailment_Ext
{
    public static ExpandedAilment Expansion(this Ailment ailment)
        => ExpandedAilment.Get(ailment);
}

/// <summary>
/// Holds additional runtime state for an <see cref="Ailment"/> instance.
/// Populated at construction time by <see cref="ExpandedAilmentDefinition.ScriptedAilmentFactory"/>
/// when the definition has one set.
/// </summary>
public class ExpandedAilment
{
    private static readonly Dictionary<Ailment, ExpandedAilment> _expansions
        = new Dictionary<Ailment, ExpandedAilment>();

    public static ExpandedAilment Get(Ailment ailment)
    {
        if (!_expansions.TryGetValue(ailment, out var expansion))
        {
            expansion = new ExpandedAilment();
            _expansions[ailment] = expansion;
        }

        return expansion;
    }

    /// <summary>
    /// The scripted behaviour attached to this ailment instance.
    /// Null if this is a purely data-driven ailment.
    /// </summary>
    public IScriptedAilment ScriptedAilment;
}