using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiAppLevelMeter))]
internal static class UiAppLevelMeterPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiAppLevelMeter __instance)
        => ExpandedUiAppLevelMeter.Get(__instance).Start();

    [HarmonyPatch("OnDestroy")]
    [HarmonyPrefix]
    public static void OnDestroy(UiAppLevelMeter __instance)
        => ExpandedUiAppLevelMeter.Get(__instance).OnDestroy();

    [HarmonyPatch(nameof(UiAppLevelMeter.Populate))]
    [HarmonyPostfix]
    public static void Populate(UiAppLevelMeter __instance)
        => ExpandedUiAppLevelMeter.Get(__instance).Populate();

    [HarmonyPatch("OnTooltipPreShow")]
    [HarmonyPostfix]
    public static void OnTooltipPreShow(UiAppLevelMeter __instance)
        => ExpandedUiAppLevelMeter.Get(__instance).OnTooltipPreShow();
}

internal class ExpandedUiAppLevelMeter
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
    public IExpInfo ExpDisplay;
    protected UiAppLevelMeter _core;
    private ExpandedUiAppLevelMeter(UiAppLevelMeter core)
    {
        _core = core;
    }

    public void Start()
    {
        ExpandedItemSlotBehavior.Get(_core.smoothieSlot.itemSlot).PreShowEvent += OnTooltipPreShowEvent;
    }

    public void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.smoothieSlot.itemSlot).PreShowEvent -= OnTooltipPreShowEvent;
        _expansions.Remove(_core);
    }

    public void Populate()
    {
        if (ExpDisplay == null)
        {
            return;
        }

        _core.meterFront.fillAmount = ExpDisplay.Percentage;
    }

    public void OnTooltipPreShow()
    {
        if (ExpDisplay == null)
        {
            return;
        }

        _core.smoothieSlot.itemSlot.tooltip.categoryLabel.text = ExpDisplay.ExpDesc;
    }

    private void OnTooltipPreShowEvent() => m_onTooltipPreShow.Invoke(_core, null);
}
