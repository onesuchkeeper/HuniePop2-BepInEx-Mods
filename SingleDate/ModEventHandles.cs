using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Extension.IEnumerableExtension;
using UnityEngine;

namespace SingleDate;

internal static class ModEventHandles
{
    private static FieldInfo f_roundOverCutscene = AccessTools.Field(typeof(PuzzleManager), "_roundOverCutscene");
    private static FieldInfo f_newRoundCutscene = AccessTools.Field(typeof(PuzzleManager), "_newRoundCutscene");
    private static readonly FieldInfo f_altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");
    private static FieldInfo f_gameOver = AccessTools.Field(typeof(PuzzleStatus), "_gameOver");

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
        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel;

        args.SexPool?.RemoveAll(x => State.IsSingle(x.girlPairDefinition)
            && State.SaveFile.GetGirl(x.girlPairDefinition.girlDefinitionTwo.id)?.RelationshipLevel != maxSingleGirlRelationshipLevel - 1);

        foreach (var id in args.SexPool)
        {
            ModInterface.Log.LogInfo(id.ToString());
        }

        if (Plugin.RequireLoversBeforeThreesome)
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

        //step 1, set base game cutscenes properly
        var newRoundCutscene = f_newRoundCutscene.GetValue<CutsceneDefinition>(Game.Session.Puzzle);
        var roundOverCutscene = f_roundOverCutscene.GetValue<CutsceneDefinition>(Game.Session.Puzzle);
        var gameOver = Game.Session.Puzzle.puzzleStatus.gameOver;

        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);
        var girlSave = State.SaveFile.GetGirl(girlId);

        //the game doesn't have a way to actually check this properly, may need to handle in base mod state
        var preBonusRound = f_newRoundCutscene.GetValue<CutsceneDefinition>(Game.Session.Puzzle) == Game.Session.Puzzle.cutsceneNewroundBonus;

        if (State.IsSingleDate)
        {
            var singleDateGirl = Plugin.GetSingleDateGirl(girlId);

            //correct cutscenes
            //bonus round without required level
            if (preBonusRound
                && girlSave?.RelationshipLevel < maxSingleGirlRelationshipLevel - 1)
            {
                ModInterface.Log.LogInfo("Single date deny bonus round");
                newRoundCutscene = null;
                roundOverCutscene = ModInterface.GameData.GetCutscene(CutsceneIds.Success);
                gameOver = true;
            }
            //at required level at sex loc and time
            else if ((Game.Session.Location.currentGirlPair.sexLocationDefinition == null
                        || Game.Session.Location.currentGirlPair.sexLocationDefinition == Game.Session.Location.currentLocation)
                    && girlSave?.RelationshipLevel >= maxSingleGirlRelationshipLevel - 1
                    && Game.Session.Location.currentGirlPair.sexDaytime == (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4))
            {
                //after bonus round
                if (Game.Session.Puzzle.puzzleStatus.bonusRound)
                {
                    gameOver = true;
                    newRoundCutscene = null;
                    roundOverCutscene = Game.Session.Puzzle.cutsceneSuccessBonus;
                }
                //before bonus round
                else
                {
                    gameOver = false;
                    newRoundCutscene = Game.Session.Puzzle.cutsceneNewroundBonus;
                    roundOverCutscene = Game.Session.Puzzle.cutsceneSuccessAttracted;
                }
            }

            //TODO there's an interop issue with repeat threesome here. I have no way of making sure that is comes
            //after the repeat threesome cutscene corrections. And I don't want it to happen whenever
            //cuz that's not how HP1 works
            //I could add an interop to expose some kind of "set cutscenes" delegate
            //Or in the base mod I'd need some way to coordinate them. Like flags that separately set
            //if the threesome should occur and what cutscenes to use for a threesome.
            //hmmm

            //replace default cutscenes with customs if they exist.
            ModInterface.Log.LogInfo($"Pre cutscene replace- Game Over: {gameOver}, Round Over Cutscene: {roundOverCutscene?.name ?? "null"}, New Round Cutscene: {newRoundCutscene?.name ?? "null"}");
            if (gameOver && roundOverCutscene == Game.Session.Puzzle.cutsceneSuccess)
            {
                roundOverCutscene = ModInterface.GameData.GetCutscene(CutsceneIds.Success);
            }

            if (!gameOver
                && (roundOverCutscene == Game.Session.Puzzle.cutsceneSuccessAttracted
                    || roundOverCutscene == Game.Session.Puzzle.cutsceneSuccess)
                && singleDateGirl.CutsceneSuccessAttracted != RelativeId.Default)
            {
                roundOverCutscene = ModInterface.GameData.GetCutscene(singleDateGirl.CutsceneSuccessAttracted);
            }

            if (roundOverCutscene == Game.Session.Puzzle.cutsceneSuccessBonus
                && singleDateGirl.CutsceneSuccessBonus != RelativeId.Default)
            {
                roundOverCutscene = ModInterface.GameData.GetCutscene(singleDateGirl.CutsceneSuccessBonus);
            }

            //set cutscenes
            f_gameOver.SetValue(Game.Session.Puzzle.puzzleStatus, gameOver);
            f_newRoundCutscene.SetValue(Game.Session.Puzzle, newRoundCutscene);
            f_roundOverCutscene.SetValue(Game.Session.Puzzle, roundOverCutscene);
        }
        else if (preBonusRound && Plugin.RequireLoversBeforeThreesome)
        {
            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            if ((girlSaveOne != null && girlSaveOne.RelationshipLevel < maxSingleGirlRelationshipLevel)
                || (girlSaveTwo != null && girlSaveTwo.RelationshipLevel < maxSingleGirlRelationshipLevel))
            {
                ModInterface.Log.LogInfo("Deny double date bonus due to single date levels");
                f_gameOver.SetValue(Game.Session.Puzzle.puzzleStatus, false);
                f_newRoundCutscene.SetValue(Game.Session.Puzzle, null);
                f_roundOverCutscene.SetValue(Game.Session.Puzzle, Game.Session.Puzzle.cutsceneSuccess);
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

        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel;

        if (State.IsSingleDate)
        {
            // Deny single dates if not enough stamina
            var statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(true);
            if (statusGirl.stamina < 1)
            {
                args.DenyDate = true;
                // exhausted
                Game.Session.gameCanvas.GetDoll(true).ReadDialogTrigger(ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 35)), DialogLineFormat.PASSIVE);
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
        }
        else
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
        }

        //if not a sex date, set it to null to force it to re-roll
        args.Location = null;
    }

    internal static void On_SinglePhotoDisplayed(SinglePhotoDisplayArgs args)
    {
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null
            || !State.IsSingle(playerFileGirlPair.girlPairDefinition))
        {
            return;
        }

        var girlId = ModInterface.Data.GetDataId(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo);
        var girlSave = State.SaveFile.GetGirl(girlId);
        var singleDateGirl = Plugin.GetSingleDateGirl(girlId);

        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            if (!singleDateGirl.SexPhotos.Any())
            {
                args.BigPhotoId = PhotoDefault.Id;
                return;
            }

            var locId = ModInterface.Data.GetDataId(GameDataType.Location, Game.Session.Location.currentLocation.id);

            var sexPhotos = singleDateGirl.SexPhotos.Where(x => x.LocationId == locId).Select(x => x.PhotoId).ToArray();

            if (sexPhotos.Length == 0)
            {
                sexPhotos = singleDateGirl.SexPhotos.Select(x => x.PhotoId).ToArray();
            }

            if (sexPhotos.Length == 0)
            {
                sexPhotos = [PhotoDefault.Id];
            }

            var photoPool = sexPhotos.Except(girlSave.UnlockedPhotos).ToArray();
            args.BigPhotoId = photoPool.Length == 0
                ? sexPhotos.GetRandom()
                : photoPool.GetRandom();
        }
        else
        {
            var datePercentage = girlSave.RelationshipLevel / (float)Plugin.MaxSingleGirlRelationshipLevel;

            if (!singleDateGirl.DatePhotos.Any())
            {
                args.BigPhotoId = PhotoDefault.Id;
                return;
            }

            var validDatePhotos = singleDateGirl.DatePhotos.Where(x => x.RelationshipPercentage <= datePercentage).Select(x => x.PhotoId).ToArray();

            if (validDatePhotos.Length == 0)
            {
                validDatePhotos = [PhotoDefault.Id];
            }

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
        var maxSingleGirlRelationshipLevel = Plugin.MaxSingleGirlRelationshipLevel;

        if (State.IsSingleDate)
        {
            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            //must be non default and at pre max level
            if (Game.Session.Location.currentGirlPair.Expansion().PairStyle.SexGirlTwo.OutfitId != RelativeId.Default
                && girlSave?.RelationshipLevel == maxSingleGirlRelationshipLevel - 1)
            {
                args.Style = PreDateDollResetArgs.StyleType.Sex;
            }
        }
        else if (Plugin.RequireLoversBeforeThreesome)
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
}
