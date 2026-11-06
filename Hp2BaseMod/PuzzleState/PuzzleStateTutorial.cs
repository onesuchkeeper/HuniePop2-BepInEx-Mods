// Hp2BaseMod 2026, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod;

/// <summary>
/// The tutorial date, played at the tutorial location (the date the old <c>PuzzleStatus.IsTutorial</c> was about). It is set up and styled like a
/// normal date, but it is scripted: it cannot fail, the player's moves and gifts are limited to what the current tutorial step expects,
/// and it pays out a fixed round of seeds. This is the only place that knows what a tutorial is.
/// </summary>
public sealed class PuzzleStateTutorial(IGameStateLocation owner) : PuzzleStateNormal(owner)
{
    public new class Factory : IPuzzleStateFactory
    {
        public IFullPuzzleState Create(IGameStateLocation gameState) => new PuzzleStateTutorial(gameState);
    }

    private static readonly RelativeId[] REWARD_AFFECTIONS =
    [
        PuzzleAffectionId.Talent,
        PuzzleAffectionId.Flirtation,
        PuzzleAffectionId.Romance,
        PuzzleAffectionId.Sexuality,
    ];

    private int _rewardAffectionIndex;

    // The scripted layout of the grid, one column per row of the string. 0 is an empty slot.
    private const string PRELOADED_TOKENS = "9,7,1,2,5,2,6,4,1,1,9|8,6,8,4,1,8,8,8,8,3,8,6,3,6,2|5,4,7,4,8,7,1|2,1,5,7,2,5,8,9|3,6,3,7,9,2,5,4,3,5|5,3,5,4,1,6,2,9,3,4|9,6,3,1,8,5,1,7|6,1,4,2,4,5,7,2,3,6|4,9,8,6,9,9,3,9,8,1,2";

    // The player can skip the tutorial. It then plays as an ordinary date at the tutorial location.
    private static bool Skipped => Game.Persistence.playerFile.GetFlagValue(Flags.SKIP_TUTORIAL) > 0;

    public override void OnArrivalCompleted(ArrivalCompletedArgs args)
    {
        if (args.ArrivalCutscene != null) return;

        args.ArrivalCutscene = Game.Session.Puzzle.cutsceneTutorialIntro;
    }

    // ---- puzzle lifecycle ----

    // The scripted token layout has to be in place before the grid creates its first tokens.
    public override void OnPuzzleStarting()
    {
        if (Skipped) return;

        Game.Persistence.playerFile.SetFlagValue(Flags.TUTORIAL_PROGRESS, 0);

        var preloadedTokens = Game.Session.Puzzle.puzzleGrid.preloadedTokens;
        var columns = PRELOADED_TOKENS.Split('|');
        for (int i = 0; i < columns.Length; i++)
        {
            preloadedTokens.Add(new List<TokenDefinition>());
            foreach (var entry in columns[i].Split(','))
            {
                int tokenId = StringUtils.ParseIntValue(entry);
                preloadedTokens[i].Add(tokenId > 0 ? Game.Data.Tokens.Get(tokenId) : null);
            }
        }
    }

    // The tutorial's script starts once the grid first settles.
    public override void OnPuzzleStateChanged()
    {
        if (Skipped) return;

        if (Game.Persistence.playerFile.GetFlagValue(Flags.TUTORIAL_PROGRESS) == 0
            && Game.Session.Puzzle.puzzleGrid.state == PuzzleGameState.WAITING)
        {
            Game.Persistence.playerFile.SetFlagValue(Flags.TUTORIAL_PROGRESS, 1);
            Game.Session.Cutscenes.StartCutscene(Game.Session.Puzzle.cutsceneTutorialDate, null);
        }
    }

    // The bonus round of the tutorial is played by the left girl alone. Picking by stamina would be a coin flip, both girls start it with the
    // same stamina, and the focused girl is the one the puzzle's dialog comes from.
    public override void OnNextRound()
    {
        if (Game.Session.Puzzle.puzzleStatus.bonusRound)
        {
            Game.Session.Puzzle.puzzleStatus.SetGirlFocus(false);
        }
    }

    // The tutorial always goes on to the post tutorial location (the airplane cabin).
    public override void OnPuzzleEnded(bool succeeded)
    {
        Game.Session.Location.Depart(Game.Session.Puzzle.postTutorialLocationDefinition, null, false);
        Owner.QueueTransition(new GameStateSpecial.Payload());
    }

    // ---- rules of play ----

    public override bool FailsWhenOutOfMoves => false;

    // a scripted puzzle has nothing to give up on
    public override bool AllowsForfeit => false;

    public override bool IsRepeatDate => false;

    // ---- the tutorial's script ----
    // What the player may do is whatever the current step of the script asks for. A player who skipped the tutorial may do anything.

    public override bool MatchesTutorialCondition(bool requireScripted) => !requireScripted || !Skipped;

    public override bool AllowsAction(PuzzleTutorialStepType action)
    {
        if (Skipped) return true;

        var step = Game.Session.Puzzle.GetTutorialStep();
        return (step.stepType == PuzzleTutorialStepType.NONE && step.alt) || step.stepType == action;
    }

    public override bool AllowsMove(UiPuzzleSlot from, UiPuzzleSlot to)
    {
        if (Skipped) return true;
        if (from == null || to == null) return false;

        var step = Game.Session.Puzzle.GetTutorialStep();
        if (step.stepType == PuzzleTutorialStepType.NONE) return step.alt;

        return step.stepType == PuzzleTutorialStepType.MATCH
            && step.fromKey == from.key
            && step.toKey.Split('|').Contains(to.key);
    }

    public override bool AllowsTokenPickup(UiPuzzleSlot slot)
    {
        if (!AllowsAction(PuzzleTutorialStepType.MATCH)) return false;
        if (Skipped) return true;

        var step = Game.Session.Puzzle.GetTutorialStep();
        return step.stepType != PuzzleTutorialStepType.MATCH || step.fromKey == slot.key;
    }

    // While the tutorial's cutscenes play, the girls stay quiet (outside of its bonus round).
    public override bool SuppressesDollDialog
        => !Skipped
        && Game.Session.Cutscenes.isCutscenePlaying
        && !Game.Session.Puzzle.puzzleStatus.bonusRound;

    public override bool AllowsGift => AllowsAction(PuzzleTutorialStepType.GIFT);

    // ---- rules of the status ----

    public override bool ChecksStaminaFreezeOnStart => false;

    // The tutorial date can never run out of moves.
    public override int MinMovesRemaining => 1;

    public override void GetRoundGoal(int roundIndex, bool bonusRound, out int affectionGoal, out float bonusDrainDelay)
    {
        affectionGoal = bonusRound ? 400 : 200;
        bonusDrainDelay = bonusRound ? 0.1f : -1f;
    }

    // The tutorial is never numbered.
    public override bool TryGetHeaderLabel(out PuzzleHeaderLabel label)
    {
        label = default;
        return false;
    }

    // Only the left girl is in the mood after a photo.
    protected override bool BothGirlsHornyAfterPhoto => false;

    public override bool CreatesReplacementTokens => false;

    public override bool SwitchesFocusInBonusRound => false;

    // ---- rewards after the puzzle ----

    // The tutorial never unlocks styles and never pays a failure reward.
    public override bool UnlocksStylesAfterRewards => false;

    public override int GetPostRewardFruitCount(int baseFruitCount, out bool failed)
    {
        _rewardAffectionIndex = 0;
        failed = false;
        return baseFruitCount;
    }

    // The seeds cycle through the four affections, two per reward.
    public override void GetPostRewardFruits(int rewardIndex, out ItemDefinition forFocused, out ItemDefinition forUnfocused)
    {
        forFocused = ModInterface.GameData.GetAffection(REWARD_AFFECTIONS[_rewardAffectionIndex++ % REWARD_AFFECTIONS.Length]).GetRandomFruit();
        forUnfocused = ModInterface.GameData.GetAffection(REWARD_AFFECTIONS[_rewardAffectionIndex++ % REWARD_AFFECTIONS.Length]).GetRandomFruit();
    }
}