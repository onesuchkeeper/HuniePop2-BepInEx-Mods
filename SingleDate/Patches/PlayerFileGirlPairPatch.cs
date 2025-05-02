using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(PlayerFileGirlPair))]
public static class PlayerFileGirlPairPatch
{
    [HarmonyPatch(nameof(PlayerFileGirlPair.RelationshipLevelUp))]
    [HarmonyPrefix]
    public static bool RelationshipLevelUp(PlayerFileGirlPair __instance)
    {
        if (!State.IsSingle(__instance.girlPairDefinition)
            || __instance.relationshipType == GirlPairRelationshipType.LOVERS)
        {
            return true;
        }

        __instance.relationshipType++;
        if (!__instance.girlPairDefinition.specialPair)
        {
            if (__instance.relationshipType == GirlPairRelationshipType.ATTRACTED
                || __instance.relationshipType == GirlPairRelationshipType.LOVERS)
            {
                Game.Persistence.playerFile.relationshipUpCount++;
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionOne).relationshipUpCount++;
                Game.Persistence.playerFile.GetPlayerFileGirl(__instance.girlPairDefinition.girlDefinitionTwo).relationshipUpCount++;
            }

            if (__instance.relationshipType == GirlPairRelationshipType.COMPATIBLE
                && !Game.Persistence.playerFile.metGirlPairs.Contains(__instance.girlPairDefinition))
            {
                Game.Persistence.playerFile.metGirlPairs.Add(__instance.girlPairDefinition);
            }

            //don't add single pairs to met pairs
            // if (__instance.relationshipType == GirlPairRelationshipType.LOVERS
            //     && !Game.Persistence.playerFile.completedGirlPairs.Contains(__instance.girlPairDefinition))
            // {
            //     Game.Persistence.playerFile.completedGirlPairs.Add(__instance.girlPairDefinition);
            // }
        }

        Game.Persistence.playerFile.KeyValueChanged();

        return false;
    }
}
