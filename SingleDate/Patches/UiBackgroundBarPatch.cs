using HarmonyLib;

namespace SingleDate;

/// <summary>Fixes the location bar's label for single pairs.</summary>
[HarmonyPatch(typeof(UiBackgroundBar))]
internal static class UiBackgroundBarPatch
{
    /// <summary>The bar shows the time of day and both girls' names, for single pairs it shows only the real girl's name.</summary>
    [HarmonyPatch(nameof(UiBackgroundBar.Refresh))]
    [HarmonyPostfix]
    public static void Refresh(UiBackgroundBar __instance, LocationDefinition locationDef, int daytimeElapsed, GirlPairDefinition girlPairDef = null, bool sidesFlipped = false)
    {
        // The label for the bar uses both girls in the pair, for single pair use only the girlTwo
        if (!State.IsSingle(girlPairDef))
        {
            return;
        }

        __instance.daytimeLabel.text = $"{((ClockDaytimeType)(daytimeElapsed % 4)).ToString().ToUpper()} | {girlPairDef.girlDefinitionTwo.girlName}";
    }
}
