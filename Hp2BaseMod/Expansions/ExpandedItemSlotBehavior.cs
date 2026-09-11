using System;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

/// <summary>
/// Allows tooltips to be displayed when ui is offset outside the hub.
/// Use <see cref="ModInterface.State.CellphoneOnLeft"/> to control ui position
/// </summary>
[Expansion(typeof(ItemSlotBehavior))]
public partial class ExpandedItemSlotBehavior
{
    [HarmonyPatch(typeof(ItemSlotBehavior))]
    private static class Patch
    {
        [HarmonyPatch(nameof(ItemSlotBehavior.ShowTooltip))]
        [HarmonyPrefix]
        private static bool ShowTooltip(ItemSlotBehavior __instance)
            => ExpandedItemSlotBehavior.Get(__instance).ShowTooltip();

        [HarmonyPatch("OnDestroy")]
        [HarmonyPrefix]
        private static void OnDestroy(ItemSlotBehavior __instance)
            => ExpandedItemSlotBehavior.Destroy(__instance);
    }

    public event Action PreShowEvent;

    private bool ShowTooltip()
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
}
