// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

/// <summary>
/// The boss date (the boss pair, Moxie and Jewn in the base game). The puzzle is a series of rounds played by pairs of regular girls,
/// ending with the boss pair.
/// </summary>
public sealed class PuzzleStateBoss(IGameStateLocation owner) : PuzzleStateBase(owner)
{
    public class Factory : IPuzzleStateFactory
    {
        public IFullPuzzleState Create(IGameStateLocation gameState) => new PuzzleStateBoss(gameState);
    }

    // the base game's cap on regular girls (4 rounds of 2) before the boss round
    private const int MaxRegularGirls = 8;

    public override void OnLocationArrived(LocationArriveArgs args)
    {
        var bossPair = args.girlPairDef;
        var girls = PuzzleGirlPool.GetMetRegularGirls();
        ListUtils.ShuffleList(girls);

        // Rounds take girls two at a time and the boss pair must be the final round, so the regular girls have to come in pairs.
        var regularCount = Math.Min(girls.Count, MaxRegularGirls);
        regularCount -= regularCount % 2;
        girls.RemoveRange(regularCount, girls.Count - regularCount);

        // Reset only builds a BOSS status for more than 2 girls, so there must be at least 2 regular girls besides the boss pair.
        PuzzleGirlPool.PadToAtLeast(girls, 2, "Boss date");

        girls.Add(bossPair.girlDefinitionOne);
        girls.Add(bossPair.girlDefinitionTwo);

        Game.Session.Puzzle.puzzleStatus.GetExpansion().Reset(girls);
        Game.Session.Puzzle.puzzleStatus.PopulateAilments();

        ModInterface.Log.Message($"Boss date: {girls.Count - 2} regular girls + the boss pair");
    }

    public override void OnArrivalCompleted(ArrivalCompletedArgs args)
    {
        // The boss intro cutscenes are normally queued by the location's arrive bundles. The generic date intro is the base game's fallback.
        if (args.ArrivalCutscene != null)
        {
            ModInterface.Log.Message("Boss date: arrival cutscene was queued by a bundle");
            return;
        }

        ModInterface.Log.Message("Boss date: no arrival cutscene was queued, using the generic date intro");
        args.ArrivalCutscene = Game.Session.Puzzle.cutsceneIntro;
    }

    public override void OnLocationSettled(LocationSettledArgs args)
    {
        ModInterface.Log.Message("GameState.Puzzle (boss): starting puzzle");
        Game.Session.Puzzle.StartPuzzle();
    }

    // The dolls of a boss date hold the status girls with the boss pair as their soul girls. The generic pair based styling would apply
    // the boss pair's styles to the wrong girls, so keep what the base game loaded.
    public override bool TryResolveDollStyles(ResolveDollStylesArgs args) => true;

    // ---- rules of the status ----

    // Three times the moves of a date: the pairs play one after the other on a single supply.
    private static readonly int[] _initialMoves = [120, 100, 80];
    private static readonly int[] _maxMoves = [140, 120, 100];

    public override int GetInitialMoves(SettingDifficulty difficulty) => _initialMoves[(int)difficulty];

    public override int GetMaxMoves(SettingDifficulty difficulty) => _maxMoves[(int)difficulty];

    public override bool PopulatesRandomAilments => true;

    // The last pair, the boss pair, plays the bonus round.
    public override bool IsBonusRound(int roundIndex, int girlsRemaining) => girlsRemaining <= 2;

    public override PuzzleStatusType ConditionDateType => PuzzleStatusType.BOSS;

    public override void GetRoundGoal(int roundIndex, bool bonusRound, out int affectionGoal, out float bonusDrainDelay)
    {
        affectionGoal = bonusRound ? 1000 : 5000;
        bonusDrainDelay = bonusRound ? 0.04f : -1f;
    }

    public override bool TryGetHeaderLabel(out PuzzleHeaderLabel label)
    {
        var status = Game.Session.Puzzle.puzzleStatus;
        label = new PuzzleHeaderLabel(
            PuzzleHeaderIcon.Daytime,
            status.bonusRound ? "BONUS" : $"ROUND {status.roundIndex + 1}/4");
        return true;
    }

    // ---- puzzle lifecycle ----

    public override void OnRoundOver(PuzzleRoundOverContext context)
    {
        var playerFile = Game.Persistence.playerFile;
        var status = Game.Session.Puzzle.puzzleStatus;

        if (context.RoundSucceeded)
        {
            if (status.bonusRound)
            {
                // the boss pair's own round: beating it ends the puzzle and saves the story
                context.RoundOverCutscene = Game.Session.Puzzle.cutsceneSuccessBonus;
                if (playerFile.storyProgress <= PlayerFile.STORY_PROGRESS_NYMPHOJINN_FUCKED)
                {
                    playerFile.storyProgress = PlayerFile.STORY_PROGRESS_POOLSIDE_ENDING;
                }

                if (playerFile.storyProgress >= PlayerFile.STORY_PROGRESS_POOLSIDE_ENDING)
                {
                    Game.Manager.Platform.UnlockAchievement("savior", true);
                    if (playerFile.settingDifficulty == SettingDifficulty.HARD)
                    {
                        Game.Manager.Platform.UnlockAchievement("super_savior", true);
                    }
                }
            }
            else
            {
                // another round follows; the last pair in the list is the boss pair, which plays the bonus round
                context.RoundOverCutscene = Game.Session.Puzzle.cutsceneSuccessBoss;
                context.NewRoundCutscene = status.girlListCount <= 2
                    ? Game.Session.Puzzle.cutsceneNewroundBossBonus
                    : Game.Session.Puzzle.cutsceneNewroundBoss;
                context.GameOver = false;
            }
        }
        else if (playerFile.storyProgress < PlayerFile.STORY_PROGRESS_POOLSIDE_ENDING && playerFile.GetFlagValue(Flags.NYMPHOJINN_FAILURE) < 0)
        {
            playerFile.SetFlagValue(Flags.NYMPHOJINN_FAILURE, 0);
        }
    }

    // Every round of a boss date, the bonus round included, is played by a new pair of dolls.
    public override void OnNextRound()
    {
        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            Game.Session.Location.ResetDolls(false);
        }
    }

    // A win at the end of the story goes to the post boss location, anything else back to the hub.
    public override void OnPuzzleEnded(bool succeeded)
    {
        var playerFile = Game.Persistence.playerFile;
        var toPostBoss = succeeded
            && (playerFile.storyProgress == PlayerFile.STORY_PROGRESS_POOLSIDE_ENDING || playerFile.GetFlagValue(Flags.KYU_HOLE_SELECTION) < 0);

        if (toPostBoss)
        {
            Game.Session.Location.Depart(Game.Session.Puzzle.postBossLocationDefinition, null, false);
            Owner.QueueTransition(new GameStateSpecial.Payload());
        }
        else
        {
            Game.Session.Location.Depart(Game.Session.Hub.hubLocationDefinition, null, false);
            Owner.QueueTransition(new GameStateHub.Payload());
        }
    }
}