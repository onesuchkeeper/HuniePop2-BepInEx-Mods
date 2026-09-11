using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiPuzzleAilmentSlot))]
public partial class ExpandedUiPuzzleAilmentSlot
{
    [HarmonyPatch(typeof(UiPuzzleAilmentSlot))]
    private static class Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(UiPuzzleAilmentSlot __instance)
            => ExpandedUiPuzzleAilmentSlot.Get(__instance).Start_Postfix();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(UiPuzzleAilmentSlot __instance)
            => ExpandedUiPuzzleAilmentSlot.Destroy(__instance);
    }

    private void Start_Postfix()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent += OnTooltipPreShow;
    }

    private void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent -= OnTooltipPreShow;
    }
}
