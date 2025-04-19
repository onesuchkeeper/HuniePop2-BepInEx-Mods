using HarmonyLib;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleManager))]
public static class PuzzleManagerPatch
{
    private static Vector3 _defaultPuzzleGridPosition;

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void Start(PuzzleManager __instance)
    {
        _defaultPuzzleGridPosition = __instance.puzzleGrid.transform.position;
    }

    [HarmonyPatch(nameof(PuzzleManager.StartPuzzle))]
    [HarmonyPostfix]
    private static void StartPuzzle(PuzzleManager __instance)
    {
        if (!State.IsLocationPairSingle())
        {
            __instance.puzzleGrid.transform.position = _defaultPuzzleGridPosition;
            return;
        }

        Game.Session.gameCanvas.dollLeft.focusButton.Disable();
    }
}
