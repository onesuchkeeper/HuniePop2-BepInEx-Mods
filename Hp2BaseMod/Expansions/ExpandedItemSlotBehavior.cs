using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

/// <summary>
/// Allows tooltips to be displayed when ui is offset outside the hub, and replaces the base game's tooltip so it never asks the
/// location for its type.
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

    /// <summary>
    /// Replaces the base game's tooltip, which asked LocationManager.AtLocationType whether it was at the hub. That throws,
    /// and the base method must never run, so every case is handled here: the tooltip goes east when the slot asks for it
    /// on the hub and the ui is offset like it is there, otherwise it uses the slot's own direction.
    /// </summary>
    private bool ShowTooltip()
    {
        var itemDef = f_itemDefinition.GetValue<ItemDefinition>(_core);

        if (!_core.showTooltip || itemDef == null)
        {
            return false;
        }

        var direction = _core.eastOnHub && ModInterface.State.CellphoneOnLeft
            ? CardinalDirection.EAST
            : _core.direction;

        var tooltip = f_tooltip.GetValue<UiTooltipItem>(_core);
        tooltip.Populate(itemDef, direction);
        RaisePreShowEvent();

        tooltip.Show(_core.transform.position,
            MathUtils.DirectionToVector(direction)
                * (f_offsetOverride.GetValue<bool>(_core)
                    ? f_tooltipOffset.GetValue<int>(_core)
                    : 20f),
            false);

        return false;
    }
}