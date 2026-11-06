using Hp2BaseMod;

namespace SingleDate;

/// <summary>
/// The upset state for the girl on single dates, used in place of the base mod's Upset state (see
/// <see cref="ModEventHandles.On_RequestGirlStateTransition"/>).
/// </summary>
/// <remarks>
/// The girl stays visibly upset for the move that enters the state and clears when the next move completes,
/// instead of recovering instantly.
/// </remarks>
public class SingleDateUpsetGirlState : IPuzzleStatusGirlState
{
    // Moves completed since entering (or since the last broken heart match).
    private int _movesInState = 0;

    public RelativeId Id => PuzzleStatusGirlStateId.SingleDateUpset;

    /// <summary>Shows her upset and plays the exhausted line, and triggers exhaustion ailments.</summary>
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

    /// <summary>Clears the upset look.</summary>
    public void OnExit(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        ModInterface.Log.Message("Exited Single Date Upset");

        UiDoll doll = Game.Session.gameCanvas.GetDoll(true);

        // Clear mood expression and stop particles
        doll.ClearMood();
        doll.upsetEmitter.Stop();

        // Use UNCHECKED so recovery audio plays smoothly without UI cancellation
        // Recovery line intentionally not played, the state exits quietly. (Left here in case it is wanted later.)
        // doll.ReadDialogTrigger(Game.Session.Puzzle.dtBrokenRecovered, DialogLineFormat.UNCHECKED, -1);
    }

    /// <summary>Stamina is irrelevant on single dates (it isn't spent in the puzzle), stay upset.</summary>
    public RelativeId OnStaminaChanged(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int newStamina) => Id;

    /// <summary>Another broken heart match while upset restarts the countdown and replays the reaction.</summary>
    public RelativeId OnBrokenHeartMatched(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int brokenCount)
    {
        _movesInState = 0; // Reset timer on consecutive broken matches

        UiDoll doll = Game.Session.gameCanvas.GetDoll(true);
        doll.SetMood(GirlExpressionType.UPSET, 4f);
        doll.upsetEmitter.Init();
        doll.ReadDialogTrigger(Game.Session.Puzzle.dtBrokenExhausted, DialogLineFormat.UNCHECKED, -1);

        return Id;
    }

    /// <summary>Counts moves and returns to the normal state once the state has outlasted the next move.</summary>
    public RelativeId OnMoveCompleted(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        _movesInState++;

        // The move that entered the state completes first (_movesInState = 1) and she stays upset through it.
        // The next move completes (_movesInState = 2) and clears the state. This keeps her visibly upset until the
        // player's next move, rather than recovering the moment the match is resolved.
        if (_movesInState >= 2)
        {
            return Hp2BaseMod.PuzzleStatusGirlStateId.Normal;
        }

        return Id;
    }

    /// <summary>Counts as upset for abilities and conditions that check for it.</summary>
    public bool SatisfiesCondition(AbilityStepConditionType abilityStepConditionType) 
        => abilityStepConditionType == AbilityStepConditionType.IS_UPSET;

    /// <summary>Counts as upset (and as "exhausted or upset") for girl conditions.</summary>
    public bool SatisfiesCondition(GirlConditionType girlConditionType) 
        => girlConditionType == GirlConditionType.UPSET || girlConditionType == GirlConditionType.EXHAUSTED_OR_UPSET;

    /// <summary>Reviving returns her to normal.</summary>
    public RelativeId OnRevive(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion)
    {
        return Hp2BaseMod.PuzzleStatusGirlStateId.Normal;
    }
}
