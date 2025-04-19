using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiPuzzleGrid))]
public static class UiPuzzleGridPatch
{
    private static readonly FieldInfo _status = AccessTools.Field(typeof(UiPuzzleGrid), "_status");

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void Start(UiPuzzleGrid __instance)
    {
        State.DefaultPuzzleGridPosition = __instance.transform.position;
    }

    [HarmonyPatch(nameof(UiPuzzleGrid.StartPuzzle))]
    [HarmonyPostfix]
    private static void StartPuzzle(UiPuzzleGrid __instance)
    {
        if (!State.IsLocationPairSingle())
        {
            return;
        }

        var status = (PuzzleStatus)_status.GetValue(__instance);
        status.SetGirlFocus(true);
        Game.Session.gameCanvas.dollLeft.focusButton.Disable();
    }

    [HarmonyPatch(nameof(UiPuzzleGrid.AttemptGirlFocusSwitch))]
    [HarmonyPrefix]
    public static bool AttemptGirlFocusSwitch(UiPuzzleGrid __instance, ref bool __result)
    {
        if (!State.IsLocationPairSingle())
        {
            return true;
        }

        __result = false;
        return false;
    }
}