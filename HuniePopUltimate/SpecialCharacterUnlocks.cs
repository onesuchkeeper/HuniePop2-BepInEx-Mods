using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using HarmonyLib;

namespace HuniePopUltimate;

internal static class SpecialCharacterUnlocks
{
    [HarmonyPatch(typeof(UiCellphoneTrashZone))]
    private static class UiCellphoneTrashZonePatch
    {
        private static readonly RelativeId _goldFishPlushId = new RelativeId(-1, 45);

        [HarmonyPatch("OnDrop")]
        [HarmonyPrefix]
        public static void OnDrop(UiCellphoneTrashZone __instance, Draggable draggable)
        {
            var id = draggable.GetItemDefinition()?.ModId();
            if (id == _goldFishPlushId)
            {
                ModInterface.Log.Message("Threw out the gold fish!");
                SpecialCharacterUnlocks._threwOutGoldfish = true;
            }
        }
    }

    private static bool _threwOutGoldfish { get; set; }

    internal static void On_FinderSlotSelected(UiAppFinderSlot slot) => _finderPending = true;
    private static bool _finderPending;

    internal static void On_PreLocationDepart(LocationDepartArgs args)
    {
        // Only handle finder departures
        if (!Plugin.HasSingleDate
            || !_finderPending) return;

        var nobodyDef = ModInterface.GameData.GetGirl(Plugin.SingleDateNobodyId);
        var previousLocId = args.from?.ModId();

        if (VenusOverride(nobodyDef, args))
        {
            _threwOutGoldfish = false;
            _finderPending = false;
            return;
        }

        if (KyuOverride(nobodyDef, args)
            || CelesteOverride(previousLocId, args)
            || MomoOverride(previousLocId, args))
        {
            ModInterface.Log.Message("HpUltimate overriding depart");
        }

        _threwOutGoldfish = false;
        _finderPending = false;
    }

    private static bool KyuOverride(GirlDefinition nobodyDef, LocationDepartArgs args)
    {
        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            var pair = ModInterface.GameData.GetGirlPair(Pairs.KyuSingleDate);
            var pairSave = Game.Persistence.playerFile.GetPlayerFileGirlPair(pair);

            if (pairSave.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                if (Game.Session.Location.currentGirlPair?.girlDefinitionOne == nobodyDef)
                {
                    ModInterface.Log.Message("Starting Kyu cutscene");
                    args.transition = new LocationTransitionFakeOut(args.to, GameStateId.Sim);
                    args.to = args.from;
                    args.girlPairDef = pairSave.girlPairDefinition;
                    ModInterface.State.CellphoneOnLeft = true;
                    return true;
                }
            }
        }
        return false;
    }

    private static bool VenusOverride(GirlDefinition nobodyDef, LocationDepartArgs args)
    {
        var pairId = args.girlPairDef?.ModId();

        if (pairId.HasValue && pairId.Value == Pairs.KyuSingleDate)
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
                    args.girlPairDef = pair;
                    ModInterface.State.CellphoneOnLeft = true;
                    return true;
                }
            }
        }
        return false;
    }

    private static bool CelesteOverride(RelativeId? previousLocId, LocationDepartArgs args)
    {
        var rawTime = (Game.Persistence.playerFile.daytimeElapsed - 1) % 4;
        var time = (ClockDaytimeType)((rawTime + 4) % 4);
        var isTimeValid = time == ClockDaytimeType.EVENING || time == ClockDaytimeType.NIGHT;

        if (previousLocId == LocationIds.Beach && isTimeValid)
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

                    args.transition = new LocationTransitionFakeOut(args.to, GameStateId.Sim);
                    args.to = args.from;
                    args.girlPairDef = pair;
                    ModInterface.State.CellphoneOnLeft = true;
                    return true;
                }
            }
        }
        return false;
    }

    private static bool MomoOverride(RelativeId? previousLocId, LocationDepartArgs args)
    {
        if (previousLocId == LocationIds.Park && _threwOutGoldfish)
        {
            var pair = ModInterface.GameData.GetGirlPair(Girls.Momo);
            var pairSave = Game.Persistence.playerFile.GetPlayerFileGirlPair(pair);

            if (pairSave.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                ModInterface.Log.Message("Starting Momo cutscene");

                args.transition = new LocationTransitionFakeOut(args.to, GameStateId.Sim);
                args.to = args.from;
                args.girlPairDef = pair;
                ModInterface.State.CellphoneOnLeft = true;
                return true;
            }
        }
        return false;
    }
}
