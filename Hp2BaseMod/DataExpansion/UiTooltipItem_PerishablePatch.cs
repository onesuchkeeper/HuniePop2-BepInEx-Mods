using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Required for the depreciation of ItemType, adds perishes labels
/// to tooltips base on the <see cref="ExpandedItemDefinition.IsPerishable"> flag
/// </summary>
[HarmonyPatch(typeof(UiTooltipItem))]
internal static class UiTooltipItem_PerishablePatch
{
    [HarmonyPatch(nameof(UiTooltipItem.Populate))]
    [HarmonyPostfix]
    private static void Populate_Postfix(UiTooltipItem __instance, ItemDefinition itemDef)
    {
        if (itemDef == null) return;
        var itemExp = itemDef.GetExpansion();

        if (itemExp.IsPerishable && string.IsNullOrEmpty(__instance.costLabel.text))
        {
            __instance.costLabel.text = "Perishable";
        }
    }
}