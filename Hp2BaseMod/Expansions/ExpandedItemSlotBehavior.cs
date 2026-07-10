using System;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(ItemSlotBehavior))]
internal static class ItemSlotBehaviorPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(ItemSlotBehavior __instance)
    {
        __instance.itemIcon.useSpriteMesh = true;
    }

    [HarmonyPatch(nameof(ItemSlotBehavior.ShowTooltip))]
    [HarmonyPrefix]
    public static bool ShowTooltip(ItemSlotBehavior __instance)
        => ExpandedItemSlotBehavior.Get(__instance).ShowTooltip();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    public static void OnDestroy(ItemSlotBehavior __instance)
        => ExpandedItemSlotBehavior.Get(__instance).OnDestroy();
}

/// <summary>
/// Allows tooltips to be displayed when ui is offset outside the hub.
/// Use <see cref="ModInterface.State.CellphoneOnLeft"/> to control ui position
/// </summary>
[Expansion(typeof(ItemSlotBehavior), 
    Fields = new[]{"_itemDefinition", "_tooltip", "_offsetOverride", "_tooltipOffset"})]
public partial class ExpandedItemSlotBehavior
{
    public event Action PreShowEvent;

    public bool ShowTooltip()
    {
        if (_core.showTooltip
            && _core.eastOnHub
            && ModInterface.State.CellphoneOnLeft)
        {
            var itemDef = f_itemDefinition.GetValue<ItemDefinition>(_core);

            if (itemDef != null)
            {
                var tooltip = f_tooltip.GetValue<UiTooltipItem>(_core);
                tooltip.Populate(itemDef, CardinalDirection.EAST);
                PreShowEvent?.Invoke();

                tooltip.Show(_core.transform.position,
                    MathUtils.DirectionToVector(CardinalDirection.EAST)
                    * (f_offsetOverride.GetValue<bool>(_core)
                        ? f_tooltipOffset.GetValue<int>(_core)
                        : 20f),
                    false);

                return false;
            }
        }

        return true;
    }

    public void OnDestroy()
    {
        PreShowEvent = null;
        _expansions.Remove(_core);
    }
}
