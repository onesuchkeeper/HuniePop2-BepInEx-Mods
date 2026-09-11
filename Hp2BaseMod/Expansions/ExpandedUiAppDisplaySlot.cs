using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Handles <see cref="ExpandedItemSlotBehavior.PreShowEvent"/>.
/// </summary>
[Expansion(typeof(UiAppDisplaySlot))]
public partial class ExpandedUiAppDisplaySlot
{
    [HarmonyPatch(typeof(UiAppDisplaySlot))]
    private static class Patch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void Start(UiAppDisplaySlot __instance)
            => ExpandedUiAppDisplaySlot.Get(__instance).Start_Postfix();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        public static void OnDestroy(UiAppDisplaySlot __instance)
            => ExpandedUiAppDisplaySlot.Destroy(__instance);
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
