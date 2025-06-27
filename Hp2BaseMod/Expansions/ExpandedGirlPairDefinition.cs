using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public static class GirlPairDefinition_Ext
{
    public static ExpandedGirlPairDefinition Expansion(this GirlPairDefinition def)
        => ExpandedGirlPairDefinition.Get(def);
}

/// <summary>
/// Holds additional fields for a <see cref="GirlPairDefinition"/>.
/// Consider this readonly and do not modify these fields unless you know what your doing, instead
/// register a <see cref="GirlPairDataMod"/> using <see cref="ModInterface.AddDataMod"/>.
/// </summary>
public class ExpandedGirlPairDefinition
{
    private static Dictionary<RelativeId, ExpandedGirlPairDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedGirlPairDefinition>();

    public static ExpandedGirlPairDefinition Get(GirlPairDefinition def)
        => Get(def.id);

    public static ExpandedGirlPairDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.GirlPair, runtimeId));

    public static ExpandedGirlPairDefinition Get(RelativeId id)
    {
        if (!_expansions.TryGetValue(id, out var expansion))
        {
            expansion = new ExpandedGirlPairDefinition();
            _expansions[id] = expansion;
        }

        return expansion;
    }

    /// <summary>
    /// Maps a pair's id to its style info.
    /// </summary>
    public PairStyleInfo PairStyle;
}
