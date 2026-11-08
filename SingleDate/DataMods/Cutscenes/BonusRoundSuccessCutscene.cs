using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;

namespace SingleDate;

/// <summary>
/// The cutscene (<see cref="CutsceneIds.BonusSuccess"/>) for winning the sex date (the bonus round). She
/// climaxes, the success banner shows, a sex photo is shown (see <see cref="ShowSexPhotoCutsceneStep"/>), then the lovers
/// sub cutscene plays for the pair and the puzzle grid is hidden.
/// </summary>
public static class BonusRoundSuccessCutscene
{
    /// <summary>Adds the cutscene. It uses the append insert style where the other cutscenes replace, which is the same result as this id is only defined here.</summary>
    internal static void AddDataMods()
    {
        var mod = new CutsceneDataMod(CutsceneIds.BonusSuccess, InsertStyle.append)
        {
            Steps = new()
            {
                CutsceneStepUtility.MakeDialogTriggerInfo(DialogTriggerId.SexClimax, CutsceneStepProceedType.INSTANT).TargetOrientation(DollOrientationType.RIGHT),
                CutsceneStepUtility.MakeBannerTextInfo("BannerTextSuccess", 0, CutsceneStepProceedType.AUTOMATIC),
                // Let the climax line and banner play before the photo.
                CutsceneStepUtility.MakeWaitInfo(1.8f),
                new ShowSexPhotoCutsceneStep.Info(),
                CutsceneStepUtility.MakeBannerTextHideInfo(CutsceneStepProceedType.AUTOMATIC),
                // The pair's lovers cutscene (the "now lovers" scene).
                CutsceneStepUtility.MakeSubCutsceneGirlPairInfo(GirlPairRelationshipType.LOVERS, CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeHidePuzzleGridInfo(CutsceneStepProceedType.AUTOMATIC),
                CutsceneStepUtility.MakeWaitInfo(0.25f),
            },
            // -1 is not one of the CutsceneCleanUpType values. The game only resets the dolls, header and phone for EMPTY and
// FULL, and skips all clean up for NONE, so -1 skips the doll/header/phone reset (which the sex scene doesn't want)
// but still removes the banner text and particle emitter the cutscene created.
            CleanUpType = (CutsceneCleanUpType)(-1)
        };

        ModInterface.DataMod.AddDataMod(mod);
    }
}
