using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Extension.IEnumerableExtension;

namespace Hp2BaseMod;

/// <summary>
/// Allows special characters to show up in multiple pairings at the same time
/// 
/// If too many additional pairs are added,
/// there aren't enough location slots to accommodate and
/// the base PopulateFinderSlots doesn't account for that possibility
/// 
/// also allows mods to validate pairs via <see cref="ModEvents.FinderSlotsPopulate"/>
/// </summary>
[HarmonyPatch(typeof(PlayerFile), nameof(PlayerFile.PopulateFinderSlots))]
public static class PlayerFile_PopulateFinderSlots
{
    private static bool Prefix(PlayerFile __instance)
    {
        var playerFile = __instance;

        var normalGirls = Game.Data.Girls.GetAllBySpecial(false).ToDictionary(x => x, playerFile.GetPlayerFileGirl);
        List<PlayerFileGirlPair> filePairs = Game.Data.GirlPairs.GetAllBySpecial(false).Select(playerFile.GetPlayerFileGirlPair).ToList();
        ListUtils.ShuffleList(filePairs);

        //remove pairs containing a girl from the current pair
        if (playerFile.girlPairDefinition != null)
        {
            if (!playerFile.girlPairDefinition.girlDefinitionOne.specialCharacter)
            {
                filePairs.RemoveAll(x => x.girlPairDefinition.girlDefinitionOne == playerFile.girlPairDefinition.girlDefinitionOne
                    || x.girlPairDefinition.girlDefinitionTwo == playerFile.girlPairDefinition.girlDefinitionOne);
            }

            if (!playerFile.girlPairDefinition.girlDefinitionTwo.specialCharacter)
            {
                filePairs.RemoveAll(x => x.girlPairDefinition.girlDefinitionOne == playerFile.girlPairDefinition.girlDefinitionTwo
                    || x.girlPairDefinition.girlDefinitionTwo == playerFile.girlPairDefinition.girlDefinitionTwo);
            }
        }

        //remove pairs where neither normal girl has been met
        filePairs.RemoveAll(x =>
            !((normalGirls.TryGetValue(x.girlPairDefinition.girlDefinitionOne, out var pairFileOne) && pairFileOne.playerMet)
                || (normalGirls.TryGetValue(x.girlPairDefinition.girlDefinitionTwo, out var pairFileTwo) && pairFileTwo.playerMet)));

        var nextTime = (ClockDaytimeType)((playerFile.daytimeElapsed + 1) % 4);

        var args = new FinderSlotPopulateEventArgs();

        args.SexPool = filePairs.Where(x =>
                x.relationshipType == GirlPairRelationshipType.ATTRACTED
                && nextTime == x.girlPairDefinition.sexDaytime)
            .ToList();

        args.IntroPool = filePairs.Where(x =>
                x.relationshipType == GirlPairRelationshipType.UNKNOWN
                && x.girlPairDefinition.introductionPair)
            .ToList();

        args.MeetingPool = filePairs.Where(x =>
                x.relationshipType == GirlPairRelationshipType.UNKNOWN
                && !x.girlPairDefinition.introductionPair
                && (!normalGirls.TryGetValue(x.girlPairDefinition.girlDefinitionOne, out var pairFileOne) || pairFileOne.playerMet)
                && (!normalGirls.TryGetValue(x.girlPairDefinition.girlDefinitionTwo, out var pairFileTwo) || pairFileTwo.playerMet))
            .ToList();

        args.CompatiblePool = filePairs.Where(x =>
                x.relationshipType == GirlPairRelationshipType.COMPATIBLE)
            .ToList();

        args.LoversPool = filePairs.Where(x =>
                x.relationshipType == GirlPairRelationshipType.LOVERS)
            .ToList();

        args.AttractedPool = filePairs.Where(x =>
                x.relationshipType == GirlPairRelationshipType.ATTRACTED)
            .ToList();

        var simLocationPool = Game.Data.Locations.GetAllByLocationType(LocationType.SIM);
        Dictionary<LocationDefinition, PlayerFileFinderSlot> locToFinderSlot = new Dictionary<LocationDefinition, PlayerFileFinderSlot>();

        foreach (var loc in simLocationPool)
        {
            var playerFileFinderSlot = playerFile.GetPlayerFileFinderSlot(loc);
            playerFileFinderSlot.Clear();
            locToFinderSlot.Add(loc, playerFileFinderSlot);
        }

        simLocationPool.Remove(playerFile.locationDefinition);
        args.LocationPool = simLocationPool;
        ModInterface.Events.NotifyPreFinderSlotPopulatePairs(args);

        var girlPool = normalGirls.Keys.ToList();

        if (args.SexPool != null)
        {
            foreach (var pairFile in args.SexPool)
            {
                if (args.LocationPool.Count == 0) { return false; }

                if ((pairFile.girlPairDefinition.girlDefinitionOne.specialCharacter || girlPool.Contains(pairFile.girlPairDefinition.girlDefinitionOne))
                    && (pairFile.girlPairDefinition.girlDefinitionTwo.specialCharacter || girlPool.Contains(pairFile.girlPairDefinition.girlDefinitionTwo)))
                {
                    girlPool.Remove(pairFile.girlPairDefinition.girlDefinitionOne);
                    girlPool.Remove(pairFile.girlPairDefinition.girlDefinitionTwo);

                    bool flipped = (!pairFile.girlPairDefinition.specialPair
                        && pairFile.girlPairDefinition.introductionPair
                        && pairFile.relationshipType == GirlPairRelationshipType.UNKNOWN)
                            ? pairFile.girlPairDefinition.introSidesFlipped
                            : MathUtils.RandomBool();

                    locToFinderSlot[args.LocationPool.PopRandom()].Populate(pairFile.girlPairDefinition, flipped);
                }
            }
        }

        foreach (var pairFile in args.IntroPool.OrEmptyIfNull().ConcatNN(args.MeetingPool).Distinct())
        {
            if (args.LocationPool.Count == 0) { return false; }

            if ((pairFile.girlPairDefinition.girlDefinitionOne.specialCharacter || girlPool.Contains(pairFile.girlPairDefinition.girlDefinitionOne))
                && (pairFile.girlPairDefinition.girlDefinitionTwo.specialCharacter || girlPool.Contains(pairFile.girlPairDefinition.girlDefinitionTwo))
                && args.LocationPool.Remove(pairFile.girlPairDefinition.meetingLocationDefinition))
            {
                girlPool.Remove(pairFile.girlPairDefinition.girlDefinitionOne);
                girlPool.Remove(pairFile.girlPairDefinition.girlDefinitionTwo);

                bool flipped = (!pairFile.girlPairDefinition.specialPair
                    && pairFile.girlPairDefinition.introductionPair
                    && pairFile.relationshipType == GirlPairRelationshipType.UNKNOWN)
                        ? pairFile.girlPairDefinition.introSidesFlipped
                        : MathUtils.RandomBool();

                locToFinderSlot[pairFile.girlPairDefinition.meetingLocationDefinition].Populate(pairFile.girlPairDefinition, flipped);
            }
        }

        foreach (var pairFile in args.CompatiblePool.OrEmptyIfNull()
            .ConcatNN(args.LoversPool)
            .ConcatNN(args.AttractedPool))
        {
            if (args.LocationPool.Count == 0) { return false; }

            if ((pairFile.girlPairDefinition.girlDefinitionOne.specialCharacter || girlPool.Contains(pairFile.girlPairDefinition.girlDefinitionOne))
                && (pairFile.girlPairDefinition.girlDefinitionTwo.specialCharacter || girlPool.Contains(pairFile.girlPairDefinition.girlDefinitionTwo)))
            {
                girlPool.Remove(pairFile.girlPairDefinition.girlDefinitionOne);
                girlPool.Remove(pairFile.girlPairDefinition.girlDefinitionTwo);

                bool flipped = (!pairFile.girlPairDefinition.specialPair
                    && pairFile.girlPairDefinition.introductionPair
                    && pairFile.relationshipType == GirlPairRelationshipType.UNKNOWN)
                        ? pairFile.girlPairDefinition.introSidesFlipped
                        : MathUtils.RandomBool();

                locToFinderSlot[args.LocationPool.PopRandom()].Populate(pairFile.girlPairDefinition, flipped);
            }
        }

        return false;
    }
}
