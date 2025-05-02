using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(UiPuzzleDateGiftsContainer))]
internal static class UiPuzzleDateGiftsContainerPatch
{
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleDateGiftsContainer __instance)
    {
        foreach (var slot in __instance.slotsRight)
        {
            slot.itemSlot.eastOnHub = true;
        }
    }
}