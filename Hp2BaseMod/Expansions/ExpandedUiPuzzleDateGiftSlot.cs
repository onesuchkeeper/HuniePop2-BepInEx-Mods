using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiPuzzleDateGiftSlot))]
public partial class ExpandedUiPuzzleDateGiftSlot
{
    [HarmonyPatch(typeof(UiPuzzleDateGiftSlot))]
    private static class Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(UiPuzzleDateGiftSlot __instance)
            => ExpandedUiPuzzleDateGiftSlot.Get(__instance).Start_Postfix();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(UiPuzzleDateGiftSlot __instance)
            => ExpandedUiPuzzleDateGiftSlot.Destroy(__instance);
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
