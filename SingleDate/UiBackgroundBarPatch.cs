using HarmonyLib;
using Hp2BaseMod;

namespace SingleDate;

[HarmonyPatch(typeof(UiBackgroundBar))]
public static class UiBackgroundBarPatch
{
    [HarmonyPatch(nameof(UiBackgroundBar.Refresh))]
    [HarmonyPostfix]
    public static void Refresh(UiBackgroundBar __instance, LocationDefinition locationDef, int daytimeElapsed, GirlPairDefinition girlPairDef = null, bool sidesFlipped = false)
    {
        if (!State.IsSingle(girlPairDef))
        {
            return;
        }

        __instance.daytimeLabel.text = $"{((ClockDaytimeType)(daytimeElapsed % 4)).ToString().ToUpper()} | {girlPairDef.girlDefinitionTwo.girlName}";
    }
}