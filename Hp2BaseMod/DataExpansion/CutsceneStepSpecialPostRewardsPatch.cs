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
    private static FieldInfo _rewardDelay = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardDelay");
    private static FieldInfo _rewardtimestamp = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardtimestamp");
    private static FieldInfo _stylesUnlocked = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_stylesUnlocked");
    private static FieldInfo _puzzleStatus = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleStatus");
    private static FieldInfo _puzzleFailure = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleFailure");

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static void Update(CutsceneStepSpecialPostRewards __instance)
    {
        if (_stylesUnlocked.GetValue<bool>(__instance)) { return; }

        //see if enough time has elapsed
        var rewardtimestamp = (float)_rewardtimestamp.GetValue(__instance);
        var rewardDelay = (float)_rewardDelay.GetValue(__instance);

        var delta = Game.Manager.Time.Lifetime(__instance.pauseDefinition) - rewardtimestamp;

        if (delta < rewardDelay)
        {
            return;
        }

        ModInterface.Log.LogInfo("Handling style unlocks");

        _stylesUnlocked.SetValue(__instance, true);

        if (Game.Session.Location.currentLocation.locationType == LocationType.DATE
            && _puzzleStatus.GetValue(__instance) is PuzzleStatus puzzleStatus
            && puzzleStatus.statusType == PuzzleStatusType.NORMAL
            && !puzzleStatus.IsTutorial(false)
            && !(bool)_puzzleFailure.GetValue(__instance))
        {
            var silent = StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusLeft.playerFileGirl, false);
            StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusRight.playerFileGirl, silent);
        }
    }
}
