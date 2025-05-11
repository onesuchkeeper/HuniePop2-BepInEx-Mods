using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleStatus))]
internal static class PuzzleStatusPatch
{
    private static readonly FieldInfo _altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");
    private static readonly FieldInfo _affection = AccessTools.Field(typeof(PuzzleStatus), "_affection");
    private static readonly FieldInfo _affectionGoal = AccessTools.Field(typeof(PuzzleStatus), "_affectionGoal");

    [HarmonyPatch(nameof(PuzzleStatus.AddResourceValue))]
    [HarmonyPostfix]
    public static void AddResourceValue(PuzzleStatus __instance, PuzzleResourceType resourceType, int value, bool altGirl)
    {
        if (!State.IsSingleDate
            || resourceType != PuzzleResourceType.BROKEN)
        {
            return;
        }

        var affection = _affection.GetValue<int>(__instance);
        var affectionGoal = _affectionGoal.GetValue<int>(__instance);

        _affection.SetValue(__instance, Mathf.Clamp(affection + value, 0, affectionGoal));
    }

    [HarmonyPatch(nameof(PuzzleStatus.NextRound))]
    [HarmonyPostfix]
    public static void NextRound(PuzzleStatus __instance, int staminaOverride, bool checkStaminaFreeze)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        ModInterface.Log.LogInfo("Forcing next round girl focus for single date to alt girl");

        _altGirlFocused.SetValue(__instance, true);
    }

    [HarmonyPatch(nameof(PuzzleStatus.SetGirlFocus))]
    [HarmonyPrefix]
    public static void SetGirlFocus(PuzzleStatus __instance, ref bool altGirl)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        ModInterface.Log.LogInfo("Overwrite setGirlFocus for single date to alt girl");

        altGirl = true;
    }

    [HarmonyPatch(nameof(PuzzleStatus.SetGirlFocusByStamina))]
    [HarmonyPostfix]
    public static void SetGirlFocusByStamina(PuzzleStatus __instance)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        ModInterface.Log.LogInfo("Overwrite setGirlFocusByStamina for single date to alt girl");

        _altGirlFocused.SetValue(__instance, true);
    }
}
