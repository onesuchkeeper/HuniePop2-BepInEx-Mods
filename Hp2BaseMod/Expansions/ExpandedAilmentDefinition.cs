using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

public static class AilmentDefinition_Ext
{
    public static ExpandedAilmentDefinition Expansion(this AilmentDefinition def)
        => ExpandedAilmentDefinition.Get(def);

    public static RelativeId ModId(this AilmentDefinition def)
        => ModInterface.Data.GetDataId(GameDataType.Ailment, def.id);
}

/// <summary>
/// Holds additional fields for a <see cref="AilmentDefinition"/>.
/// Consider this readonly and do not modify these fields unless you know what you're doing, instead
/// register a <see cref="ScriptedAilmentDataMod"/> using <see cref="ModInterface.AddDataMod"/>.
/// </summary>
public class ExpandedAilmentDefinition
{
    private static readonly Dictionary<RelativeId, ExpandedAilmentDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedAilmentDefinition>();

    public static ExpandedAilmentDefinition Get(AilmentDefinition def)
        => Get(def.ModId());

    public static ExpandedAilmentDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.Ailment, runtimeId));

    public static ExpandedAilmentDefinition Get(RelativeId id) => _expansions.GetOrNew(id);

    /// <summary>
    /// Factory that produces the IScriptedAilment for a new Ailment instance built from this definition.
    /// Null if this is a purely data-driven ailment.
    /// Set this via <see cref="ScriptedAilmentDataMod"/>.
    /// </summary>
    public Func<Ailment, IScriptedAilment> ScriptedAilmentFactory;
}