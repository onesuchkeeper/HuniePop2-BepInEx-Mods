using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleManager))]
public static class PuzzleManagerPatch
{
    private static readonly FieldInfo _puzzleStatus = AccessTools.Field(typeof(PuzzleManager), "_puzzleStatus");

    [HarmonyPatch(nameof(PuzzleManager.StartPuzzle))]
    [HarmonyPrefix]
    private static void StartPuzzle(PuzzleManager __instance)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        //when starting a puzzle, focus is selected by who has more stamina
        //this makes sure the single date girl is focused
        //and make it so there's max stamina to start
        var puzzleStatus = _puzzleStatus.GetValue<PuzzleStatus>(__instance);
        puzzleStatus.girlStatusLeft.stamina = 1;
        puzzleStatus.girlStatusRight.stamina = 6;
    }
}