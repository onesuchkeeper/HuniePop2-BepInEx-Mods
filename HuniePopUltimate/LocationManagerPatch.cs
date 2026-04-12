using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace HuniePopUltimate;

[HarmonyPatch(typeof(LocationManager))]
public static class LocationManagerPatch
{
    private static readonly LocationTransitionFakeOut _fakeTransition = new();

    [HarmonyPatch(nameof(LocationManager.Depart))]
    [HarmonyPrefix]
    public static bool Depart(LocationManager __instance, LocationDefinition locationDef, ref GirlPairDefinition girlPairDef, bool sidesFlipped)
    {
        if (!Plugin.HasSingleDate) return true;

        // ignore hub to hub transition (sleep) cuz I don't want to deal with it...
        if (__instance.currentLocation.locationType == LocationType.HUB
            && locationDef.locationType == LocationType.HUB)
        {
            return true;
        }

        // and dates
        if (locationDef.locationType == LocationType.DATE)
        {
            return true;
        }

        var nobodyDef = ModInterface.GameData.GetGirl(Plugin.SingleDateNobodyId);
        var currentLocId = __instance.currentLocation.ModId();

        if (VenusOverride(nobodyDef, ref girlPairDef))
        {
            Plugin.ThrewOutGoldfish = false;
            return true;
        }

        var shouldOverride =!(KyuOverride(__instance, nobodyDef, locationDef, sidesFlipped)
            || CelesteOverride(currentLocId, locationDef, sidesFlipped)
            || MomoOverride(currentLocId, locationDef, sidesFlipped));

        Plugin.ThrewOutGoldfish = false;
        return shouldOverride;
    }

    private static bool KyuOverride(LocationManager locationManager,
        GirlDefinition nobodyDef,
        LocationDefinition locationDef,
        bool sidesFlipped)
    {
        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            var kyuPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(ModInterface.GameData.GetGirlPair(Pairs.KyuSingleDate));

            if (kyuPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                if (locationManager.currentGirlPair.girlDefinitionOne == nobodyDef)
                {
                    ModInterface.Log.Message("Starting Kyu cutscene");
                    _fakeTransition.Depart(locationDef, ModInterface.GameData.GetGirlPair(Pairs.KyuSingleDate), sidesFlipped);
                    ModInterface.State.CellphoneOnLeft = true;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool VenusOverride(GirlDefinition nobodyDef,
        ref GirlPairDefinition girlPairDef)
    {
        var pairId = girlPairDef?.ModId();

        if (pairId.HasValue
            && pairId.Value == Pairs.KyuSingleDate)
        {
            var pair = ModInterface.GameData.GetGirlPair(Girls.Venus);
           
            var pairSave = Game.Persistence.playerFile.GetPlayerFileGirlPair(pair);
            if (pairSave.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                var venusDef = ModInterface.GameData.GetGirl(Girls.Venus);
                var momoDef = ModInterface.GameData.GetGirl(Girls.Momo);
                var celesteDef = ModInterface.GameData.GetGirl(Girls.Celeste);

                if (Game.Persistence.playerFile.girlPairs
                    .Where(x => x.girlPairDefinition.girlDefinitionOne == nobodyDef
                        && x.girlPairDefinition.girlDefinitionTwo != venusDef
                        && x.girlPairDefinition.girlDefinitionTwo != momoDef
                        && x.girlPairDefinition.girlDefinitionTwo != celesteDef)
                    .All(x => x.relationshipType == GirlPairRelationshipType.LOVERS))
                {
                    ModInterface.Log.Message("Changing pair to venus");
                    girlPairDef = pair;
                    ModInterface.State.CellphoneOnLeft = true;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool CelesteOverride(RelativeId currentLocId,
        LocationDefinition locationDef,
        bool sidesFlipped)
    {
        //time gets updated, so -1 to checks. Not handling data location anymore so 
        //don't have to worry about date transitions not updating time
        var time = (ClockDaytimeType)((Game.Persistence.playerFile.daytimeElapsed - 1) % 4);
        var isTimeValid = time == ClockDaytimeType.EVENING || time == ClockDaytimeType.NIGHT;

        if (currentLocId == LocationIds.Beach
            && isTimeValid)
        {
            var weirdThing = ModInterface.GameData.GetItem(Items.WeirdThing);

            if (Game.Persistence.playerFile.IsItemInInventory(weirdThing, false))
            {
                var pair = ModInterface.GameData.GetGirlPair(Girls.Celeste);
                var pairSave = Game.Persistence.playerFile.GetPlayerFileGirlPair(pair);

                if (pairSave.relationshipType == GirlPairRelationshipType.UNKNOWN)
                {
                    ModInterface.Log.Message("Starting Celeste cutscene");

                    Game.Persistence.playerFile.inventorySlots
                        .Where(x => x.itemDefinition == weirdThing)
                        .ForEach(x => x.Clear());

                    _fakeTransition.Depart(locationDef, ModInterface.GameData.GetGirlPair(Girls.Celeste), sidesFlipped);
                    ModInterface.State.CellphoneOnLeft = true;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool MomoOverride(RelativeId currentLocId,
        LocationDefinition locationDef,
        bool sidesFlipped)
    {
        if (currentLocId == LocationIds.Park
            && Plugin.ThrewOutGoldfish)
        {
            var pair = ModInterface.GameData.GetGirlPair(Girls.Momo);
            var pairSave = Game.Persistence.playerFile.GetPlayerFileGirlPair(pair);

            if (pairSave.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                ModInterface.Log.Message("Starting Momo cutscene");
                _fakeTransition.Depart(locationDef, pair, sidesFlipped);
                ModInterface.State.CellphoneOnLeft = true;
                return true;
            }
        }

        return false;
    }
}
