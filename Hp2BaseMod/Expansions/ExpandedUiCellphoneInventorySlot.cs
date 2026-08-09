using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiCellphoneInventorySlot))]
internal static class UiCellphoneInventorySlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiCellphoneInventorySlot __instance)
        => ExpandedUiCellphoneInventorySlot.Get(__instance).Start_Postfix();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiCellphoneInventorySlot __instance)
        => ExpandedUiCellphoneInventorySlot.Destroy(__instance);
}

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiCellphoneInventorySlot))]
public partial class ExpandedUiCellphoneInventorySlot
{
    internal void Start_Postfix()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent += OnTooltipPreShow;
    }

    private void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent -= OnTooltipPreShow;
    }
}
