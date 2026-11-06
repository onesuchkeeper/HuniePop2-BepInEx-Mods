// using HarmonyLib;

// namespace SingleDate;

// /// <summary>Changes how single pairs level up their relationship.</summary>
// [HarmonyPatch(typeof(PlayerFileGirlPair))]
// internal static class PlayerFileGirlPairPatch
// {
//     /// <summary>
//     /// Levels a single pair up one relationship type (unknown, compatible, attracted, lovers). Does the same
//     /// bookkeeping as the base game (the player's and both girls' relationship up counts when reaching attracted or
//     /// lovers, and the met pairs list when compatible), but for a pair whose girl one is Nobody. Double pairs and
//     /// pairs that are already lovers use the original.
//     /// </summary>
//     /// <returns>True to run the original (double pairs and lovers), false for single pairs.</returns>
//     [HarmonyPatch(nameof(PlayerFileGirlPair.RelationshipLevelUp))]
//     [HarmonyPrefix]
//     public static bool RelationshipLevelUp(PlayerFileGirlPair __instance)
//     {
//         if (!State.IsSingle(__instance.girlPairDefinition)
//             || __instance.relationshipType == GirlPairRelationshipType.LOVERS)
//         {
//             return true;
//         }

//         __instance.relationshipType++;
//         if (!__instance.girlPairDefinition.specialPair)
//         {
//             if (__instance.relationshipType == GirlPairRelationshipType.ATTRACTED
//                 || __instance.relationshipType == GirlPairRelationshipType.LOVERS)
//             {
//                 Game.Persistence.playerFile.relationshipUpCount++;
//                 Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionOne).relationshipUpCount++;
//                 Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionTwo).relationshipUpCount++;
//             }

//             if (__instance.relationshipType == GirlPairRelationshipType.COMPATIBLE
//                 && !Game.Persistence.playerFile.metGirlPairs.Contains(__instance.girlPairDefinition))
//             {
//                 Game.Persistence.playerFile.metGirlPairs.Add(__instance.girlPairDefinition);
//             }
//         }

//         Game.Persistence.playerFile.KeyValueChanged();

//         return false;
//     }
// }
