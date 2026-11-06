// Hp2BaseMod 2025, By OneSuchKeeper

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Replaces the post puzzle rewards step. The base game's Start and Update both switch on the kind of puzzle (how many fruit, which
/// fruit, whether styles unlock); here they ask the <see cref="IPuzzleState"/>. Also allows for only a single hairstyle or outfit to be
/// unlocked rather than both, and speeds the rewards up by <see cref="ModState.RewardSpeedMult"/>.
/// </summary>
[HarmonyPatch(typeof(CutsceneStepSpecialPostRewards))]
internal static class CutsceneStepSpecialPostRewardsPatch
{
    private static readonly FieldInfo f_rewardDelay = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardDelay");
    private static readonly FieldInfo f_rewardtimestamp = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_rewardtimestamp");
    private static readonly FieldInfo f_stylesUnlocked = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_stylesUnlocked");
    private static readonly FieldInfo f_puzzleStatus = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleStatus");
    private static readonly FieldInfo f_puzzleFailure = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleFailure");
    private static readonly FieldInfo f_puzzleGrid = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_puzzleGrid");
    private static readonly FieldInfo f_postRewards = AccessTools.Field(typeof(CutsceneStepSpecialPostRewards), "_postRewards");
    private static readonly MethodInfo m_stepComplete = AccessTools.Method(typeof(CutsceneStepSpecial), "StepComplete");

    private static float ScaleDelay(float delay)
        => Mathf.Max(0.025f, delay / ModInterface.State.RewardSpeedMult);

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool Start(CutsceneStepSpecialPostRewards __instance)
    {
        var puzzleStatus = Game.Session.Puzzle.puzzleStatus;
        f_puzzleStatus.SetValue(__instance, puzzleStatus);
        f_puzzleGrid.SetValue(__instance, Game.Session.Puzzle.puzzleGrid);
        f_puzzleFailure.SetValue(__instance, false);
        var postRewards = f_postRewards.GetValue<List<PuzzlePostReward>>(__instance);

        if (!(ModInterface.GameState.CurrentState is IPuzzleState puzzleState))
        {
            throw new System.Exception("Rewards called outside a puzzle state");
        }

        var fruitCount = puzzleState.GetPostRewardFruitCount(__instance.baseFruitCount, out var puzzleFailed);
        f_puzzleFailure.SetValue(__instance, puzzleFailed);

        var rewardCount = Mathf.RoundToInt(fruitCount * 0.5f);
        for (int k = 0; k < rewardCount; k++)
        {
            puzzleState.GetPostRewardFruits(k, out var forFocused, out var forUnfocused);

            if (forFocused != null)
            {
                postRewards.Add(new PuzzlePostReward(forFocused, "+1 " + StringUtils.Titleize(forFocused.GetExpansion().Affection.Name) + " Seed", puzzleStatus.altGirlFocused, null));
            }

            if (forUnfocused != null)
            {
                postRewards.Add(new PuzzlePostReward(forUnfocused, "+1 " + StringUtils.Titleize(forUnfocused.GetExpansion().Affection.Name) + " Seed", !puzzleStatus.altGirlFocused, null));
            }
        }

        f_rewardtimestamp.SetValue(__instance, Game.Manager.Time.Lifetime(__instance.pauseDefinition));
        f_rewardDelay.SetValue(__instance, ScaleDelay(0.64f));

        if (!puzzleFailed)
        {
            Game.Session.gameCanvas.GetDoll(puzzleStatus.altGirlFocused).ChangeExpression(__instance.startExpression, false);
            Game.Session.gameCanvas.GetDoll(!puzzleStatus.altGirlFocused).ChangeExpression(__instance.startExpression, false);
        }

        return false;
    }

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static bool Update(CutsceneStepSpecialPostRewards __instance)
    {
        var rewardtimestamp = f_rewardtimestamp.GetValue<float>(__instance);
        var rewardDelay = f_rewardDelay.GetValue<float>(__instance);
        if (Game.Manager.Time.Lifetime(__instance.pauseDefinition) - rewardtimestamp < rewardDelay) return false;

        var puzzleStatus = f_puzzleStatus.GetValue<PuzzleStatus>(__instance);
        var puzzleFailed = f_puzzleFailure.GetValue<bool>(__instance);

        if (!f_stylesUnlocked.GetValue<bool>(__instance))
        {
            f_stylesUnlocked.SetValue(__instance, true);

            ModInterface.Log.Message("Handling style unlocks");

            if (!puzzleFailed
                && ModInterface.GameState.CurrentState is IPuzzleState puzzleState
                && puzzleState.UnlocksStylesAfterRewards)
            {
                var silent = StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusLeft.playerFileGirl, false);
                StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusRight.playerFileGirl, silent);
            }
        }

        var postRewards = f_postRewards.GetValue<List<PuzzlePostReward>>(__instance);
        if (postRewards.Count > 0)
        {
            var reward = postRewards[0];
            postRewards.RemoveAt(0);

            var doll = Game.Session.gameCanvas.GetDoll(reward.altGirl);
            Game.Persistence.playerFile.AddFruitCount(reward.itemDefinition.affectionType, 1);
            Object.Instantiate(__instance.energyTrailPrefab).Init(EnergyTrailFormat.START_AND_END, reward.energyDefinition, reward.itemDefinition, doll, reward.splashText);
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Gift.sfxFruitReward, doll.pauseDefinition).audioSource.pitch = 1.2f;
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Session.Gift.sfxFruitPop, doll.pauseDefinition).audioSource.pitch = Random.Range(0.75f, 1.5f);

            f_rewardtimestamp.SetValue(__instance, Game.Manager.Time.Lifetime(__instance.pauseDefinition));
            f_rewardDelay.SetValue(__instance, ScaleDelay(postRewards.Count > 0 ? 0.32f : 2f));
            return false;
        }

        if (!puzzleFailed)
        {
            Game.Session.gameCanvas.GetDoll(puzzleStatus.altGirlFocused).ChangeExpression(__instance.endExpression, false);
            Game.Session.gameCanvas.GetDoll(!puzzleStatus.altGirlFocused).ChangeExpression(__instance.endExpression, false);
        }

        m_stepComplete.Invoke(__instance, null);
        return false;
    }
}