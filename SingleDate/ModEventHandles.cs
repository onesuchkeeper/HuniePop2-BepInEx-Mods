using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace SingleDate;

/// <summary>
/// Handlers for the Hp2BaseMod events (<see cref="ModInterface.Events"/>) that SingleDate listens to for the whole run of
/// the game. Subscriptions are made in <see cref="Plugin"/>.
/// </summary>
/// <remarks>
/// What happens during a single date is in <see cref="SingleDateSim"/> and <see cref="SingleDatePuzzleState"/>. What is
/// left here either has no state (the cellphone, the photo gallery), picks the state (<see cref="On_ResolveStatePayload"/>),
/// or is the <see cref="Plugin.RequireLoversBeforeThreesome"/> rule for double dates.
/// </remarks>
internal static class ModEventHandles
{
    /// <summary>
    /// Every sim state for a single pair is a <see cref="SingleDateSim"/>. States, saves and logic actions ask for a
    /// <see cref="GameStateSim.Payload"/> and this swaps it, so saves made before this state existed still load.
    /// </summary>
    internal static void On_ResolveStatePayload(ModEvents.ResolveStatePayloadArgs args)
    {
        if (args.Payload is GameStateSim.Payload simPayload
            && State.IsSingle(ModInterface.GameData.GetGirlPair(simPayload.PairId)))
        {
            ModInterface.Log.Message("Replacing base game sim with single date sim");
            args.Payload = new SingleDateSim.Payload(simPayload.PairId);
        }
    }

    /// <summary>
    /// Adjusts the cellphone finder's pools so single date pairs only appear as sex candidates when they should.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>A single pair is only allowed in the sex pool when its girl is one level below
    /// <see cref="Plugin.MaxSingleGirlRelationshipLevel"/> (the level at which the sex date is available).</item>
    /// <item>If <see cref="Plugin.RequireLoversBeforeThreesome"/> is on, a double pair is only allowed in the sex pool
    /// when both girls have reached the max single date level.</item>
    /// <item>Single pairs the base game considers attracted, but whose single date level is still below the sex date
    /// level, are moved from the attracted pool to the compatible pool, so they keep showing up as regular dates.</item>
    /// </list>
    /// </remarks>
    internal static void On_FinderSlotsPopulate(FinderSlotPopulateEventArgs args)
    {
        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;

        args.SexPool?.RemoveAll(x => State.IsSingle(x.girlPairDefinition)
            && State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != maxSingleGirlRelationshipLevel - 1);

        if (Plugin.RequireLoversBeforeThreesome.Value)
        {
            args.SexPool?.RemoveAll(x => !State.IsSingle(x.girlPairDefinition)
                && (State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionOne.id)?.RelationshipLevel != maxSingleGirlRelationshipLevel
                || State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != maxSingleGirlRelationshipLevel));
        }

        args.CompatiblePool.AddRange(args.AttractedPool.Where(x => IsIncompleteAttracted(x, maxSingleGirlRelationshipLevel)));
        args.AttractedPool.RemoveAll(x => IsIncompleteAttracted(x, maxSingleGirlRelationshipLevel));
    }

    /// <summary>
    /// True for a single pair that the base game has marked as attracted but whose single date level has not yet
    /// reached the level below max (the level where the sex date unlocks). A girl with no save entry is not matched.
    /// </summary>
    private static bool IsIncompleteAttracted(PlayerFileGirlPair filePair, int maxSingleGirlRelationshipLevel) => State.IsSingle(filePair.girlPairDefinition)
        && State.SaveFile.GetGirl(filePair.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel < (maxSingleGirlRelationshipLevel - 1);

    /// <summary>
    /// Adds every photo unlocked through single dates (across all girls in this save file) to the list of photos the
    /// photo gallery shows as unlocked.
    /// </summary>
    internal static void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        args.UnlockedPhotos ??= new List<PhotoDefinition>();
        args.UnlockedPhotos.AddRange(State.SaveFile.Girls.Values.SelectManyNN(x => x.UnlockedPhotos).Select(ModInterface.GameData.GetPhoto));
    }

    // ---- Plugin.RequireLoversBeforeThreesome, for double dates. Single dates are in the states. ----

    /// <summary>
    /// On a double date with <see cref="Plugin.RequireLoversBeforeThreesome"/>: the bonus round and cutscene are denied
    /// while either girl is below the max single date level.
    /// </summary>
    internal static void On_PuzzleRoundOver(PuzzleRoundOverArgs args)
    {
        if (!Plugin.RequireLoversBeforeThreesome.Value
            || State.IsSingleDate
            || Game.Session.Puzzle.puzzleStatus.statusType != PuzzleStatusType.NORMAL
            || Game.Session.Puzzle.puzzleGrid.roundState != PuzzleRoundState.SUCCESS)
        {
            return;
        }

        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null
            || playerFileGirlPair.relationshipType != GirlPairRelationshipType.ATTRACTED)
        {
            return;
        }

        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;
        var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
        var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

        if ((girlSaveOne != null && girlSaveOne.RelationshipLevel < maxSingleGirlRelationshipLevel)
            || (girlSaveTwo != null && girlSaveTwo.RelationshipLevel < maxSingleGirlRelationshipLevel))
        {
            ModInterface.Log.Message("Deny double date bonus due to single date levels");
            args.IsSexDate = false;
            args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.None;
        }
    }

    /// <summary>
    /// On double dates with <see cref="Plugin.RequireLoversBeforeThreesome"/>, the pair's sex location is only used if both
    /// girls are at max single date level, otherwise it is cleared.
    /// </summary>
    internal static void On_DateLocationSelected(DateLocationSelectedArgs args)
    {
        if (!Plugin.RequireLoversBeforeThreesome.Value || State.IsSingleDate) return;

        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);
        if (playerFileGirlPair == null) return;

        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;
        var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
        var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

        if (girlSaveOne?.RelationshipLevel == maxSingleGirlRelationshipLevel
            && girlSaveTwo?.RelationshipLevel == maxSingleGirlRelationshipLevel
            && playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
            && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime)
        {
            args.Location = playerFileGirlPair.girlPairDefinition.sexLocationDefinition;
        }
        else
        {
            args.Location = null;
        }
    }

    /// <summary>
    /// On a double date (with <see cref="Plugin.RequireLoversBeforeThreesome"/>), the sex style is used when both girls
    /// are at max single date level.
    /// </summary>
    internal static void On_PreDateDollsRefresh(PreDateDollResetArgs args)
    {
        if (!Plugin.RequireLoversBeforeThreesome.Value || State.IsSingleDate) return;

        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);
        if (playerFileGirlPair == null) return;

        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;
        var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
        var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

        if ((girlSaveOne == null || girlSaveOne.RelationshipLevel == maxSingleGirlRelationshipLevel)
            && (girlSaveTwo == null || girlSaveTwo.RelationshipLevel == maxSingleGirlRelationshipLevel))
        {
            args.Style = PreDateDollResetArgs.StyleType.Sex;
        }
    }
}