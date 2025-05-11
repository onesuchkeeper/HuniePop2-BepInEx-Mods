using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiCellphoneInventorySlot))]
internal static class UiCellphoneInventorySlotPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiCellphoneInventorySlot __instance)
        => ExpandedUiCellphoneInventorySlot.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiCellphoneInventorySlot __instance)
        => ExpandedUiCellphoneInventorySlot.Get(__instance).OnDestroy();
}

internal class ExpandedUiCellphoneInventorySlot
{
    private static Dictionary<UiCellphoneInventorySlot, ExpandedUiCellphoneInventorySlot> _expansions
        = new Dictionary<UiCellphoneInventorySlot, ExpandedUiCellphoneInventorySlot>();

    public static ExpandedUiCellphoneInventorySlot Get(UiCellphoneInventorySlot core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiCellphoneInventorySlot(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static MethodInfo m_onTooltipPreShow = AccessTools.Method(typeof(UiCellphoneInventorySlot), "OnTooltipPreShow");

    protected UiCellphoneInventorySlot _core;
    private ExpandedUiCellphoneInventorySlot(UiCellphoneInventorySlot core)
    {
        _core = core;
    }

    public void Start()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent += OnTooltipPreShow;
    }

    public void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.itemSlot).PreShowEvent -= OnTooltipPreShow;
    }

    private void OnTooltipPreShow() => m_onTooltipPreShow.Invoke(_core, null);
}
