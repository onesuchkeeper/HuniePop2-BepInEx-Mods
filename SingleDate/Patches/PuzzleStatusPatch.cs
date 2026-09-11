using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleStatus))]
internal static class PuzzleStatusPatch
{
    private static readonly FieldInfo f_altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");
    private static readonly FieldInfo f_affection = AccessTools.Field(typeof(PuzzleStatus), "_affection");
    private static readonly FieldInfo f_affectionGoal = AccessTools.Field(typeof(PuzzleStatus), "_affectionGoal");

    [HarmonyPatch(nameof(PuzzleStatus.NextRound))]
    [HarmonyPostfix]
    public static void NextRound(PuzzleStatus __instance, int staminaOverride, bool checkStaminaFreeze)
    {
        if (!State.IsSingleDate) return;
        f_altGirlFocused.SetValue(__instance, true);
    }

    [HarmonyPatch(nameof(PuzzleStatus.SetGirlFocus))]
    [HarmonyPrefix]
    public static void SetGirlFocus(PuzzleStatus __instance, ref bool altGirl)
    {
        if (!State.IsSingleDate) return;
        altGirl = true;
    }

    [HarmonyPatch(nameof(PuzzleStatus.SetGirlFocusByStamina))]
    [HarmonyPostfix]
    public static void SetGirlFocusByStamina(PuzzleStatus __instance)
    {
        if (!State.IsSingleDate) return;
        f_altGirlFocused.SetValue(__instance, true);
    }
}