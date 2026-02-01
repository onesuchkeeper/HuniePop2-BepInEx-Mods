using System.Linq;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace HuniePopUltimate;

[HarmonyPatch(typeof(LocationManager))]
public static class LocationManagerPatch
{
    private static readonly LocationTransitionFakeOut _fakeTransition = new();
    private static bool _removeOverride;
    private static LocationType? _overrideLocationType;

    [HarmonyPatch(nameof(LocationManager.AtLocationType))]
    [HarmonyPostfix]
    public static void AtLocationType(LocationType[] locationTypes, ref bool __result)
    {
        if (_overrideLocationType.HasValue)
        {
            __result = locationTypes.Contains(_overrideLocationType.Value);
        }
    }

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
        var kyuDef = ModInterface.GameData.GetGirl(Hp2BaseMod.Girls.Kyu);
        var momoDef = ModInterface.GameData.GetGirl(Girls.Momo);
        var celesteDef = ModInterface.GameData.GetGirl(Girls.Celeste);
        var currentLocId = __instance.currentLocation.ModId();

        if (VenusOverride(nobodyDef, momoDef, celesteDef, ref girlPairDef))
        {
            Plugin.ThrewOutGoldfish = false;
            return true;
        }

        var shouldOverride = KyuOverride(__instance, kyuDef, nobodyDef, locationDef, sidesFlipped)
            || CelesteOverride(currentLocId, locationDef, celesteDef, sidesFlipped)
            || MomoOverride(currentLocId, momoDef, locationDef, sidesFlipped);

        if (!shouldOverride)
        {
            _removeOverride = true;
        }

        Plugin.ThrewOutGoldfish = false;
        return !shouldOverride;
    }

    [HarmonyPatch(nameof(LocationManager.Arrive))]
    [HarmonyPrefix]
    public static void Arrive()
    {
        // we can't remove the override when departing
        // since it's needed for the transition, so instead
        // remove it here
        if (_removeOverride)
        {
            _removeOverride = false;
            _overrideLocationType = null;
        }
    }

    private static bool KyuOverride(LocationManager locationManager,
        GirlDefinition kyuDef,
        GirlDefinition nobodyDef,
        LocationDefinition locationDef,
        bool sidesFlipped)
    {
        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            var kyuSave = Game.Persistence.playerFile.GetPlayerFileGirl(kyuDef);
            if (!kyuSave.playerMet)
            {
                if (locationManager.currentGirlPair.girlDefinitionOne == nobodyDef)
                {
                    ModInterface.Log.Message("Starting Kyu cutscene");
                    _fakeTransition.Depart(locationDef, ModInterface.GameData.GetGirlPair(Pairs.KyuSingleDate), sidesFlipped);
                    ModInterface.State.CellphoneOnLeft = true;
                    _overrideLocationType = LocationType.SIM;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool VenusOverride(GirlDefinition nobodyDef,
        GirlDefinition momoDef,
        GirlDefinition celesteDef,
        ref GirlPairDefinition girlPairDef)
    {
        var pairId = girlPairDef?.ModId();

        // I'm only doing when kyu is at a sim loc, not at the hub
        // the hub is too coupled and messy and I don't wanna fix it
        if (pairId.HasValue
            && pairId.Value == Pairs.KyuSingleDate)
        {
            var venusDef = ModInterface.GameData.GetGirl(Girls.Venus);
            var venusSave = Game.Persistence.playerFile.GetPlayerFileGirl(venusDef);
            if (!venusSave.playerMet)
            {
                if (Game.Persistence.playerFile.girlPairs
                    .Where(x => x.girlPairDefinition.girlDefinitionOne == nobodyDef
                        && x.girlPairDefinition.girlDefinitionTwo != venusDef
                        && x.girlPairDefinition.girlDefinitionTwo != momoDef
                        && x.girlPairDefinition.girlDefinitionTwo != celesteDef)
                    .All(x => x.relationshipType == GirlPairRelationshipType.LOVERS))
                {
                    ModInterface.Log.Message("Changing pair to venus");
                    girlPairDef = ModInterface.GameData.GetGirlPair(Girls.Venus);
                    ModInterface.State.CellphoneOnLeft = true;
                    _overrideLocationType = LocationType.SIM;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool CelesteOverride(RelativeId currentLocId,
        LocationDefinition locationDef,
        GirlDefinition celesteDef,
        bool sidesFlipped)
    {
        var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4) - 1;
        var isTimeValid = time == ClockDaytimeType.EVENING || time == ClockDaytimeType.NIGHT;

        ModInterface.Log.Message($"Checking for celeste unlock scene, isTimeValid: {isTimeValid}");

        if (currentLocId == LocationIds.Beach
            && isTimeValid)
        {
            var weirdThing = ModInterface.GameData.GetItem(Items.WeirdThing);

            if (Game.Persistence.playerFile.IsItemInInventory(weirdThing, false))
            {
                var girlSave = Game.Persistence.playerFile.GetPlayerFileGirl(celesteDef);
                if (!girlSave.playerMet)
                {
                    ModInterface.Log.Message("Starting Celeste cutscene");

                    Game.Persistence.playerFile.inventorySlots
                        .Where(x => x.itemDefinition == weirdThing)
                        .ForEach(x => x.Clear());

                    _fakeTransition.Depart(locationDef, ModInterface.GameData.GetGirlPair(Girls.Celeste), sidesFlipped);
                    ModInterface.State.CellphoneOnLeft = true;
                    _overrideLocationType = LocationType.SIM;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool MomoOverride(RelativeId currentLocId,
        GirlDefinition momoDef,
        LocationDefinition locationDef,
        bool sidesFlipped)
    {
        if (currentLocId == LocationIds.Park
            && Plugin.ThrewOutGoldfish)
        {
            var girlSave = Game.Persistence.playerFile.GetPlayerFileGirl(momoDef);
            if (!girlSave.playerMet)
            {
                ModInterface.Log.Message("Starting Momo cutscene");
                _fakeTransition.Depart(locationDef, ModInterface.GameData.GetGirlPair(Girls.Momo), sidesFlipped);
                ModInterface.State.CellphoneOnLeft = true;
                _overrideLocationType = LocationType.SIM;
                return true;
            }
        }

        return false;
    }
}
