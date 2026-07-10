using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;

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
        => ExpandedUiAppLevelMeter.Destroy(__instance);

    [HarmonyPatch(nameof(UiAppLevelMeter.Populate))]
    [HarmonyPostfix]
    public static void Populate(UiAppLevelMeter __instance)
        => ExpandedUiAppLevelMeter.Get(__instance).Populate();

    [HarmonyPatch("OnTooltipPreShow")]
    [HarmonyPostfix]
    public static void OnTooltipPreShow(UiAppLevelMeter __instance)
        => ExpandedUiAppLevelMeter.Get(__instance).OnTooltipPreShow_Postfix();
}

[Expansion(typeof(UiAppLevelMeter), 
    Methods = new[]{"OnTooltipPreShow"})]
public partial class ExpandedUiAppLevelMeter
{
    public IExpInfo ExpDisplay;

    public void Start()
    {
        ExpandedItemSlotBehavior.Get(_core.smoothieSlot.itemSlot).PreShowEvent += OnTooltipPreShow;
    }

    private void OnDestroy()
    {
        ExpandedItemSlotBehavior.Get(_core.smoothieSlot.itemSlot).PreShowEvent -= OnTooltipPreShow;
    }

    public void Populate()
    {
        if (ExpDisplay == null) return;

        _core.meterFront.fillAmount = ExpDisplay.Percentage;
    }

    public void OnTooltipPreShow_Postfix()
    {
        if (ExpDisplay == null) return;

        _core.smoothieSlot.itemSlot.tooltip.categoryLabel.text = ExpDisplay.ExpDesc;
    }
}
