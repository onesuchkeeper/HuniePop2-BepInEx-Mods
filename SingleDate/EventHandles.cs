using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

internal static class EventHandles
{
    private static readonly FieldInfo _altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");

    private static bool _singleDropZone;

    internal static void On_PreLocationTransitionNormalSequencePlay(LocationArriveSequenceArgs sequence)
    {
        var rightOuter = Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.OUTER);
        var rightSinglePos = (Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.INNER) + rightOuter) / 2f;
        var diff = rightSinglePos - rightOuter - rightSinglePos;
        var rightDrop = Game.Session.gameCanvas.dollRight.dropZone.transform.position;

        if (State.IsSingleDate)
        {
            if (!_singleDropZone)
            {
                _singleDropZone = true;
                Game.Session.gameCanvas.dollRight.dropZone.transform.position = new Vector3(rightDrop.x - diff.x, rightDrop.y - diff.y, rightDrop.z);
            }

            var delta = Game.Session.gameCanvas.header.xValues.y - Game.Session.gameCanvas.header.xValues.x;

            Game.Session.Puzzle.puzzleGrid.transform.position = new Vector3(
                State.DefaultPuzzleGridPosition.x + delta,
                State.DefaultPuzzleGridPosition.y,
                0);
        }
        else
        {
            if (_singleDropZone)
            {
                _singleDropZone = false;
                Game.Session.gameCanvas.dollRight.dropZone.transform.position = new Vector3(rightDrop.x + diff.x, rightDrop.y + diff.y, rightDrop.z);
            }

            Game.Session.Puzzle.puzzleGrid.transform.position = State.DefaultPuzzleGridPosition;
        }
    }

    internal static void On_RandomDollSelected(RandomDollSelectedArgs args)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        ModInterface.Log.LogInfo("Forcing focus for single date");

        args.SelectedDoll = Game.Session.gameCanvas.dollRight;

        //here we force the focus of the puzzle grid because the initial puzzle 'NextRound' occurs before the current pair is set
        _altGirlFocused.SetValue(Game.Session.Puzzle.puzzleStatus, true);
    }

    internal static void On_FinderSlotsPopulate(FinderSlotPopulateEventArgs args)
    {
        args.SexPool?.RemoveAll(x => State.IsSingle(x.girlPairDefinition)
            && State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != State.MaxSingleGirlRelationshipLevel - 1);

        foreach (var id in args.SexPool)
        {
            ModInterface.Log.LogInfo(id.ToString());
        }

        if (State.RequireLoversBeforeThreesome)
        {
            args.SexPool?.RemoveAll(x => !State.IsSingle(x.girlPairDefinition)
                && (State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionOne.id)?.RelationshipLevel != State.MaxSingleGirlRelationshipLevel
                || State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != State.MaxSingleGirlRelationshipLevel));
        }

        args.CompatiblePool.AddRange(args.AttractedPool.Where(IsIncompleteAttracted));
        args.AttractedPool.RemoveAll(IsIncompleteAttracted);
    }

    private static bool IsIncompleteAttracted(PlayerFileGirlPair filePair) => State.IsSingle(filePair.girlPairDefinition)
        && State.SaveFile.GetGirl(filePair.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel < (State.MaxSingleGirlRelationshipLevel - 1);

    private static FieldInfo _roundOverCutscene = AccessTools.Field(typeof(PuzzleManager), "_roundOverCutscene");
    private static FieldInfo _newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");

    internal static void On_PreRoundOverCutscene()
    {
        if (Game.Session.Puzzle.puzzleStatus.statusType != PuzzleStatusType.NORMAL
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

        bool validSingleLevels = false;
        if (State.IsSingleDate)
        {
            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            //single date relationship levels are handled post round over, so girl will be at max already for bonus round
            validSingleLevels = girlSave?.RelationshipLevel == State.MaxSingleGirlRelationshipLevel;
        }
        else if (State.RequireLoversBeforeThreesome)
        {
            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            validSingleLevels = (girlSaveOne == null || girlSaveOne.RelationshipLevel == State.MaxSingleGirlRelationshipLevel)
                && (girlSaveTwo == null || girlSaveTwo.RelationshipLevel == State.MaxSingleGirlRelationshipLevel);
        }

        //disable the bonus round for dates without the correct single relationship level
        if (!validSingleLevels && Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            ModInterface.Log.LogInfo("Invalid single date level, changing from bonus round to cutscene success");
            Game.Session.Puzzle.puzzleStatus.gameOver = true;
            _roundOverCutscene.SetValue(Game.Session.Puzzle, Game.Session.Puzzle.cutsceneSuccess);
            _newRoundCutscene.SetValue(Game.Session.Puzzle, null);
        }
    }

    internal static void On_DateLocationSelected(DateLocationSelectedArgs args)
    {
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null)
        {
            return;
        }

        bool sexDate;

        if (State.IsSingleDate)
        {
            args.LeftStaminaGain = 0;
            args.RightStaminaGain = 0;

            args.PlayerPoints = 5;
            args.LeftPoints = 0;
            args.RightPoints = 5;

            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            sexDate = playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && girlSave.RelationshipLevel == State.MaxSingleGirlRelationshipLevel - 1
                && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime;
        }
        else
        {
            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            sexDate = girlSaveOne?.RelationshipLevel == State.MaxSingleGirlRelationshipLevel
                && girlSaveTwo?.RelationshipLevel == State.MaxSingleGirlRelationshipLevel
                && playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime;
        }

        args.Location = sexDate ? playerFileGirlPair.girlPairDefinition.sexLocationDefinition : null;
    }
}
