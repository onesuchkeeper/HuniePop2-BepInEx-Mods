using System;
using HarmonyLib;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiAppPairSlot), "Start")]
internal static class UiAppPairSlotPatch_Start
{
    public static void Postfix(UiAppPairSlot __instance)
    {
        __instance.girlHeadOne.preserveAspect = true;
        __instance.girlHeadTwo.preserveAspect = true;
    }
}
