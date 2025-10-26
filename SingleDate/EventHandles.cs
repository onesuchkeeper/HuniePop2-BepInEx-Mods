using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace SingleDate;

internal static class EventHandles
{
    private static FieldInfo f_roundOverCutscene = AccessTools.Field(typeof(PuzzleManager), "_roundOverCutscene");
    private static FieldInfo f_newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");
    private static readonly FieldInfo f_altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");

    private static bool _singleDropZone = false;

    internal static void On_LocationArriveSequence(LocationArriveSequenceArgs sequence)
    {
        var rightOuter = Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.OUTER);
        var rightInnerPos = Game.Session.gameCanvas.dollRight.GetPositionByType(DollPositionType.INNER);
        var diff = rightInnerPos - rightOuter;

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
                Game.Session.gameCanvas.dollRight.dropZone.transform.localPosition -= new Vector3(diff.x, diff.y, 0);
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
        f_altGirlFocused.SetValue(Game.Session.Puzzle.puzzleStatus, true);
    }

    internal static void On_FinderSlotsPopulate(FinderSlotPopulateEventArgs args)
    {
        var maxSingleGirlRelationshipLevel = Plugin.Instance.MaxSingleGirlRelationshipLevel;

        args.SexPool?.RemoveAll(x => State.IsSingle(x.girlPairDefinition)
            && State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != maxSingleGirlRelationshipLevel - 1);

        foreach (var id in args.SexPool)
        {
            ModInterface.Log.LogInfo(id.ToString());
        }

        if (Plugin.Instance.RequireLoversBeforeThreesome)
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

        if (f_newRoundCutscene.GetValue<CutsceneDefinition>(Game.Session.Puzzle) == Game.Session.Puzzle.cutsceneNewroundBonus)
        {
            var maxSingleGirlRelationshipLevel = Plugin.Instance.MaxSingleGirlRelationshipLevel;

            var validSingleLevels = false;
            if (State.IsSingleDate)
            {
                var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

                //single date relationship levels are handled post round over, so girl will be at max already for bonus round
                validSingleLevels = girlSave?.RelationshipLevel == maxSingleGirlRelationshipLevel;
            }
            else if (Plugin.Instance.RequireLoversBeforeThreesome)
            {
                var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
                var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

                validSingleLevels = (girlSaveOne == null || girlSaveOne.RelationshipLevel == maxSingleGirlRelationshipLevel)
                    && (girlSaveTwo == null || girlSaveTwo.RelationshipLevel == maxSingleGirlRelationshipLevel);
            }

            if (!validSingleLevels)
            {
                ModInterface.Log.LogInfo("Disabling bonus round per single date requirements");
                Game.Session.Puzzle.puzzleStatus.gameOver = true;

                f_newRoundCutscene.SetValue(Game.Session.Puzzle, null);
                f_roundOverCutscene.SetValue(Game.Session.Puzzle, State.IsSingleDate
                    ? ModInterface.GameData.GetCutscene(CutsceneIds.Success)
                    : Game.Session.Puzzle.cutsceneSuccess);
            }
        }
        else if (State.IsSingleDate && f_roundOverCutscene.GetValue<CutsceneDefinition>(Game.Session.Puzzle) == Game.Session.Puzzle.cutsceneSuccess)
        {
            f_newRoundCutscene.SetValue(Game.Session.Puzzle, null);
            f_roundOverCutscene.SetValue(Game.Session.Puzzle, ModInterface.GameData.GetCutscene(CutsceneIds.Success));
        }
    }

    internal static void On_DateLocationSelected(DateLocationSelectedArgs args)
    {
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null)
        {
            return;
        }

        var maxSingleGirlRelationshipLevel = Plugin.Instance.MaxSingleGirlRelationshipLevel;
        bool sexDate;

        if (State.IsSingleDate)
        {
            // Deny single dates if not enough stamina
            var statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(true);
            if (statusGirl.stamina < 1)
            {
                args.DenyDate = true;
                // TODO play too hungry audio
                return;
            }

            args.LeftStaminaGain = 0;
            args.RightStaminaGain = 0;

            args.PlayerPoints = 5;
            args.LeftPoints = 0;
            args.RightPoints = 5;

            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            sexDate = playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && girlSave.RelationshipLevel == maxSingleGirlRelationshipLevel - 1
                && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime;
        }
        else
        {
            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            sexDate = girlSaveOne?.RelationshipLevel == maxSingleGirlRelationshipLevel
                && girlSaveTwo?.RelationshipLevel == maxSingleGirlRelationshipLevel
                && playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime;
        }

        args.Location = sexDate ? playerFileGirlPair.girlPairDefinition.sexLocationDefinition : null;
    }

    internal static void On_SinglePhotoDisplayed(SinglePhotoDisplayArgs args)
    {
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null
            || !State.IsSingle(playerFileGirlPair.girlPairDefinition))
        {
            return;
        }

        ModInterface.Log.LogInfo("Single Date Photo");//TODO actually unlock the photos in normal file?

        var girlId = ModInterface.Data.GetDataId(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo);
        var girlSave = State.SaveFile.GetGirl(girlId);

        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            if (!(Plugin.Instance.GirlIdToSexPhotoId.TryGetValue(girlId, out var sexPhotos) && sexPhotos.Any()))
            {
                args.BigPhotoId = PhotoDefault.Id;
                return;
            }

            var photoPool = sexPhotos.Except(girlSave.UnlockedPhotos).ToArray();
            args.BigPhotoId = photoPool.Length == 0
                ? sexPhotos.GetRandom()
                : photoPool.GetRandom();
        }
        else
        {
            var datePercentage = girlSave.RelationshipLevel / (float)Plugin.Instance.MaxSingleGirlRelationshipLevel;

            if (!(Plugin.Instance.GirlIdToDatePhotoId.TryGetValue(girlId, out var datePhotos) && datePhotos.Any()))
            {
                args.BigPhotoId = PhotoDefault.Id;
                return;
            }

            var validDatePhotos = datePhotos.Where(x => x.Item2 <= datePercentage).Select(x => x.Item1).ToArray();
            var lockedDatePhotos = girlSave.UnlockedPhotos == null
                ? validDatePhotos
                : validDatePhotos.Except(girlSave.UnlockedPhotos).ToArray();

            args.BigPhotoId = lockedDatePhotos.Length == 0
                ? validDatePhotos.GetRandom()
                : lockedDatePhotos.GetRandom();
        }

        girlSave.UnlockedPhotos ??= new();
        girlSave.UnlockedPhotos.Add(args.BigPhotoId);
    }

    internal static void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        args.UnlockedPhotos ??= new List<PhotoDefinition>();
        args.UnlockedPhotos.AddRange(State.SaveFile.Girls.Values.SelectManyNN(x => x.UnlockedPhotos).Select(ModInterface.GameData.GetPhoto));
    }

    internal static void On_PreDateDollsRefresh(PreDateDollResetArgs args)
    {
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);
        var maxSingleGirlRelationshipLevel = Plugin.Instance.MaxSingleGirlRelationshipLevel;

        if (State.IsSingleDate)
        {
            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);
            args.UseSexStyles = girlSave?.RelationshipLevel == maxSingleGirlRelationshipLevel - 1;
        }
        else if (Plugin.Instance.RequireLoversBeforeThreesome)
        {
            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            args.UseSexStyles = (girlSaveOne == null || girlSaveOne.RelationshipLevel == maxSingleGirlRelationshipLevel)
                && (girlSaveTwo == null || girlSaveTwo.RelationshipLevel == maxSingleGirlRelationshipLevel);
        }
    }
}
