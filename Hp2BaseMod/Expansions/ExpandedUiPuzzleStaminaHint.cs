using HarmonyLib;

namespace Hp2BaseMod;

[Expansion(typeof(UiPuzzleStaminaHint))]
public partial class ExpandedUiPuzzleStaminaHint
{
    [HarmonyPatch(typeof(UiPuzzleStaminaHint))]
    private static class Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(UiPuzzleStaminaHint __instance)
            => ExpandedUiPuzzleStaminaHint.Get(__instance).Start_Postfix();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(UiPuzzleStaminaHint __instance)
            => ExpandedUiPuzzleStaminaHint.Destroy(__instance);
    }

    private void Start_Postfix()
    {
        ExpandedUiPuzzleGrid.Get().MoveCompleteEvent += OnShouldCheck;
    }

    private void OnDestroy()
    {
        ExpandedUiPuzzleGrid.Get().MoveCompleteEvent -= OnShouldCheck;
    }
}