using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SingleDate;

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
        => ExpandedUiAppDisplaySlot.Get(__instance).OnDestroy();
}

internal class ExpandedUiAppDisplaySlot
{
    private static Dictionary<UiAppDisplaySlot, ExpandedUiAppDisplaySlot> _expansions
        = new Dictionary<UiAppDisplaySlot, ExpandedUiAppDisplaySlot>();

    public static ExpandedUiAppDisplaySlot Get(UiAppDisplaySlot core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiAppDisplaySlot(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static MethodInfo m_onTooltipPreShow = AccessTools.Method(typeof(UiAppDisplaySlot), "OnTooltipPreShow");

    protected UiAppDisplaySlot _core;
    private ExpandedUiAppDisplaySlot(UiAppDisplaySlot core)
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
