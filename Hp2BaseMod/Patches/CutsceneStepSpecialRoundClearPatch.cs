// Hp2BaseMod 2026, By OneSuchKeeper
//TODO, combine with the other cutsceneStepSpecialRoundClear patch
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Replaces the round clear step's Update. The base game unlocked the styles only for non-stop dates, by checking the kind of puzzle;
/// here the <see cref="IPuzzleState"/> says (<see cref="IPuzzleState.UnlocksStylesAfterRoundClear"/>).
/// </summary>
[HarmonyPatch(typeof(CutsceneStepSpecialRoundClear))]
internal static class CutsceneStepSpecialRoundClearPatch
{
    private static readonly FieldInfo f_puzzleStatus = AccessTools.Field(typeof(CutsceneStepSpecialRoundClear), "_puzzleStatus");
    private static readonly FieldInfo f_resourceValue = AccessTools.Field(typeof(CutsceneStepSpecialRoundClear), "_resourceValue");
    private static readonly FieldInfo f_loopIndex = AccessTools.Field(typeof(CutsceneStepSpecialRoundClear), "_loopIndex");
    private static readonly FieldInfo f_rewardtimestamp = AccessTools.Field(typeof(CutsceneStepSpecialRoundClear), "_rewardtimestamp");
    private static readonly FieldInfo f_rewardDelay = AccessTools.Field(typeof(CutsceneStepSpecialRoundClear), "_rewardDelay");
    private static readonly FieldInfo f_stylesUnlocked = AccessTools.Field(typeof(CutsceneStepSpecialRoundClear), "_stylesUnlocked");
    private static readonly MethodInfo m_stepComplete = AccessTools.Method(typeof(CutsceneStepSpecial), "StepComplete");

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static bool Update(CutsceneStepSpecialRoundClear __instance)
    {
        var rewardtimestamp = f_rewardtimestamp.GetValue<float>(__instance);
        var rewardDelay = f_rewardDelay.GetValue<float>(__instance);
        if (Game.Manager.Time.Lifetime(__instance.pauseDefinition) - rewardtimestamp < rewardDelay) return false;

        var puzzleStatus = f_puzzleStatus.GetValue<PuzzleStatus>(__instance);

        if (!f_stylesUnlocked.GetValue<bool>(__instance))
        {
            f_stylesUnlocked.SetValue(__instance, true);

            if (ModInterface.GameState.CurrentState is IPuzzleState puzzleState
                && puzzleState.UnlocksStylesAfterRoundClear)
            {
                var silent = StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusLeft.playerFileGirl, false);
                StyleUnlockUtility.UnlockCurrentStyle(puzzleStatus.girlStatusRight.playerFileGirl, silent);
            }
        }

        var loopIndex = f_loopIndex.GetValue<int>(__instance);
        var resourceValue = f_resourceValue.GetValue<int>(__instance);
        if (loopIndex < resourceValue)
        {
            var altGirl = loopIndex % 2 == 0 ? puzzleStatus.altGirlFocused : !puzzleStatus.altGirlFocused;
            puzzleStatus.GetExpansion().AddResourceValue(PuzzleResourceId.From(__instance.bonusResourceType), 1, altGirl);
            puzzleStatus.CheckChanges();

            var token = Game.Data.Tokens.GetByResourceType(__instance.bonusResourceType, PuzzleAffectionType.TALENT);
            Object.Instantiate(__instance.energyTrailPrefab).Init(
                EnergyTrailFormat.START_AND_END,
                token.energyDefinition,
                null,
                Game.Session.gameCanvas.GetDoll(altGirl),
                "+1 " + token.resourceName);
            Game.Manager.Audio.Play(AudioCategory.SOUND, token.sfxMatch, __instance.pauseDefinition);

            loopIndex++;
            f_loopIndex.SetValue(__instance, loopIndex);
            f_rewardtimestamp.SetValue(__instance, Game.Manager.Time.Lifetime(__instance.pauseDefinition));
            f_rewardDelay.SetValue(__instance, loopIndex < resourceValue ? 0.32f : 2f);
            return false;
        }

        Game.Session.gameCanvas.GetDoll(puzzleStatus.altGirlFocused).ChangeExpression(__instance.endExpression, false);
        Game.Session.gameCanvas.GetDoll(!puzzleStatus.altGirlFocused).ChangeExpression(__instance.endExpression, false);
        m_stepComplete.Invoke(__instance, null);
        return false;
    }
}