using HarmonyLib;

namespace Hp2BaseMod.EnumExpansion
{
    [HarmonyPatch(typeof(LocationManager), "Awake")]
    public static class LocationManager_Awake
    {
        public static void Postfix(LocationManager __instance)
        {
            var dateLocations = Game.Data.Locations.GetAllByLocationType(LocationType.DATE).ToArray();
            for (int i = 0; i < Game.Session.Location.dateLocationsInfos.Length; i++)
            {
                Game.Session.Location.dateLocationsInfos[i].locationDefinitions = dateLocations;
            }
        }
    }
}
