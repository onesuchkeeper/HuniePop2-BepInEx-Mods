using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(PuzzleSet))]
internal static class PuzzleSetPatch
{
    [HarmonyPatch(nameof(PuzzleSet.GetStaminaCost))]
    [HarmonyPostfix]
    public static void GetStaminaCost(PuzzleSet __instance, bool raw, bool forceFull, ref int __result)
    {
        if (!State.IsSingleDate || raw) return;

        __result = 0;
    }
}