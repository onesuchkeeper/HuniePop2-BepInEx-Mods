using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppLevelMeter))]
public static class UiAppLevelMeterPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiAppLevelMeter __instance)
        => ExpandedUiAppLevelMeter.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(UiAppLevelMeter __instance)
        => ExpandedUiAppLevelMeter.Get(__instance).OnDestroy();
}

public class ExpandedUiAppLevelMeter
{
    private static Dictionary<UiAppLevelMeter, ExpandedUiAppLevelMeter> _expansions
        = new Dictionary<UiAppLevelMeter, ExpandedUiAppLevelMeter>();

    public static ExpandedUiAppLevelMeter Get(UiAppLevelMeter core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedUiAppLevelMeter(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static MethodInfo m_onTooltipPreShow = AccessTools.Method(typeof(UiAppLevelMeter), "OnTooltipPreShow");

    protected UiAppLevelMeter _core;
    private ExpandedUiAppLevelMeter(UiAppLevelMeter core)
    {
        _core = core;
    }

    public void Start()
    {
        ExpandedItemSlotBehavior.Get(_core.smoothieSlot.itemSlot).PreShowEvent += OnTooltipPreShow;
    }

    public void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.smoothieSlot.itemSlot).PreShowEvent -= OnTooltipPreShow;
    }

    private void OnTooltipPreShow() => m_onTooltipPreShow.Invoke(_core, null);
}
