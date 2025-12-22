using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace SingleDate;

public static class BonusRoundSuccessCutscene
{
    public static void AddDataMods()
    {
        var mod = new CutsceneDataMod(CutsceneIds.BonusSuccess, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeDialogTriggerInfo(DialogTriggers.SexClimax, CutsceneStepProceedType.INSTANT, CutsceneStepDollTargetType.RANDOM),
                CutsceneStepUtility.MakeBannerTextInfo("BannerTextSuccess", 0, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeWaitInfo(1.8f),
                new ShowSexPhotoCutsceneStep.Info(),
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
