// Hp2BaseMod 2026, By OneSuchKeeper


using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

/// <summary>
/// The non-stop date (the hub's non-stop option): no girl pair, a status made of the whole pool of regular girls. Each round takes two
/// of them from the first half of the list and moves them to the back, so unlike the boss date the pool needs no particular size.
/// </summary>
public sealed class PuzzleStateNonstop(IGameStateLocation owner) : PuzzleStateBase(owner)
{
    public class Factory : IPuzzleStateFactory
    {
        public IFullPuzzleState Create(IGameStateLocation gameState) => new PuzzleStateNonstop(gameState);
    }

    // Reset only builds a NONSTOP status for more than 2 girls.
    private const int MinimumGirls = 3;

    public override void OnLocationArrived(LocationArriveArgs args)
    {
        var girls = PuzzleGirlPool.GetMetRegularGirls();
        PuzzleGirlPool.PadToAtLeast(girls, MinimumGirls, "Non-stop");
        ListUtils.ShuffleList(girls);

        Game.Session.Puzzle.puzzleStatus.GetExpansion().Reset(girls);
        Game.Session.Puzzle.puzzleStatus.PopulateAilments();

        ModInterface.Log.Message($"Non-stop: {girls.Count} girls in the pool");
    }

    public override void OnLocationSettled(LocationSettledArgs args)
    {
        ModInterface.Log.Message("GameState.Puzzle (non-stop): starting puzzle");
        Game.Session.Puzzle.StartPuzzle();
    }

    // ---- rules of the status ----

    private static readonly int[] _initialMoves = [12, 10, 8];

    public override int GetInitialMoves(SettingDifficulty difficulty) => _initialMoves[(int)difficulty];

    // Moves do not cap: the rounds are long.
    public override int GetMaxMoves(SettingDifficulty difficulty) => 999;

    public override bool PopulatesRandomAilments => true;

    // There is no bonus round, only more rounds.
    public override bool IsBonusRound(int roundIndex, int girlsRemaining) => false;

    public override PuzzleStatusType ConditionDateType => PuzzleStatusType.NONSTOP;

    // Two girls come from the first half of the pool, and go to the back of it.
    public override void TakeRoundGirls(List<GirlDefinition> girlList, GirlDefinition[] roundGirls)
    {
        for (int i = 0; i < roundGirls.Length; i++)
        {
            int index = Random.Range(0, Mathf.RoundToInt(girlList.Count * 0.5f));
            roundGirls[i] = girlList[index];
            girlList.Add(girlList[index]);
            girlList.RemoveAt(index);
        }
    }

    // Each round asks for 250 more than the last.
    public override void GetRoundGoal(int roundIndex, bool bonusRound, out int affectionGoal, out float bonusDrainDelay)
    {
        affectionGoal = 250 + 250 * roundIndex;
        bonusDrainDelay = -1f;
    }

    public override bool TryGetHeaderLabel(out PuzzleHeaderLabel label)
    {
        label = new PuzzleHeaderLabel(PuzzleHeaderIcon.Round, "ROUND #" + (Game.Session.Puzzle.puzzleStatus.roundIndex + 1));
        return true;
    }

    // Each cleared round unlocks the styles the girls wore.
    public override bool UnlocksStylesAfterRoundClear => true;

    // ---- puzzle lifecycle ----

    // Winning a round starts the next one, with no end in sight. Losing ends it, and the rounds played are the player's record.
    public override void OnRoundOver(PuzzleRoundOverContext context)
    {
        var puzzle = Game.Session.Puzzle;
        var status = puzzle.puzzleStatus;

        if (context.RoundSucceeded)
        {
            context.RoundOverCutscene = puzzle.cutsceneSuccessNonstop;
            context.NewRoundCutscene = puzzle.cutsceneNewroundNonstop;
            context.GameOver = false;
            return;
        }

        if (status.roundIndex > 0)
        {
            context.RoundOverCutscene = puzzle.cutsceneSuccess;
        }

        var playerFile = Game.Persistence.playerFile;
        if (status.roundIndex > playerFile.nonstopDateCount)
        {
            playerFile.nonstopDateCount = status.roundIndex;
            if (playerFile.nonstopDateCount >= 8)
            {
                Game.Manager.Platform.UnlockAchievement("non_stop_novice", true);
                if (playerFile.nonstopDateCount >= 16)
                {
                    Game.Manager.Platform.UnlockAchievement("non_stop_ninja", true);
                }
            }
        }
    }

    // A finished non-stop date always goes back to the hub.
    public override void OnPuzzleEnded(bool succeeded)
    {
        Game.Session.Location.Depart(Game.Session.Hub.hubLocationDefinition, null, false);
        Owner.QueueTransition(new GameStateHub.Payload());
    }

    // ---- rewards after the puzzle ----

    // The pool pays out by how many rounds were played.
    public override int GetPostRewardFruitCount(int baseFruitCount, out bool failed)
    {
        failed = false;

        var fruitCount = 0;
        var perRound = 2;
        for (var i = 0; i < Game.Session.Puzzle.puzzleStatus.roundIndex; i++)
        {
            if (i % 2 == 0 && i > 0)
            {
                perRound += 2;
            }

            fruitCount += perRound;
        }

        return fruitCount * 2;
    }

    // Any fruit, one at a time, alternating between the focused girl and the other.
    public override void GetPostRewardFruits(int rewardIndex, out ItemDefinition forFocused, out ItemDefinition forUnfocused)
    {
        var fruit = ModInterface.GameData.GetRandomFruit();

        forFocused = rewardIndex % 2 == 0 ? fruit : null;
        forUnfocused = rewardIndex % 2 == 0 ? null : fruit;
    }
}