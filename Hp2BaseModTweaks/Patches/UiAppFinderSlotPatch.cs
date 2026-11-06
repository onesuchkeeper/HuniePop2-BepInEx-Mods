using System.Linq;
using HarmonyLib;
using UnityEngine;
using Hp2BaseMod;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(UiAppFinderSlot))]
internal static class UiAppFinderSlotPatch
{
    [HarmonyPatch(nameof(UiAppFinderSlot.Populate))]
    [HarmonyPostfix]
    public static void Populate(UiAppFinderSlot __instance, bool settled)
    {
        // show hub girl even if not met
        if (__instance.locationDefinition.ModId() == Locations.HotelRoom)
        {
            var pairs = Game.Persistence.playerFile.finderSlots.Select(x => x.girlPairDefinition).AddItem(Game.Session.Location.currentGirlPair);

            if (pairs.Any(x => x != null
                && (x.girlDefinitionOne == Game.Session.Hub.hubGirlDefinition
                    || x.girlDefinitionTwo == Game.Session.Hub.hubGirlDefinition)))
            {
                __instance.headSlotLeft.Populate(null);
            }
            else
            {
                __instance.headSlotLeft.itemIcon.color = Color.white;
            }
        }
    }
}
