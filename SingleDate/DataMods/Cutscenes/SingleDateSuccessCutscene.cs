using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
namespace SingleDate;

/// <summary>
/// The cutscene (<see cref="CutsceneIds.Success"/>) for winning a single date. Shows the success banner, refocuses
/// the puzzle, hands out the post date seed rewards (see <see cref="CutsceneStepSpecialPostRewardsPatch"/>), then she
/// says her success line and a date photo is shown, and the puzzle grid is hidden.
/// </summary>
public static class SingleDateSuccessCutscene
{
    /// <summary>Adds the cutscene, replacing the pair's success cutscene.</summary>
    internal static void AddDataMods()
    {
        ModInterface.DataMod.AddDataMod(new CutsceneDataMod(CutsceneIds.Success, InsertStyle.replace)
        {
            CleanUpType = CutsceneCleanUpType.NONE,
            Steps = new List<IGameDefinitionInfo<CutsceneStepSubDefinition>>()
            {
                CutsceneStepUtility.MakeBannerTextInfo("BannerTextSuccess", 0, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakePuzzleRefocusInfo(false, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeSpecialStepInfo("CutsceneStepPostRewards", CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeBannerTextHideInfo(CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeDialogTriggerInfo(Hp2BaseMod.DialogTriggerId.DateSuccess).TargetOrientation(DollOrientationType.RIGHT),
                CutsceneStepUtility.MakeWaitInfo(0.5f),
                new ShowDatePhotoCutsceneStep.Info(),
                CutsceneStepUtility.MakeHidePuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeWaitInfo(0.5f),
            }
        });
    }
}
