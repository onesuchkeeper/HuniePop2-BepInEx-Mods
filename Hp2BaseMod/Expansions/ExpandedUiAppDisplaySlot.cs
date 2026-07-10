using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiAppDisplaySlot))]
internal static class UiAppDisplaySlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiAppDisplaySlot __instance)
        => ExpandedUiAppDisplaySlot.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiAppDisplaySlot __instance)
        => ExpandedUiAppDisplaySlot.Destroy(__instance);
}

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiAppDisplaySlot), 
    Methods = new[]{"OnTooltipPreShow"})]
public partial class ExpandedUiAppDisplaySlot
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
