using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public static class SuccessAttractedCutscene
{
    public static void AddDataMods()
    {
        var mod = new CutsceneDataMod(Cutscenes.SuccessAttracted, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeBannerTextInfo("BannerTextSuccess", 0, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakePuzzleRefocusInfo(false, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeSpecialStepInfo("CutsceneStepPostRewards", CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeBannerTextHideInfo( CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeSubCutsceneGirlPairInfo(GirlPairRelationshipType.ATTRACTED, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeSetMoodInfo(GirlExpressionType.HORNY, false, CutsceneStepDollTargetType.RANDOM, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeBannerTextInfo("BannerTextBonusRound", 0, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeTogglePhoneInfo(false, CutsceneStepProceedType.AUTOMATIC),
            },
            CleanUpType = (CutsceneCleanUpType)(-1)
        };

        ModInterface.AddDataMod(mod);
    }
}
