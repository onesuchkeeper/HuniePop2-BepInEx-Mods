using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public static class AbilityDefinition_Ext
{
    public static ExpandedAbilityDefinition Expansion(this AbilityDefinition def)
        => ExpandedAbilityDefinition.Get(def);

    public static RelativeId ModId(this AbilityDefinition def)
        => ModInterface.Data.GetDataId(GameDataType.Ability, def.id);
}

/// <summary>
/// Holds additional fields for a <see cref="AbilityDefinition"/>.
/// Consider this readonly and do not modify these fields unless you know what you're doing, instead
/// register an <see cref="AbilityDataMod"/> using <see cref="ModInterface.AddDataMod"/>.
/// </summary>
public class ExpandedAbilityDefinition
{
    private static readonly Dictionary<RelativeId, ExpandedAbilityDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedAbilityDefinition>();

    public static ExpandedAbilityDefinition Get(AbilityDefinition def)
        => Get(def.ModId());

    public static ExpandedAbilityDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.Ability, runtimeId));

    public static ExpandedAbilityDefinition Get(RelativeId id) => _expansions.GetOrNew(id);

    /// <summary>
    /// Factory invoked once per <see cref="Ability"/> construction to produce scripted behaviour.
    /// Receives the newly constructed Ability so the factory can capture instance-specific context.
    /// Null if this is a purely data-driven ability.
    /// </summary>
    public Func<Ability, IScriptedAbility> ScriptedAbilityFactory;
}