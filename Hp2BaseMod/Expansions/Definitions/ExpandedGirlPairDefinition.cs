using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

[Expansion(typeof(GirlPairDefinition), HasModId = true)]
public partial class ExpandedGirlPairDefinition
{
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
