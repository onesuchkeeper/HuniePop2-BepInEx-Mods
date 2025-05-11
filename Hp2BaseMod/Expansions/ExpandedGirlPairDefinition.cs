using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

public static class GirlPairDefinition_Ext
{
    public static ExpandedGirlPairDefinition Expansion(this GirlPairDefinition def)
        => ExpandedGirlPairDefinition.Get(def);
}

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
    /// Maps a pair's id to its style info
    /// </summary>
    public PairStyleInfo PairStyle;
}
