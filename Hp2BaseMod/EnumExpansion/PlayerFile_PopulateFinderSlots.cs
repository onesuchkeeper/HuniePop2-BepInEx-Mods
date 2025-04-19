using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// Allows special characters to show up in multiple pairings at the same time
/// 
/// If too many additional pairs are added,
/// there aren't enough location slots to accommodate and
/// the base PopulateFinderSlots doesn't account for that possibility
/// </summary>
[HarmonyPatch(typeof(PlayerFile), nameof(PlayerFile.PopulateFinderSlots))]
public static class PlayerFile_PopulateFinderSlots
{
    private static bool Prefix(PlayerFile __instance)
    {
        var playerFile = __instance;

        //start with all non-special girls 
        // and non-special pairs shuffled
        Dictionary<GirlDefinition, PlayerFileGirl> normalGirls = Game.Data.Girls.GetAllBySpecial(false).ToDictionary(x => x, playerFile.GetPlayerFileGirl);

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
        filePairs.RemoveAll(x => !((normalGirls.TryGetValue(x.girlPairDefinition.girlDefinitionOne, out var pairFileOne) && pairFileOne.playerMet)
                                    || (normalGirls.TryGetValue(x.girlPairDefinition.girlDefinitionTwo, out var pairFileTwo) && pairFileTwo.playerMet)));

        List<GirlPairDefinition> girlPairs = new List<GirlPairDefinition>();
        List<LocationDefinition> locations = new List<LocationDefinition>();

        //grab pairs from pool who will be able to become lovers
        var nextTime = (ClockDaytimeType)((playerFile.daytimeElapsed + 1) % 4);
        for (int l = 0; l < filePairs.Count; l++)
        {
            if (filePairs[l].relationshipType == GirlPairRelationshipType.ATTRACTED
                && nextTime == filePairs[l].girlPairDefinition.sexDaytime)
            {
                AddToFinderLists(girlPairs, locations, filePairs[l].girlPairDefinition, null, filePairs);
            }
        }

        //grab pairs from pool who meet at a different location and are an intro pair and 
        //who's location hasn't been used yet
        for (int m = 0; m < filePairs.Count; m++)
        {
            if (filePairs[m].relationshipType == GirlPairRelationshipType.UNKNOWN
                && filePairs[m].girlPairDefinition.introductionPair
                && filePairs[m].girlPairDefinition.meetingLocationDefinition != playerFile.locationDefinition
                && !locations.Contains(filePairs[m].girlPairDefinition.meetingLocationDefinition))
            {
                AddToFinderLists(girlPairs, locations, filePairs[m].girlPairDefinition, filePairs[m].girlPairDefinition.meetingLocationDefinition, filePairs);
            }
        }

        //compatible pairs (met, no successful dates)
        //or
        //grab pairs who the player has not met
        //who do not meet at an already used location
        //and the player has met both girls
        for (int n = 0; n < filePairs.Count; n++)
        {
            if (filePairs[n].relationshipType == GirlPairRelationshipType.COMPATIBLE
                ||
                (filePairs[n].relationshipType == GirlPairRelationshipType.UNKNOWN
                    && !filePairs[n].girlPairDefinition.introductionPair
                    && filePairs[n].girlPairDefinition.meetingLocationDefinition != playerFile.locationDefinition
                    && !locations.Contains(filePairs[n].girlPairDefinition.meetingLocationDefinition)
                    && (!normalGirls.TryGetValue(filePairs[n].girlPairDefinition.girlDefinitionOne, out var fileGirlOne) || fileGirlOne.playerMet)
                    && (!normalGirls.TryGetValue(filePairs[n].girlPairDefinition.girlDefinitionTwo, out var fileGirlTwo) || fileGirlTwo.playerMet)))
            {
                AddToFinderLists(girlPairs,
                    locations,
                    filePairs[n].girlPairDefinition,
                    (filePairs[n].relationshipType == GirlPairRelationshipType.UNKNOWN)
                        ? filePairs[n].girlPairDefinition.meetingLocationDefinition
                        : null,
                    filePairs);
            }
        }

        //grab lovers
        for (int num = 0; num < filePairs.Count; num++)
        {
            if (filePairs[num].relationshipType == GirlPairRelationshipType.LOVERS)
            {
                AddToFinderLists(girlPairs, locations, filePairs[num].girlPairDefinition, null, filePairs);
            }
        }

        //if no pairs found, grab all attracted pairs (1 successful date)
        if (girlPairs.Count <= 0)
        {
            for (int num2 = 0; num2 < filePairs.Count; num2++)
            {
                if (filePairs[num2].relationshipType == GirlPairRelationshipType.ATTRACTED)
                {
                    AddToFinderLists(girlPairs, locations, filePairs[num2].girlPairDefinition, null, filePairs);
                }
            }
        }

        List<LocationDefinition> simLocations = Game.Data.Locations.GetAllByLocationType(LocationType.SIM);
        Dictionary<LocationDefinition, PlayerFileFinderSlot> locToFinderSlot = new Dictionary<LocationDefinition, PlayerFileFinderSlot>();

        //for each sim loc, grab and clear its slot
        foreach (var loc in simLocations)
        {
            PlayerFileFinderSlot playerFileFinderSlot = playerFile.GetPlayerFileFinderSlot(loc);
            playerFileFinderSlot.Clear();
            locToFinderSlot.Add(loc, playerFileFinderSlot);
        }

        //remove current location from location pool
        simLocations.Remove(playerFile.locationDefinition);

        //remove all instances of locations gathered when processing pairs from sim location pool
        simLocations.RemoveAll(locations.Contains);

        var i = 0;
        foreach (var pair in girlPairs)
        {
            //if the location is null, pick a random sim location from the pool
            var locationDefinition = locations[i++];
            if (locationDefinition == null)
            {
                //this is the change that fixes it
                //in the base there's not enough sim locs if you 
                //add extra pairs
                if (simLocations.Count == 0)
                {
                    continue;
                }

                locationDefinition = simLocations[Random.Range(0, simLocations.Count)];
                simLocations.Remove(locationDefinition);
            }

            //for unknown pairs yet to be introduced, use their into sidefilpped,
            //otherwise just use random flipping

            bool flipped = (!pair.specialPair
                && pair.introductionPair
                && playerFile.GetPlayerFileGirlPair(pair).relationshipType == GirlPairRelationshipType.UNKNOWN)
                    ? pair.introSidesFlipped
                    : MathUtils.RandomBool();

            //populate
            locToFinderSlot[locationDefinition].Populate(pair, flipped);
        }

        return false;
    }

    private static void AddToFinderLists(List<GirlPairDefinition> girlPairList,
        List<LocationDefinition> locationList,
        GirlPairDefinition girlPairDef,
        LocationDefinition locationDef,
        List<PlayerFileGirlPair> fileGirlPairs)
    {
        if (girlPairList != null
            && !girlPairList.Contains(girlPairDef))
        {
            girlPairList.Add(girlPairDef);

            //only add locations for pairs that haven't been handled?
            if (locationList != null
                && (locationDef == null || !locationList.Contains(locationDef)))
            {
                locationList.Add(locationDef);
            }
        }

        //remove file girl pairs with the provided pairs girls
        if (!girlPairDef.girlDefinitionOne.specialCharacter)
        {
            fileGirlPairs.RemoveAll(x => x.girlPairDefinition.girlDefinitionOne == girlPairDef.girlDefinitionOne
                || x.girlPairDefinition.girlDefinitionTwo == girlPairDef.girlDefinitionOne);
        }

        if (!girlPairDef.girlDefinitionTwo.specialCharacter)
        {
            fileGirlPairs.RemoveAll(x => x.girlPairDefinition.girlDefinitionOne == girlPairDef.girlDefinitionTwo
                || x.girlPairDefinition.girlDefinitionTwo == girlPairDef.girlDefinitionTwo);
        }
    }
}