using System.Collections.Generic;

namespace Hp2BaseMod;

public static class Ability_Ext
{
    public static ExpandedAbility Expansion(this Ability ability)
        => ExpandedAbility.Get(ability);
}

/// <summary>
/// Holds additional runtime state for an <see cref="Ability"/> instance.
/// Populated at construction time inside <see cref="ExpandedAbilityManager"/> when the
/// definition's <see cref="ExpandedAbilityDefinition.ScriptedAbilityFactory"/> is set.
/// </summary>
public class ExpandedAbility
{
    private static readonly Dictionary<Ability, ExpandedAbility> _expansions
        = new Dictionary<Ability, ExpandedAbility>();

    public static ExpandedAbility Get(Ability ability)
    {
        if (!_expansions.TryGetValue(ability, out var expansion))
        {
            expansion = new ExpandedAbility();
            _expansions[ability] = expansion;
        }

        return expansion;
    }

    /// <summary>
    /// The scripted behaviour attached to this ability instance.
    /// Null if this is a purely data-driven ability.
    /// </summary>
    public IScriptedAbility ScriptedAbility;
}