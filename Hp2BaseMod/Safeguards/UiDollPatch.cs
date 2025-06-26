using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiDoll))]
public static class UiDollPatch
{
    [HarmonyPatch(nameof(UiDoll.ShowEnergySurge))]
    [HarmonyPrefix]
    public static bool ShowEnergySurge(UiDoll __instance, EnergyDefinition energyDef, float duration, bool knockback, bool negative = false, bool silent = false)
    {
        if (__instance.girlDefinition != null)
        {
            return true;
        }

        return false;
    }
}