using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace SingleDate;

[HarmonyPatch(typeof(CutsceneStepSpecialPostRewards))]
internal static class CutsceneStepSpecialPostRewardsPatch
{
    private static readonly FieldInfo f_puzzleStatus = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleStatus");
    private static readonly FieldInfo f_puzzleGrid = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleGrid");
    private static readonly FieldInfo f_puzzleFailure = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleFailure");
    private static readonly FieldInfo f_postRewards = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_postRewards");
    private static readonly FieldInfo f_rewardtimestamp = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardtimestamp");
    private static readonly FieldInfo f_rewardDelay = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardDelay");

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool Start(CutsceneStepSpecialPostRewards __instance)
    {
        // On normal double dates, let base game & Hp2BaseMod handle both girls
        if (!State.IsSingleDate) return true;

        var puzzleStatus = Game.Session.Puzzle.puzzleStatus;
        var puzzleGrid = Game.Session.Puzzle.puzzleGrid;

        f_puzzleStatus.SetValue(__instance, puzzleStatus);
        f_puzzleGrid.SetValue(__instance, puzzleGrid);
        f_puzzleFailure.SetValue(__instance, false);

        var postRewards = f_postRewards.GetValue<List<PuzzlePostReward>>(__instance);
        postRewards.Clear();

        var fruitCount = __instance.baseFruitCount;

        // Calculate total fruit count based on date type & failure state
        if (puzzleStatus.statusType == PuzzleStatusType.NONSTOP)
        {
            fruitCount = 0;
            int fruitIncrease = 2;
            for (int i = 0; i < puzzleStatus.roundIndex; i++)
            {
                if (i % 2 == 0 && i > 0)
                {
                    fruitIncrease += 2;
                }
                fruitCount += fruitIncrease;
            }
            fruitCount *= 2;
        }
        else if (puzzleStatus.statusType == PuzzleStatusType.NORMAL 
                 && !puzzleStatus.IsTutorial(false) 
                 && puzzleGrid.roundState == PuzzleRoundState.FAILURE)
        {
            fruitCount = 2;
            for (int j = 0; j < 6; j++)
            {
                if (puzzleStatus.affection / (float)puzzleStatus.affectionGoal >= 0.15f * (j + 1))
                {
                    fruitCount += 2;
                }
            }
            f_puzzleFailure.SetValue(__instance, true);
        }

        var focusedGirl = puzzleStatus.girlStatusFocused.girlDefinition;

        // Roll fruit seeds strictly for the active date partner (Nobody is skipped)
        for (int k = 0; k < fruitCount; k++)
        {
            var itemDefinition = focusedGirl.GetExpansion().GetRandomFruit();

            if (itemDefinition != null)
            {
                postRewards.Add(new PuzzlePostReward(
                    itemDefinition,
                    "+1 " + StringUtils.Titleize(itemDefinition.GetExpansion().Affection.Name) + " Seed",
                    true // Always target the right doll on single dates
                ));
            }
        }

        f_rewardtimestamp.SetValue(__instance, Game.Manager.Time.Lifetime(__instance.pauseDefinition));
        f_rewardDelay.SetValue(__instance, 0.64f);

        if (!f_puzzleFailure.GetValue<bool>(__instance))
        {
            Game.Session.gameCanvas.GetDoll(true).ChangeExpression(__instance.startExpression, false);
        }

        return false; // Skip original method and Hp2BaseMod prefix on single dates
    }
}
