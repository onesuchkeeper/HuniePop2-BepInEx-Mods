using HarmonyLib;

namespace Hp2BaseMod;

/// <summary>
/// Required for the depreciation of ItemType, adds perishes labels
/// to tooltips base on the <see cref="ExpandedItemDefinition.IsPerishable"> flag
/// </summary>
[HarmonyPatch(typeof(UiCellphoneInventorySlot))]
internal static class UiCellphoneInventorySlot_PerishablePatch
{
    [HarmonyPatch("OnTooltipPreShow")]
    [HarmonyPrefix]
    private static bool OnTooltipPreShow_Prefix(UiCellphoneInventorySlot __instance)
    {
        var invSlot = __instance.playerFileInventorySlot;
        if (invSlot?.itemDefinition == null) return true;

        var itemExp = invSlot.itemDefinition.GetExpansion();

        // Use IsPerishable flag instead of checking itemType
        if (!itemExp.IsPerishable) return true;

        var perishTime = (ClockDaytimeType)((invSlot.daytimeStamp + (int)ClockDaytimeType.NIGHT) % 4);

        __instance.itemSlot.tooltip.costLabel.text = "Perishes: " + StringUtils.Titleize(perishTime.ToString());
        if (invSlot.daytimeStamp + 3 - Game.Persistence.playerFile.daytimeElapsed <= 1)
        {
            __instance.itemSlot.tooltip.costLabel.color = __instance.itemSlot.tooltip.errorColor;
        }
        else
        {
            __instance.itemSlot.tooltip.costLabel.color = __instance.itemSlot.tooltip.colorInfos[6].costColor;
        }

        return false;
    }
}