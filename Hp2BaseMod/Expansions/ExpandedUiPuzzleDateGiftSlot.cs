using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiPuzzleDateGiftSlot))]
internal static class UiPuzzleDateGiftSlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleDateGiftSlot __instance)
        => ExpandedUiPuzzleDateGiftSlot.Get(__instance).Start_Postfix();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiPuzzleDateGiftSlot __instance)
        => ExpandedUiPuzzleDateGiftSlot.Destroy(__instance);
}

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiPuzzleDateGiftSlot))]
public partial class ExpandedUiPuzzleDateGiftSlot
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
