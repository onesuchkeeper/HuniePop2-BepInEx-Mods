using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiCellphoneInventorySlot))]
public partial class ExpandedUiCellphoneInventorySlot
{
    [HarmonyPatch(typeof(UiCellphoneInventorySlot))]
    private static class Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start(UiCellphoneInventorySlot __instance)
            => ExpandedUiCellphoneInventorySlot.Get(__instance).Start_Postfix();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(UiCellphoneInventorySlot __instance)
            => ExpandedUiCellphoneInventorySlot.Destroy(__instance);
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
