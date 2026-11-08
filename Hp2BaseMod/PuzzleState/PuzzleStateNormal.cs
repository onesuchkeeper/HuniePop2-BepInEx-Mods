// Hp2BaseMod 2026, By OneSuchKeeper

using UnityEngine;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

/// <summary>
/// A date between a girl pair at a date location, and the fallback when no other kind applies. This is also the base of the tutorial
/// date. Bosses (<see cref="PuzzleStateBoss"/>) and non-stop (<see cref="PuzzleStateNonstop"/>) have no pair based setup and do not derive from this.
/// </summary>
public class PuzzleStateNormal(IGameStateLocation owner) : PuzzleStateBase(owner)
{
    public class Factory : IPuzzleStateFactory
    {
        public IFullPuzzleState Create(IGameStateLocation gameState) => new PuzzleStateNormal(gameState);
    }

    public override void OnLocationArrived(LocationArriveArgs args)
    {
        // The girl pair is never null here: puzzles without one are non-stop and use PuzzleStateNonstop.

        // NOTE: this first Reset ran unconditionally in the old LocationArriving
        // handler, while the Reset just below only fires when isEmpty/initialArrive.
        // Kept both since they're not provably equivalent (the isEmpty check below
        // can be false here while this one still needs to run, e.g. moving between
        // locations mid-date) - but if you know puzzleStatus well enough to say the
        // first one is always superseded, it can go.
        Owner.ResolveGirlDefinitions(args.girlPairDef, args.sidesFlipped, out var left, out var right);
        Game.Session.Puzzle.puzzleStatus.GetExpansion().Reset(left, right);

        var locationManager = Game.Session.Location;

        if (args.initialArrive || Game.Session.Puzzle.puzzleStatus.isEmpty)
        {
            Game.Session.Puzzle.puzzleStatus.GetExpansion().Reset(locationManager.currentGirlLeft, locationManager.currentGirlRight);
        }

        Game.Session.Puzzle.puzzleStatus.PopulateAilments();
    }

    // ---- rules of the status ----

    public override bool ChecksStaminaFreezeOnStart => true;

    // After the first round every round of a date is a bonus round.
    public override bool IsBonusRound(int roundIndex, int girlsRemaining) => roundIndex > 0;

    public override PuzzleStatusType ConditionDateType => PuzzleStatusType.NORMAL;

    public override void GetRoundGoal(int roundIndex, bool bonusRound, out int affectionGoal, out float bonusDrainDelay)
    {
        var playerFile = Game.Persistence.playerFile;

        if (!bonusRound)
        {
            bonusDrainDelay = -1f;

            // the goal climbs with every relationship level up, and with every post game date
            float goal = 200f;
            float step = 25f;
            int levelUps = Mathf.Min(playerFile.relationshipUpCount, 47);
            if (playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_COMPLETE && playerFile.GetFlagValue(Flags.ALPHA_MODE) <= 0)
            {
                levelUps += playerFile.alphaDateCount + 1;
            }

            for (int i = 0; i < levelUps; i++)
            {
                goal += step;
                step += 3.3525f;
            }

            affectionGoal = Mathf.RoundToInt(goal / 5f) * 5;
            return;
        }

        // the bonus round gets harder and faster with every completed pair
        affectionGoal = 400;
        bonusDrainDelay = 0.1f;
        for (int i = 0; i < playerFile.completedGirlPairs.Count; i++)
        {
            affectionGoal += 25;
            bonusDrainDelay -= 0.002f;
        }
    }

    // After the post game begins, dates are numbered instead of timed.
    public override bool TryGetHeaderLabel(out PuzzleHeaderLabel label)
    {
        var playerFile = Game.Persistence.playerFile;
        if (playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_COMPLETE && playerFile.GetFlagValue(Flags.ALPHA_MODE) <= 0)
        {
            label = new PuzzleHeaderLabel(PuzzleHeaderIcon.Alpha, "DATE #" + (Game.Session.Puzzle.puzzleStatus.alphaDateIndex + 1));
            return true;
        }

        label = default;
        return false;
    }

    // ---- puzzle lifecycle ----

    // A date between lovers that the player has already completed, before the story's end.
    public override bool IsRepeatDate
    {
        get
        {
            var pair = Game.Session.Location.currentGirlPair;
            if (pair == null) return false;

            var playerFile = Game.Persistence.playerFile;

            var pairFile = playerFile.GetPlayerFileGirlPair(pair);
            return pairFile != null
                && pairFile.relationshipType == GirlPairRelationshipType.LOVERS
                && playerFile.storyProgress < PlayerFile.STORY_PROGRESS_COMPLETE;
        }
    }

    public override void OnRoundOver(PuzzleRoundOverContext context)
    {
        var pair = Game.Session.Location.currentGirlPair;
        var pairFile = pair == null ? null : Game.Persistence.playerFile.GetPlayerFileGirlPair(pair);

        // read before the rules below level the relationship up
        var args = BuildRoundOverArgs(pair, pairFile);

        ApplyRoundOverRules(context, pair, pairFile);

        // let mods have their say before the cutscenes are chosen
        args.IsGameOver = context.GameOver;
        args.IsSuccess = context.RoundSucceeded;
        AdjustRoundOver(args);
        ModInterface.Events.NotifyPuzzleRoundOver(args);
        ModInterface.Log.Message(args.ToString());
        context.GameOver = args.IsGameOver;
        context.RoundSucceeded = args.IsSuccess;

        if (pair != null)
        {
            ChooseCutscenes(context, args, pair.GetExpansion());
        }
    }

    protected virtual void AdjustRoundOver(PuzzleRoundOverArgs args) { }

    private static PuzzleRoundOverArgs BuildRoundOverArgs(GirlPairDefinition pair, PlayerFileGirlPair pairFile)
    {
        var args = new PuzzleRoundOverArgs();
        if (pairFile == null) return args;

        args.IsSexDate = pairFile.relationshipType == GirlPairRelationshipType.ATTRACTED
            && Game.Session.Location.currentLocation == pair.sexLocationDefinition;

        args.LevelUpType = pairFile.relationshipType switch
        {
            GirlPairRelationshipType.COMPATIBLE => PuzzleRoundOverArgs.CutsceneType.CompatToAttract,
            GirlPairRelationshipType.ATTRACTED => PuzzleRoundOverArgs.CutsceneType.AttractToLovers,
            _ => PuzzleRoundOverArgs.CutsceneType.None
        };

        return args;
    }

    /// <summary>
    /// What ending a round of a date does to the save: relationship level ups, whether the date goes on into a bonus round, and
    /// the achievements. Sets the base game's default cutscenes, which <see cref="ChooseCutscenes"/> can then replace.
    /// </summary>
    private void ApplyRoundOverRules(PuzzleRoundOverContext context, GirlPairDefinition pair, PlayerFileGirlPair pairFile)
    {
        var puzzle = Game.Session.Puzzle;
        var status = puzzle.puzzleStatus;
        var playerFile = Game.Persistence.playerFile;

        if (!context.RoundSucceeded) return;

        if (pairFile == null)
        {
            context.RoundOverCutscene = puzzle.cutsceneSuccess;
            return;
        }

        if (pairFile.relationshipType == GirlPairRelationshipType.COMPATIBLE)
        {
            context.RoundOverCutscene = puzzle.cutsceneSuccessCompatible;
            LevelUpPair(pairFile);
        }
        else if (pairFile.relationshipType == GirlPairRelationshipType.ATTRACTED
            && Game.Session.Location.currentLocation == pair.sexLocationDefinition)
        {
            if (!status.bonusRound)
            {
                // the sex date goes on into its bonus round
                context.RoundOverCutscene = puzzle.cutsceneSuccessAttracted;
                context.NewRoundCutscene = puzzle.cutsceneNewroundBonus;
                context.GameOver = false;
            }
            else
            {
                context.RoundOverCutscene = puzzle.cutsceneSuccessBonus;
                LevelUpPair(pairFile);

                if (playerFile.completedGirlPairs.Count > 0)
                {
                    Game.Manager.Platform.UnlockAchievement("unchaste", true);
                    if (playerFile.daytimeElapsed < 12)
                    {
                        Game.Manager.Platform.UnlockAchievement("unchaste_with_haste", true);
                    }
                }

                if (playerFile.completedGirlPairs.Count == 24)
                {
                    Game.Manager.Platform.UnlockAchievement("debauchery", true);
                    if (playerFile.daytimeElapsed < 68)
                    {
                        Game.Manager.Platform.UnlockAchievement("debauchery_without_delay", true);
                    }
                }
            }
        }
        else
        {
            context.RoundOverCutscene = puzzle.cutsceneSuccess;
        }

        if (!status.bonusRound)
        {
            if (status.movesRemaining >= 20)
            {
                Game.Manager.Platform.UnlockAchievement("quickie", true);
            }

            var left = status.girlStatusLeft;
            var right = status.girlStatusRight;
            if (left.passion == 100 && left.sentiment == 40 && left.stamina == 6
                && right.passion == 100 && right.sentiment == 40 && right.stamina == 6)
            {
                Game.Manager.Platform.UnlockAchievement("fill_er_up", true);
            }

            if (left.exhaustedCount + right.exhaustedCount >= 8)
            {
                Game.Manager.Platform.UnlockAchievement("like_it_rough", true);
            }
        }

        // post game dates are counted
        if (context.GameOver && playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_COMPLETE && playerFile.GetFlagValue(Flags.ALPHA_MODE) <= 0)
        {
            playerFile.alphaDateCount++;
            if (playerFile.alphaDateCount >= 8)
            {
                Game.Manager.Platform.UnlockAchievement("alpha_apprentice", true);
                if (playerFile.alphaDateCount >= 24)
                {
                    Game.Manager.Platform.UnlockAchievement("alpha_adept", true);
                }
            }
        }
    }

    protected virtual void LevelUpPair(PlayerFileGirlPair pairFile) => pairFile.RelationshipLevelUp();

    /// <summary>
    /// Replaces the base game's cutscenes with the pair's own, where it has them.
    /// </summary>
    private static void ChooseCutscenes(PuzzleRoundOverContext context, PuzzleRoundOverArgs args, ExpandedGirlPairDefinition pairExpansion)
    {
        var puzzle = Game.Session.Puzzle;

        if (!args.IsSuccess)
        {
            context.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalFailureId)
                ?? puzzle.cutsceneFailure;
            return;
        }

        if (args.IsSexDate)
        {
            if (puzzle.puzzleStatus.bonusRound)
            {
                context.NewRoundCutscene = null;
                context.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalBonusSuccessId)
                    ?? puzzle.cutsceneSuccessBonus;
            }
            else
            {
                context.RoundOverCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalAttractedSuccessId)
                    ?? puzzle.cutsceneSuccessAttracted;
                context.NewRoundCutscene = ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalBonusNewRoundId)
                    ?? puzzle.cutsceneNewroundBonus;
            }

            return;
        }

        // A date that is not the sex date only levels up a compatible pair. An attracted pair that did not come to its sex location
        // gets the plain success, like in the base game.
        context.RoundOverCutscene = args.LevelUpType == PuzzleRoundOverArgs.CutsceneType.CompatToAttract
            ? ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalCompatibleSuccessId) ?? puzzle.cutsceneSuccessCompatible
            : ModInterface.GameData.GetCutscene(pairExpansion.CutsceneNormalSuccessId) ?? puzzle.cutsceneSuccess;
    }

    // A finished date opens the cellphone's finder, from which the player picks where to go next (see GameStatePuzzle).
    public override void OnPuzzleEnded(bool succeeded)
    {
        var cellphone = Game.Session.gameCanvas.cellphone;
        cellphone.openAppIndex = 1;
        cellphone.openAppLocked = true;
        cellphone.FreezeAppButtons(true, new int[] { 0, 1, 2, 3, 4, 5 });
        cellphone.Open();
    }

    // A normal date unlocks the styles the girls wore for it.
    public override bool UnlocksStylesAfterRewards => true;

    // A failed date still pays out, by how far the affection got.
    public override int GetPostRewardFruitCount(int baseFruitCount, out bool failed)
    {
        var status = Game.Session.Puzzle.puzzleStatus;

        failed = Game.Session.Puzzle.puzzleGrid.roundState == PuzzleRoundState.FAILURE;
        if (!failed) return baseFruitCount;

        var fruitCount = 2;
        for (var i = 0; i < 6; i++)
        {
            if (status.affection / (float)status.affectionGoal >= 0.15f * (i + 1))
            {
                fruitCount += 2;
            }
        }

        return fruitCount;
    }


    public override bool TryResolveDollStyles(ResolveDollStylesArgs args)
    {
        if (args.GirlPair == null) return true;

        var playerPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(args.GirlPair);
        if (playerPair == null) return true;

        Owner.ResolveGirlDefinitions(args.GirlPair, args.SidesFlipped, out var leftDef, out var rightDef);

        (GirlStyleInfo left, GirlStyleInfo right, bool isCutsceneStyle) result;

        if (playerPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
        {
            result = Owner.ResolveMeetingStyles(args.GirlPair, args.SidesFlipped);
        }
        else
        {
            var preDateArgs = BuildPreDateArgs(playerPair, args.Location);
            AdjustDollStyle(preDateArgs);
            ModInterface.Events.NotifyPreDateDollReset(preDateArgs);

            result = preDateArgs.Style switch
            {
                PreDateDollResetArgs.StyleType.Sex =>
                    ResolveSexStyles(args.GirlPair, args.Location, leftDef, rightDef, args.SidesFlipped),

                PreDateDollResetArgs.StyleType.Location =>
                    ResolveLocationStyles(args.Location, leftDef, rightDef),

                _ => ResolveFileStyles()
            };
        }

        var leftStyle = Owner.ApplyStyleOverride(leftDef, args.Location, result.left, result.isCutsceneStyle);
        var rightStyle = Owner.ApplyStyleOverride(rightDef, args.Location, result.right, result.isCutsceneStyle);

        Owner.ApplyStyleToDoll(leftStyle, Game.Session.gameCanvas.dollLeft, leftDef);
        Owner.ApplyStyleToDoll(rightStyle, Game.Session.gameCanvas.dollRight, rightDef);

        return true;
    }

    protected virtual void AdjustDollStyle(PreDateDollResetArgs args) { }

    /// <summary>
    /// Determines which style bucket a date should use. Mirrors the original
    /// branching: sex styles on the scheduled sex daytime, else location styles
    /// mid-puzzle (off the boss location), else the player's saved file style.
    /// </summary>
    private PreDateDollResetArgs BuildPreDateArgs(PlayerFileGirlPair playerPair, LocationDefinition currentLocation)
    {
        var args = new PreDateDollResetArgs();

        if (playerPair.relationshipType == GirlPairRelationshipType.ATTRACTED &&
            Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerPair.girlPairDefinition.sexDaytime)
        {
            args.Style = PreDateDollResetArgs.StyleType.Sex;
        }
        else if (!Game.Session.Puzzle.puzzleStatus.isEmpty &&
                 currentLocation != Game.Session.Puzzle.bossLocationDefinition)
        {
            args.Style = PreDateDollResetArgs.StyleType.Location;
        }
        else
        {
            args.Style = PreDateDollResetArgs.StyleType.File;
        }

        return args;
    }

    private (GirlStyleInfo, GirlStyleInfo, bool) ResolveSexStyles(
        GirlPairDefinition pair,
        LocationDefinition location,
        GirlDefinition leftDef,
        GirlDefinition rightDef,
        bool sidesFlipped)
    {
        var pairStyle = pair.GetExpansion().PairStyle;
        if (pairStyle == null) return (null, null, false);

        var left = sidesFlipped ? pairStyle.SexGirlTwo : pairStyle.SexGirlOne;
        var right = sidesFlipped ? pairStyle.SexGirlOne : pairStyle.SexGirlTwo;

        var (locLeft, locRight, _) = ResolveLocationStyles(location, leftDef, rightDef);

        if (left != null && left.OutfitId == RelativeId.Default && left.HairstyleId == RelativeId.Default)
        {
            left = locLeft;
        }

        if (right != null && right.OutfitId == RelativeId.Default && right.HairstyleId == RelativeId.Default)
        {
            right = locRight;
        }

        return (left, right, false);
    }

    private (GirlStyleInfo, GirlStyleInfo, bool) ResolveLocationStyles(
        LocationDefinition location, GirlDefinition leftDef, GirlDefinition rightDef)
    {
        var locationId = ModInterface.Data.GetDataId(GameDataType.Location, location.id);
        var puzzle = Game.Session.Puzzle.puzzleStatus;

        var left = ResolveSingleLocationStyle(puzzle.girlStatusLeft.playerFileGirl, leftDef, locationId, "left");
        var right = ResolveSingleLocationStyle(puzzle.girlStatusRight.playerFileGirl, rightDef, locationId, "right");

        return (left, right, false);
    }

    private GirlStyleInfo ResolveSingleLocationStyle(
        PlayerFileGirl playerFile, GirlDefinition girlDef, RelativeId locationId, string sideLabel)
    {
        if (!playerFile.stylesOnDates)
        {
            var expansion = girlDef.GetExpansion();

            if (expansion.GetCurrentBody().LocationIdToOutfitId.TryGetValue(locationId, out var style))
            {
                ModInterface.Log.Message($"Using location style for {sideLabel} girl: {style}");
                return style;
            }

            ModInterface.Log.Message($"No location style found for {sideLabel} girl");
            return null;
        }

        var fileStyle = Owner.BuildStyleFromFile(playerFile);
        ModInterface.Log.Message($"Using file style for {sideLabel} girl: {fileStyle}");
        return fileStyle;
    }

    private (GirlStyleInfo, GirlStyleInfo, bool) ResolveFileStyles()
    {
        var leftFile = Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl;
        var rightFile = Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl;

        return (Owner.BuildStyleFromFile(leftFile), Owner.BuildStyleFromFile(rightFile), false);
    }
}