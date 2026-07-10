using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiPuzzleAilmentSlot))]
internal static class UiPuzzleAilmentSlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleAilmentSlot __instance)
        => ExpandedUiPuzzleAilmentSlot.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiPuzzleAilmentSlot __instance)
        => ExpandedUiPuzzleAilmentSlot.Destroy(__instance);
}

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiPuzzleAilmentSlot), 
    Methods = new[]{"OnTooltipPreShow"})]
public partial class ExpandedUiPuzzleAilmentSlot
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
