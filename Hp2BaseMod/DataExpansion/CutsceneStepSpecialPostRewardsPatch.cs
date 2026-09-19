// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Overrides the style unlocking process, in order to allow for only a single hairstyle or outfit to be unlocked rather than
/// both
/// </summary>
[HarmonyPatch(typeof(CutsceneStepSpecialPostRewards))]
internal static class CutsceneStepSpecialPostRewardsPatch
{
    private static readonly RelativeId[] TUTORIAL_REWARD_SEQUENCE = [
        PuzzleAffectionId.Talent,
        PuzzleAffectionId.Flirtation,
        PuzzleAffectionId.Romance,
        PuzzleAffectionId.Sexuality,
    ];

    private static readonly FieldInfo f_rewardDelay = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardDelay");
    private static readonly FieldInfo f_rewardtimestamp = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardtimestamp");
    private static readonly FieldInfo f_stylesUnlocked = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_stylesUnlocked");
    private static readonly FieldInfo f_puzzleStatus = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleStatus");
    private static readonly FieldInfo f_puzzleFailure = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleFailure");
    private static readonly FieldInfo f_puzzleGrid = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleGrid");
    private static readonly FieldInfo f_postRewards = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_postRewards");

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static void Update(CutsceneStepSpecialPostRewards __instance)
    {
        if (f_stylesUnlocked.GetValue<bool>(__instance)) return;

        //see if enough time has elapsed
        var rewardtimestamp = (float)f_rewardtimestamp.GetValue(__instance);
        var rewardDelay = (float)f_rewardDelay.GetValue(__instance);

        var delta = Game.Manager.Time.Lifetime(__instance.pauseDefinition) - rewardtimestamp;

        if (delta < rewardDelay) return;

        ModInterface.Log.Message("Handling style unlocks");

        f_stylesUnlocked.SetValue(__instance, true);

        if (ModInterface.GameState.CurrentState.Id == GameStateId.Puzzle
            && f_puzzleStatus.GetValue(__instance) is PuzzleStatus puzzleStatus
            && puzzleStatus.statusType == PuzzleStatusType.NORMAL
            && !puzzleStatus.IsTutorial(false)
            && !(bool)f_puzzleFailure.GetValue(__instance))
        {
            var silent = StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusLeft.playerFileGirl, false);
            StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusRight.playerFileGirl, silent);
        }
    }

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static void Start(CutsceneStepSpecialPostRewards __instance)
	{
        var puzzleStatus = Game.Session.Puzzle.puzzleStatus;
        f_puzzleStatus.SetValue(__instance, puzzleStatus);
        var puzzleGrid = Game.Session.Puzzle.puzzleGrid;
        f_puzzleGrid.SetValue(__instance, puzzleGrid);
        f_puzzleFailure.SetValue(__instance, false);
        var postRewards = f_postRewards.GetValue<List<PuzzlePostReward>>(__instance);

		var fruitCount = __instance.baseFruitCount;
		if (puzzleStatus.statusType == PuzzleStatusType.NONSTOP)
		{
			fruitCount = 0;
			int num2 = 2;
			for (int i = 0; i < puzzleStatus.roundIndex; i++)
			{
				if (i % 2 == 0 && i > 0)
				{
					num2 += 2;
				}
				fruitCount += num2;
			}
			fruitCount *= 2;
		}
		else if (puzzleStatus.statusType == PuzzleStatusType.NORMAL && !puzzleStatus.IsTutorial(false) && puzzleGrid.roundState == PuzzleRoundState.FAILURE)
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

		var rewardCount = Mathf.RoundToInt(fruitCount * 0.5f);
		for (int k = 0; k < rewardCount; k++)
		{
			ItemDefinition itemDefinition = null;
			ItemDefinition itemDefinition2 = null;

			if (puzzleStatus.statusType == PuzzleStatusType.NONSTOP)
			{
				if (k % 2 == 0)
				{
					itemDefinition = ModInterface.GameData.GetRandomFruit();
				}
				else
				{
					itemDefinition2 = ModInterface.GameData.GetRandomFruit();
				}
			}
			else if (puzzleStatus.IsTutorial(false))
			{
                var index = rewardCount % 4;
                itemDefinition = ModInterface.GameData.GetAffection(TUTORIAL_REWARD_SEQUENCE[index]).GetRandomFruit();
				itemDefinition2 = ModInterface.GameData.GetAffection(TUTORIAL_REWARD_SEQUENCE[index]).GetRandomFruit();
			}
			else
			{
				itemDefinition = puzzleStatus.girlStatusFocused.girlDefinition.GetExpansion().GetRandomFruit();
				itemDefinition2 = puzzleStatus.girlStatusUnfocused.girlDefinition.GetExpansion().GetRandomFruit();
			}

			if (itemDefinition != null)
			{
				postRewards.Add(new PuzzlePostReward(itemDefinition, "+1 " + StringUtils.Titleize(itemDefinition.GetExpansion().Affection.Name) + " Seed", puzzleStatus.altGirlFocused, null));
			}

			if (itemDefinition2 != null)
			{
				postRewards.Add(new PuzzlePostReward(itemDefinition2, "+1 " + StringUtils.Titleize(itemDefinition2.GetExpansion().Affection.Name) + " Seed", !puzzleStatus.altGirlFocused, null));
			}
		}
        f_rewardtimestamp.SetValue(__instance,  Game.Manager.Time.Lifetime(__instance.pauseDefinition));
        f_rewardDelay.SetValue(__instance, 0.64f);
		if (!f_puzzleFailure.GetValue<bool>(__instance))
		{
			Game.Session.gameCanvas.GetDoll(puzzleStatus.altGirlFocused).ChangeExpression(__instance.startExpression, false);
			Game.Session.gameCanvas.GetDoll(!puzzleStatus.altGirlFocused).ChangeExpression(__instance.startExpression, false);
		}
	}
}
