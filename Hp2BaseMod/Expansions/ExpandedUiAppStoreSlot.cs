using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiAppStoreSlot))]
internal static class UiAppStoreSlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiAppStoreSlot __instance)
        => ExpandedUiAppStoreSlot.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiAppStoreSlot __instance)
        => ExpandedUiAppStoreSlot.Destroy(__instance);
}

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiAppStoreSlot), Methods = new[] {"OnTooltipPreShow"})]
public partial class ExpandedUiAppStoreSlot
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
