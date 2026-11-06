using HarmonyLib;
using Hp2BaseMod;

namespace SingleDate;

/// <summary>Hides the trait icons for Nobody on status portraits.</summary>
[HarmonyPatch(typeof(UiStatusPortrait))]
internal static class UiStatusPortraitPatch
{
    /// <summary>Nobody has no traits to show, so the icons are turned off for her portrait.</summary>
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
