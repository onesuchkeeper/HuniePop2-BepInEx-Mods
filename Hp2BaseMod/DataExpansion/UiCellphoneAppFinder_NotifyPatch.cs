using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiCellphoneAppFinder))]
internal static class UiCellphoneAppFinder_NotifyPatch
{
    [HarmonyPatch("OnFinderSlotSelected")]
    [HarmonyPrefix]
    public static void OnFinderSlotSelected_Prefix(UiCellphoneAppFinder __instance, UiAppFinderSlot finderSlot)
        => ModInterface.Events.NotifyFinderSlotSelected(finderSlot);
}