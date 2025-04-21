using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(CutsceneStepSpecialPostRewards))]
public static class Foo
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
        if ((bool)_stylesUnlocked.GetValue(__instance)) { return; }

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
            var silent = HandleStyleUnlocks(puzzleStatus, puzzleStatus.girlStatusLeft, __instance.styleUnlockDuration, __instance.styleUnlockMessage, false);
            HandleStyleUnlocks(puzzleStatus, puzzleStatus.girlStatusRight, __instance.styleUnlockDuration, __instance.styleUnlockMessage, silent);
        }
    }

    private static bool HandleStyleUnlocks(PuzzleStatus puzzleStatus, PuzzleStatusGirl puzzleStatusGirl, float styleUnlockDuration, string styleUnlockMessage, bool silent)
    {
        if (!Game.Persistence.playerFile.girls.Contains(puzzleStatusGirl.playerFileGirl)
            || (puzzleStatusGirl?.playerFileGirl?.stylesOnDates ?? true))
        {
            return false;
        }

        var doll = Game.Session.gameCanvas.GetDoll(puzzleStatusGirl.girlDefinition);
        if (doll == null)
        {
            return false;
        }

        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, puzzleStatusGirl.girlDefinition.id);

        var hairId = puzzleStatusGirl.playerFileGirl.IsHairstyleUnlocked(doll.currentHairstyleIndex)
            ? RelativeId.Default
            : ModInterface.Data.GetHairstyleId(girlId, doll.currentHairstyleIndex);

        var outfitId = puzzleStatusGirl.playerFileGirl.IsOutfitUnlocked(doll.currentOutfitIndex)
            ? RelativeId.Default
            : ModInterface.Data.GetOutfitId(girlId, doll.currentOutfitIndex);

        if (hairId == outfitId)
        {
            if (outfitId != RelativeId.Default)
            {
                //both are the same and non-default
                doll.notificationBox.Show(styleUnlockMessage.Replace("(STYLE)", puzzleStatusGirl.girlDefinition.outfits[doll.currentOutfitIndex].outfitName),
                    styleUnlockDuration, silent);

                puzzleStatusGirl.playerFileGirl.UnlockHairstyle(doll.currentHairstyleIndex);
                puzzleStatusGirl.playerFileGirl.UnlockOutfit(doll.currentOutfitIndex);
            }
            else
            {
                //both are default, nothing to unlock
                return false;
            }
        }
        else
        {
            //one or both are unlocked, they aren't the same
            string message;
            if (hairId == RelativeId.Default)
            {
                message = string.Empty;
            }
            else
            {
                message = $"\"{puzzleStatusGirl.girlDefinition.hairstyles[doll.currentHairstyleIndex].hairstyleName}\" hairstyle";
                puzzleStatusGirl.playerFileGirl.UnlockHairstyle(doll.currentHairstyleIndex);
            }

            if (outfitId != RelativeId.Default)
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    message += " and ";
                }

                message += $"\"{puzzleStatusGirl.girlDefinition.outfits[doll.currentOutfitIndex].outfitName}\" outfit";
                puzzleStatusGirl.playerFileGirl.UnlockOutfit(doll.currentOutfitIndex);
            }

            doll.notificationBox.Show(message + " unlocked!", styleUnlockDuration, silent);
        }

        return true;
    }
}
