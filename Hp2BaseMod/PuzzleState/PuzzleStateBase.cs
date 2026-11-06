// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// The rules every kind of puzzle starts from. The defaults are a date's, except where a kind has nothing to do (no doll styling, no
/// style unlocks); a kind overrides only what is unique to it. What a round ending does and where a finished puzzle leads
/// have no sensible default, so every kind has to say.
/// </summary>
public abstract class PuzzleStateBase(IGameStateLocation owner) : IFullPuzzleState
{
    public virtual bool? LockedAltGirlFocus => null;
    
    protected IGameStateLocation Owner { get; } = owner;
    private bool _disposed = false;
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            //managed
        }

        //unmanaged
    }

    ///<inheritdoc/>
    public abstract void OnLocationArrived(LocationArriveArgs args);

    ///<inheritdoc/>
    public virtual void OnArrivalCompleted(ArrivalCompletedArgs args)
    {
        if (args.ArrivalCutscene != null) return;

        args.ArrivalCutscene = Game.Session.Puzzle.cutsceneIntro;
    }

    ///<inheritdoc/>
    public virtual void OnLocationSettled(LocationSettledArgs args)
    {
        ModInterface.Log.Message("GameState.Puzzle: settled - starting puzzle");
        Game.Session.Puzzle.StartPuzzle();
    }

    // Nothing to style by default, keep what the base game loaded.
    public virtual bool TryResolveDollStyles(ResolveDollStylesArgs args) => true;

    ///<inheritdoc/>
    public virtual void OnDeactivated() { }

    ///<inheritdoc/>
    public virtual void OnPuzzleStarting() { }

    ///<inheritdoc/>
    public abstract void OnRoundOver(PuzzleRoundOverContext context);

    ///<inheritdoc/>
    public virtual void OnNextRound() { }

    ///<inheritdoc/>
    public virtual void OnPuzzleStateChanged() { }

    ///<inheritdoc/>
    public abstract void OnPuzzleEnded(bool succeeded);

    // The numbers of a normal date, indexed by difficulty (EASY, NORMAL, HARD).
    private static readonly int[] _initialMoves = [30, 25, 20];
    private static readonly int[] _maxMoves = [35, 30, 25];

    ///<inheritdoc/>
    public virtual int GetInitialMoves(SettingDifficulty difficulty) => _initialMoves[(int)difficulty];

    ///<inheritdoc/>
    public virtual int GetMaxMoves(SettingDifficulty difficulty) => _maxMoves[(int)difficulty];

    ///<inheritdoc/>
    public virtual int MinMovesRemaining => 0;

    ///<inheritdoc/>
    public virtual bool ChecksStaminaFreezeOnStart => false;

    ///<inheritdoc/>
    public virtual bool PopulatesRandomAilments => false;

    ///<inheritdoc/>
    public abstract bool IsBonusRound(int roundIndex, int girlsRemaining);

    // The next two girls in line play, and leave the line.
    public virtual void TakeRoundGirls(List<GirlDefinition> girlList, GirlDefinition[] roundGirls)
    {
        for (int i = 0; i < roundGirls.Length; i++)
        {
            roundGirls[i] = girlList[0];
            girlList.RemoveAt(0);
        }
    }

    ///<inheritdoc/>
    public abstract void GetRoundGoal(int roundIndex, bool bonusRound, out int affectionGoal, out float bonusDrainDelay);

    ///<inheritdoc/>
    public abstract PuzzleStatusType ConditionDateType { get; }

    ///<inheritdoc/>
    public virtual bool MatchesTutorialCondition(bool requireScripted) => false;

    ///<inheritdoc/>
    public virtual bool FailsWhenOutOfMoves => true;

    ///<inheritdoc/>
    public virtual bool AllowsAction(PuzzleTutorialStepType action) => true;

    ///<inheritdoc/>
    public virtual bool AllowsTokenPickup(UiPuzzleSlot slot) => true;

    ///<inheritdoc/>
    public virtual bool SuppressesDollDialog => false;

    ///<inheritdoc/>
    public virtual bool TryGetHeaderLabel(out PuzzleHeaderLabel label)
    {
        label = default;
        return false;
    }

    /// <summary>
    /// Whether both girls are in the mood after the photo window closes. The tutorial only has the left one.
    /// </summary>
    protected virtual bool BothGirlsHornyAfterPhoto => true;

    ///<inheritdoc/>
    public virtual void OnPhotoWindowClosed()
    {
        var canvas = Game.Session.gameCanvas;
        canvas.dollLeft.StopDialogLine(true, false);
        canvas.dollRight.StopDialogLine(true, false);
        canvas.dollLeft.ClearMood();
        canvas.dollRight.ClearMood();
        canvas.dollLeft.ChangeExpression(GirlExpressionType.HORNY, false);
        if (BothGirlsHornyAfterPhoto)
        {
            canvas.dollRight.ChangeExpression(GirlExpressionType.HORNY, false);
        }

        canvas.header.Refresh(false);
    }

    ///<inheritdoc/>
    public virtual bool AllowsMove(UiPuzzleSlot from, UiPuzzleSlot to) => true;

    ///<inheritdoc/>
    public virtual bool AllowsGift => true;

    ///<inheritdoc/>
    public virtual bool AllowsForfeit => true;

    ///<inheritdoc/>
    public virtual bool CreatesReplacementTokens => true;

    ///<inheritdoc/>
    public virtual bool SwitchesFocusInBonusRound => true;

    ///<inheritdoc/>
    public virtual bool IsRepeatDate => false;

    ///<inheritdoc/>
    public virtual bool UnlocksStylesAfterRewards => false;

    ///<inheritdoc/>
    public virtual bool UnlocksStylesAfterRoundClear => false;

    ///<inheritdoc/>
    public virtual int GetPostRewardFruitCount(int baseFruitCount, out bool failed)
    {
        failed = false;
        return baseFruitCount;
    }

    ///<inheritdoc/>
    public virtual void GetPostRewardFruits(int rewardIndex, out ItemDefinition forFocused, out ItemDefinition forUnfocused)
    {
        // each girl's own fruit
        var status = Game.Session.Puzzle.puzzleStatus;
        forFocused = status.girlStatusFocused.girlDefinition.GetExpansion().GetRandomFruit();
        forUnfocused = status.girlStatusUnfocused.girlDefinition.GetExpansion().GetRandomFruit();
    }
}