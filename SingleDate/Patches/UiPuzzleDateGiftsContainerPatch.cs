using HarmonyLib;

namespace SingleDate;

/// <summary>Adjusts the gift slots on the date screen for the shifted single date layout.</summary>
[HarmonyPatch(typeof(UiPuzzleDateGiftsContainer))]
internal static class UiPuzzleDateGiftsContainerPatch
{
    /// <summary>Makes the right girl's gift slot tooltips open toward the east, so they don't run off screen when the UI is shifted.</summary>
    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    public static void Start(UiPuzzleDateGiftsContainer __instance)
    {
        // uses east on hub so tooltips for the slots don't go off the screen when the ui is shifted
        foreach (var slot in __instance.slotsRight)
        {
            slot.itemSlot.eastOnHub = true;
        }
    }
}
