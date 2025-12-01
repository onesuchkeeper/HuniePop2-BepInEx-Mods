using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

public static class SingleDateSuccessCutscene
{
    public static void AddDataMods()
    {
        ModInterface.AddDataMod(new CutsceneDataMod(CutsceneIds.Success, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeBannerTextInfo("BannerTextSuccess", 0, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakePuzzleRefocusInfo(false, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeSpecialStepInfo("CutsceneStepPostRewards", CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeBannerTextHideInfo(CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeDialogTriggerInfo(new RelativeId(-1, 16), CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.RANDOM),
                CutsceneStepUtility.MakeWaitInfo(0.5f),
                CutsceneStepUtility.MakeShowWindowInfo(true, "PhotosWindow", CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeHidePuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeWaitInfo(0.5f),
            }
        });
    }
}
