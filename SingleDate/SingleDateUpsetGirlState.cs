using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace SingleDate;

public class SingleDateUpsetGirlState : IPuzzleStatusGirlState
{
    private int _movesInState = 0;

    public RelativeId Id => PuzzleStatusGirlStateId.SingleDateUpset;

    public void OnEnter(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        ModInterface.Log.Message("Entered Single Date Upset");

        _movesInState = 0;

        UiDoll doll = Game.Session.gameCanvas.GetDoll(true); // Right doll on single dates

        // Set upset mood and initialize particle emitter
        doll.SetMood(GirlExpressionType.UPSET, 4f);
        doll.upsetEmitter.Init();

        // Use UNCHECKED so the voice line isn't cut off by grid updates
        doll.ReadDialogTrigger(Game.Session.Puzzle.dtBrokenExhausted, DialogLineFormat.UNCHECKED, -1);
        Game.Session.Ailment.Trigger(AilmentTriggerType.ON_EXHAUSTION);
    }

    public void OnExit(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        ModInterface.Log.Message("Exited Single Date Upset");

        UiDoll doll = Game.Session.gameCanvas.GetDoll(true);

        // Clear mood expression and stop particles
        doll.ClearMood();
        doll.upsetEmitter.Stop();

        // Use UNCHECKED so recovery audio plays smoothly without UI cancellation
        // doll.ReadDialogTrigger(Game.Session.Puzzle.dtBrokenRecovered, DialogLineFormat.UNCHECKED, -1);
    }

    public RelativeId OnStaminaChanged(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int newStamina) => Id;

    public RelativeId OnBrokenHeartMatched(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int brokenCount)
    {
        _movesInState = 0; // Reset timer on consecutive broken matches

        UiDoll doll = Game.Session.gameCanvas.GetDoll(true);
        doll.SetMood(GirlExpressionType.UPSET, 4f);
        doll.upsetEmitter.Init();
        doll.ReadDialogTrigger(Game.Session.Puzzle.dtBrokenExhausted, DialogLineFormat.UNCHECKED, -1);

        return Id;
    }

    public RelativeId OnMoveCompleted(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        _movesInState++;

        // Persists through the matching turn (_movesInState = 1) 
        // and clears on completion of the next move (_movesInState = 2)
        if (_movesInState >= 2)
        {
            return Hp2BaseMod.PuzzleStatusGirlStateId.Normal;
        }

        return Id;
    }

    public bool SatisfiesCondition(AbilityStepConditionType abilityStepConditionType) 
        => abilityStepConditionType == AbilityStepConditionType.IS_UPSET;

    public bool SatisfiesCondition(GirlConditionType girlConditionType) 
        => girlConditionType == GirlConditionType.UPSET || girlConditionType == GirlConditionType.EXHAUSTED_OR_UPSET;
}