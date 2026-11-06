using Hp2BaseMod;

namespace SingleDate;

/// <summary>Relationship bookkeeping for single pairs.</summary>
internal static class SingleDateRelationship
{
    /// <summary>
    /// Levels a single pair up one relationship type (unknown, compatible, attracted, lovers). Does the same bookkeeping
    /// as <see cref="PlayerFileGirlPair.RelationshipLevelUp"/> (the player's and both girls' relationship up counts when
    /// reaching attracted or lovers, and the met pairs list when compatible) except it never adds the pair to the
    /// completed pairs: single pairs are not part of the base game's 24 and would otherwise make bonus rounds harder.
    /// (This is what <c>PlayerFileGirlPairPatch</c> did.)
    /// </summary>
    public static void LevelUp(PlayerFileGirlPair pair)
    {
        if (pair.relationshipType == GirlPairRelationshipType.LOVERS) return;

        var playerFile = Game.Persistence.playerFile;
        pair.relationshipType++;

        if (!pair.girlPairDefinition.specialPair)
        {
            if (pair.relationshipType == GirlPairRelationshipType.ATTRACTED
                || pair.relationshipType == GirlPairRelationshipType.LOVERS)
            {
                playerFile.relationshipUpCount++;
                playerFile.GetPlayerFileGirl(pair.girlPairDefinition.girlDefinitionOne).relationshipUpCount++;
                playerFile.GetPlayerFileGirl(pair.girlPairDefinition.girlDefinitionTwo).relationshipUpCount++;
            }

            if (pair.relationshipType == GirlPairRelationshipType.COMPATIBLE
                && !playerFile.metGirlPairs.Contains(pair.girlPairDefinition))
            {
                playerFile.metGirlPairs.Add(pair.girlPairDefinition);
            }
        }

        playerFile.KeyValueChanged();
    }
}