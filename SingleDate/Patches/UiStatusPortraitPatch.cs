using HarmonyLib;
using Hp2BaseMod;

namespace SingleDate;

[HarmonyPatch(typeof(UiStatusPortrait))]
internal static class UiStatusPortraitPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Populate")]
    public static void PopulatePrefix(GirlDefinition girlDef, ref bool showTraitIcons)
    {
        if (girlDef.ModId() == Girls.Nobody)
        {
            showTraitIcons = false;
        }
    }
}