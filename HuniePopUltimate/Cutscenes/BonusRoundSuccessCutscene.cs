using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

public static class BonusRoundSuccessCutscene
{
    public static void AddDataMods()
    {
        var mod = new CutsceneDataMod(Cutscenes.BonusRoundSuccess, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeDialogTriggerInfo(new RelativeId(-1, 47), CutsceneStepProceedType.INSTANT, CutsceneStepDollTargetType.RANDOM),
                CutsceneStepUtility.MakeBannerTextInfo("BannerTextSuccess", 0, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeWaitInfo(3.8f),
                CutsceneStepUtility.MakeShowWindowInfo(true, "PhotosWindow", CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeBannerTextHideInfo(CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeSubCutsceneGirlPairInfo(GirlPairRelationshipType.LOVERS, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeHidePuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeWaitInfo(0.25f),
            },
            CleanUpType = (CutsceneCleanUpType)(-1)
        };

        ModInterface.AddDataMod(mod);
    }
}
