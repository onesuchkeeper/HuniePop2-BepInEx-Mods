using Hp2BaseMod;
using HarmonyLib;

namespace SingleDate;
  
[HarmonyPatch(typeof(UiPuzzleGrid))]
internal static class SingleDateStartPuzzlePatch
{
    [HarmonyPatch(nameof(UiPuzzleGrid.StartPuzzle))]
    [HarmonyPrefix]
    public static void Prefix(UiPuzzleGrid __instance)
    {
        if (!State.IsSingleDate) return;
        ExpandedUiPuzzleGrid.Get().AddModifier(new SingleDateGridModifier());
    }
}