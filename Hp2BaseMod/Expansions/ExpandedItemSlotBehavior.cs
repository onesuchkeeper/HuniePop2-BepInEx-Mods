using System;
using System.Collections.Generic;
using System.Reflection;
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
public class ExpandedItemSlotBehavior
{
    private static Dictionary<ItemSlotBehavior, ExpandedItemSlotBehavior> _expansions
        = new Dictionary<ItemSlotBehavior, ExpandedItemSlotBehavior>();

    public static ExpandedItemSlotBehavior Get(ItemSlotBehavior core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedItemSlotBehavior(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_itemDefinition = AccessTools.Field(typeof(ItemSlotBehavior), "_itemDefinition");
    private static readonly FieldInfo f_tooltip = AccessTools.Field(typeof(ItemSlotBehavior), "_tooltip");
    private static readonly FieldInfo f_offsetOverride = AccessTools.Field(typeof(ItemSlotBehavior), "_offsetOverride");
    private static readonly FieldInfo f_tooltipOffset = AccessTools.Field(typeof(ItemSlotBehavior), "_tooltipOffset");

    public event Action PreShowEvent;

    protected ItemSlotBehavior _core;
    private ExpandedItemSlotBehavior(ItemSlotBehavior core)
    {
        _core = core;
    }

    public bool ShowTooltip()
    {
        if (ModInterface.State.CellphoneOnLeft || _core.eastOnHub)
        {
            return true;
        }

        var itemDef = f_itemDefinition.GetValue<ItemDefinition>(_core);

        if (!_core.showTooltip || itemDef == null)
        {
            return true;
        }

        var tooltip = f_tooltip.GetValue<UiTooltipItem>(_core);

        tooltip.Populate(itemDef, CardinalDirection.EAST);

        PreShowEvent?.Invoke();

        tooltip.Show(_core.transform.position,
            MathUtils.DirectionToVector(CardinalDirection.EAST)
            * (f_offsetOverride.GetValue<bool>(_core)
                ? f_tooltipOffset.GetValue<float>(_core)
                : 20f),
            false);

        return false;
    }

    public void OnDestroy()
    {
        PreShowEvent = null;
        _expansions.Remove(_core);
    }
}
