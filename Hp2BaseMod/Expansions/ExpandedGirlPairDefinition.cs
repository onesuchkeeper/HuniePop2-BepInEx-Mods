using System.Collections.Generic;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public static class GirlPairDefinition_Ext
{
    public static ExpandedGirlPairDefinition Expansion(this GirlPairDefinition def)
        => ExpandedGirlPairDefinition.Get(def);

    public static RelativeId ModId(this GirlPairDefinition def)
        => ModInterface.Data.GetDataId(GameDataType.GirlPair, def.id);
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

    public static ExpandedGirlPairDefinition Get(RelativeId id) => _expansions.GetOrNew(id);

    /// <summary>
    /// Maps a pair's id to its style info.
    /// </summary>
    public PairStyleInfo PairStyle;

    /// <summary>
    /// Custom default failure cutscene
    /// </summary>
    public RelativeId CutsceneNormalFailureId = RelativeId.Default;

    /// <summary>
    /// Custom default success cutscene
    /// </summary>
    public RelativeId CutsceneNormalSuccessId = RelativeId.Default;

    /// <summary>
    /// Custom compatible success cutscene
    /// </summary>
    public RelativeId CutsceneNormalCompatibleSuccessId = RelativeId.Default;

    /// <summary>
    /// Custom attracted success cutscene
    /// </summary>
    public RelativeId CutsceneNormalAttractedSuccessId = RelativeId.Default;

    /// <summary>
    /// Custom bonus new round cutscene
    /// </summary>
    public RelativeId CutsceneNormalBonusNewRoundId = RelativeId.Default;

    /// <summary>
    /// Custom bonus new round cutscene
    /// </summary>
    public RelativeId CutsceneNormalBonusSuccessId = RelativeId.Default;
}
