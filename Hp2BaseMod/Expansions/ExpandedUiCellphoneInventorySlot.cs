using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiCellphoneInventorySlot))]
internal static class UiCellphoneInventorySlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiCellphoneInventorySlot __instance)
        => ExpandedUiCellphoneInventorySlot.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiCellphoneInventorySlot __instance)
        => ExpandedUiCellphoneInventorySlot.Destroy(__instance);
}

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiCellphoneInventorySlot), 
    Methods = new[]{"OnTooltipPreShow"})]
public partial class ExpandedUiCellphoneInventorySlot
{
    public void Start()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent += OnTooltipPreShow;
    }

    private void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent -= OnTooltipPreShow;
    }
}
