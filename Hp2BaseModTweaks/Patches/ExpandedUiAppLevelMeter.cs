using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiAppLevelMeter))]
internal static class UiAppLevelMeterPatch
{
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

[Expansion(typeof(UiAppLevelMeter))]
public partial class ExpandedUiAppLevelMeter
{
    public IExpInfo ExpDisplay;

    internal void Populate()
    {
        if (ExpDisplay == null) return;

        _core.meterFront.fillAmount = ExpDisplay.Percentage;
    }

    internal void OnTooltipPreShow_Postfix()
    {
        if (ExpDisplay == null) return;

        _core.smoothieSlot.itemSlot.tooltip.categoryLabel.text = ExpDisplay.ExpDesc;
    }
}
