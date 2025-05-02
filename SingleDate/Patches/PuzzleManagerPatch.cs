using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleManager))]
internal static class PuzzleManagerPatch
{
    private static readonly FieldInfo _puzzleStatus = AccessTools.Field(typeof(PuzzleManager), "_puzzleStatus");
    private static readonly FieldInfo _puzzleGrid = AccessTools.Field(typeof(PuzzleManager), "_puzzleGrid");

    [HarmonyPatch("OnRoundOver")]
    [HarmonyPostfix]
    private static void OnRoundOver(PuzzleManager __instance)
    {
        //level ups are dependant on the cutscene played... so we handle level up here
        if (!State.IsSingleDate)
        {
            return;
        }

        var puzzleStatus = _puzzleStatus.GetValue<PuzzleStatus>(__instance);
        var puzzleGrid = _puzzleGrid.GetValue<UiPuzzleGrid>(__instance);

        if (puzzleStatus.statusType == PuzzleStatusType.NORMAL
            && puzzleGrid.roundState == PuzzleRoundState.SUCCESS)
        {
            var girlSave = State.SaveFile.GetGirl(Game.Session.Location.currentGirlPair.girlDefinitionTwo.id);

            girlSave.RelationshipLevel = puzzleStatus.bonusRound
                ? State.MaxSingleGirlRelationshipLevel
                : Mathf.Min(girlSave.RelationshipLevel + 1, State.MaxSingleGirlRelationshipLevel - 1);
        }
    }
}