using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

internal static class ModEventHandles
{
    private static readonly FieldInfo f_altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");

    private static bool _singleDropZone = false;

    internal static void On_LocationArriveSequence(LocationArriveSequenceArgs sequence)
    {
        var rightOuter = Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.OUTER);
        var rightInnerPos = Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.INNER);
        var diff = rightInnerPos - rightOuter;
        var puzzleGridRectTransform = Game.Session.Puzzle.puzzleGrid.GetComponent<RectTransform>();

        if (State.IsSingleDate)
        {
            sequence.LeftDollPosition = DollPositionType.HIDDEN;
            sequence.RightDollPosition = DollPositionType.INNER;

            if (!_singleDropZone)
            {
                _singleDropZone = true;
                Game.Session.gameCanvas.dollRight.dropZone.transform.localPosition += new Vector3(diff.x, diff.y, 0);
            }

            Game.Session.gameCanvas.dollRight.slideLayer.anchoredPosition += diff;

            var delta = Game.Session.gameCanvas.header.xValues.y - Game.Session.gameCanvas.header.xValues.x;

            puzzleGridRectTransform.anchoredPosition = new Vector3(
                State.DefaultPuzzleGridPosition.x + delta,
                State.DefaultPuzzleGridPosition.y,
                0);
        }
        else
        {
            if (_singleDropZone)
            {
                _singleDropZone = false;
                Game.Session.gameCanvas.dollRight.dropZone.transform.localPosition -= new Vector3(diff.x, diff.y, 0);
            }

            puzzleGridRectTransform.anchoredPosition = State.DefaultPuzzleGridPosition;
        }
    }

    internal static void On_RandomDollSelected(RandomDollSelectedArgs args)
    {
        if (!State.IsSingleDate)
        {
            return;
        }

        ModInterface.Log.Message("Forcing focus for single date");

        args.SelectedDoll = Game.Session.gameCanvas.dollRight;

        //here we force the focus of the puzzle grid because the initial puzzle 'NextRound' occurs before the current pair is set
        f_altGirlFocused.SetValue(Game.Session.Puzzle.puzzleStatus, true);
    }

    internal static void On_FinderSlotsPopulate(FinderSlotPopulateEventArgs args)
    {
        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;

        args.SexPool?.RemoveAll(x => State.IsSingle(x.girlPairDefinition)
            && State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != maxSingleGirlRelationshipLevel - 1);

        foreach (var id in args.SexPool)
        {
            ModInterface.Log.Message(id.ToString());
        }

        if (Plugin.RequireLoversBeforeThreesome.Value)
        {
            args.SexPool?.RemoveAll(x => !State.IsSingle(x.girlPairDefinition)
                && (State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionOne.id)?.RelationshipLevel != maxSingleGirlRelationshipLevel
                || State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != maxSingleGirlRelationshipLevel));
        }

        args.CompatiblePool.AddRange(args.AttractedPool.Where(x => IsIncompleteAttracted(x, maxSingleGirlRelationshipLevel)));
        args.AttractedPool.RemoveAll(x => IsIncompleteAttracted(x, maxSingleGirlRelationshipLevel));
    }

    private static bool IsIncompleteAttracted(PlayerFileGirlPair filePair, int maxSingleGirlRelationshipLevel) => State.IsSingle(filePair.girlPairDefinition)
        && State.SaveFile.GetGirl(filePair.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel < (maxSingleGirlRelationshipLevel - 1);

    internal static void On_PuzzleRoundOver(PuzzleRoundOverArgs args)
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

        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);
        var girlSave = State.SaveFile.GetGirl(girlId);

        if (State.IsSingleDate)
        {
            var oneUnderMaxLevel = girlSave?.RelationshipLevel >= maxSingleGirlRelationshipLevel - 1;

            if ((Game.Session.Location.currentGirlPair.sexLocationDefinition == null
                    || Game.Session.Location.currentGirlPair.sexLocationDefinition == Game.Session.Location.currentLocation)
                && oneUnderMaxLevel
                && Game.Session.Location.currentGirlPair.sexDaytime == (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4))
            {
                girlSave.RelationshipLevel = maxSingleGirlRelationshipLevel;

                ModInterface.Log.Message("Enable single date bonus round");
                args.IsSexDate = true;
                args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.AttractToLovers;
                args.IsGameOver = Game.Session.Puzzle.puzzleStatus.bonusRound;
            }
            else
            {
                if (!oneUnderMaxLevel)
                {
                    girlSave.RelationshipLevel++;
                }

                ModInterface.Log.Message("Single date deny bonus round");
                if (args.LevelUpType == PuzzleRoundOverArgs.CutsceneType.AttractToLovers)
                {
                    args.LevelUpType = PuzzleRoundOverArgs.CutsceneType.None;
                }
                args.IsSexDate = false;
                args.IsGameOver = true;
            }
        }
        else if (Plugin.RequireLoversBeforeThreesome.Value)
        {
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
    }

    internal static void On_DateLocationSelected(DateLocationSelectedArgs args)
    {
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null)
        {
            return;
        }

        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;

        if (State.IsSingleDate)
        {
            // Deny single dates if not enough stamina
            var statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(true);
            if (statusGirl.stamina < 1)
            {
                args.DenyDate = true;
                // exhausted
                Game.Session.gameCanvas.GetDoll(true).ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(Hp2BaseMod.DialogTriggers.StaminaInsufficient), DialogLineFormat.PASSIVE);
                return;
            }

            args.LeftStaminaGain = 0;
            args.RightStaminaGain = 0;

            args.PlayerPoints = 5;
            args.LeftPoints = 0;
            args.RightPoints = 5;

            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);
            var girlSave = State.SaveFile.GetGirl(girlId);

            if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && girlSave.RelationshipLevel == maxSingleGirlRelationshipLevel - 1
                && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime
                && Plugin.TryGetSingleDateGirl(girlId, out var singleDateGirl))
            {
                //pick a photo with a loc
                if (singleDateGirl.SexPhotos.Any()
                    && singleDateGirl.SexPhotos.Select(x => x.LocationId).ToArray().GetRandom() is var randomLocId
                    && randomLocId != RelativeId.Default)
                {
                    args.Location = ModInterface.GameData.GetLocation(randomLocId);
                }
                //or is the photo doesn't have a loc pick randomly
                else
                {
                    args.Location = playerFileGirlPair.girlPairDefinition.sexLocationDefinition;

                    if (args.Location == null)
                    {
                        var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4);
                        Game.Data.Locations.GetAllByLocationType(LocationType.DATE).Where(x =>
                        {
                            var expansion = x.Expansion();
                            return expansion.IsValidForNormalDate(time)
                                && !singleDateGirl.SexLocBlackList.Contains(ModInterface.Data.GetDataId(GameDataType.Location, x.id));
                        }).ToArray().TryGetRandom(out args.Location);
                    }
                }

                return;
            }
            else
            {
                args.Location = null;
            }
        }
        else if (Plugin.RequireLoversBeforeThreesome.Value)
        {
            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            if (girlSaveOne?.RelationshipLevel == maxSingleGirlRelationshipLevel
                && girlSaveTwo?.RelationshipLevel == maxSingleGirlRelationshipLevel
                && playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime)
            {
                args.Location = playerFileGirlPair.girlPairDefinition.sexLocationDefinition;
                return;
            }
            else
            {
                args.Location = null;
            }
        }
    }

    internal static void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        args.UnlockedPhotos ??= new List<PhotoDefinition>();
        args.UnlockedPhotos.AddRange(State.SaveFile.Girls.Values.SelectManyNN(x => x.UnlockedPhotos).Select(ModInterface.GameData.GetPhoto));
    }

    internal static void On_PreDateDollsRefresh(PreDateDollResetArgs args)
    {
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);
        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel.Value;

        if (State.IsSingleDate)
        {
            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            // must be non default and at pre max level
            var pairStyle = Game.Session.Location.currentGirlPair.Expansion().PairStyle;
            if (pairStyle != null && pairStyle.SexGirlTwo.OutfitId != RelativeId.Default
                && girlSave?.RelationshipLevel == maxSingleGirlRelationshipLevel - 1)
            {
                args.Style = PreDateDollResetArgs.StyleType.Sex;
            }
        }
        else if (Plugin.RequireLoversBeforeThreesome.Value)
        {
            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            if ((girlSaveOne == null || girlSaveOne.RelationshipLevel == maxSingleGirlRelationshipLevel)
                && (girlSaveTwo == null || girlSaveTwo.RelationshipLevel == maxSingleGirlRelationshipLevel))
            {
                args.Style = PreDateDollResetArgs.StyleType.Sex;
            }
        }
    }

    internal static void On_TalkFavQuestionResponse(TalkFavQuestionResponseArgs args)
    {
        if (State.IsSingleDate)
        {
            args.OtherGirlResponds = false;
        }
    }

    internal static void On_PreLocationArrive(LocationArriveArgs args)
    {
        State.On_LocationManger_Arrive(args.girlPairDef);

        if (State.IsSingle(args.girlPairDef))
        {
            args.cellphoneOnLeft = true;
            args.meetingCutscene = UiPrefabs.SingleCutsceneMeeting;
        }
    }

    internal static void On_PreLocationSettled(LocationSettledArgs args)
    {
        if (State.IsSingleDate)
        {
            args.actionBubblesWindow = UiPrefabs.SingleDateBubbles;
        }
    }
}
