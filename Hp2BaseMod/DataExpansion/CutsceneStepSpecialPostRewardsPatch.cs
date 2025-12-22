// Hp2BaseMod 2025, By OneSuchKeeper

using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod;

/// <summary>
/// Overrides the style unlocking process, in order to allow for only a single hairstyle or outfit to be unlocked rather than
/// both
/// </summary>
[HarmonyPatch(typeof(CutsceneStepSpecialPostRewards))]
public static class CutsceneStepSpecialPostRewardsPatch
{
    private static readonly FieldInfo f_rewardDelay = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardDelay");
    private static readonly FieldInfo f_rewardtimestamp = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardtimestamp");
    private static readonly FieldInfo f_stylesUnlocked = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_stylesUnlocked");
    private static readonly FieldInfo f_puzzleStatus = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleStatus");
    private static readonly FieldInfo f_puzzleFailure = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleFailure");

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static void Update(CutsceneStepSpecialPostRewards __instance)
    {
        if (f_stylesUnlocked.GetValue<bool>(__instance)) { return; }

        //see if enough time has elapsed
        var rewardtimestamp = (float)f_rewardtimestamp.GetValue(__instance);
        var rewardDelay = (float)f_rewardDelay.GetValue(__instance);

        var delta = Game.Manager.Time.Lifetime(__instance.pauseDefinition) - rewardtimestamp;

        if (delta < rewardDelay)
        {
            return;
        }

        ModInterface.Log.Message("Handling style unlocks");

        f_stylesUnlocked.SetValue(__instance, true);

        if (Game.Session.Location.currentLocation.locationType == LocationType.DATE
            && f_puzzleStatus.GetValue(__instance) is PuzzleStatus puzzleStatus
            && puzzleStatus.statusType == PuzzleStatusType.NORMAL
            && !puzzleStatus.IsTutorial(false)
            && !(bool)f_puzzleFailure.GetValue(__instance))
        {
            var silent = StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusLeft.playerFileGirl, false);
            StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusRight.playerFileGirl, silent);
        }
    }
}
