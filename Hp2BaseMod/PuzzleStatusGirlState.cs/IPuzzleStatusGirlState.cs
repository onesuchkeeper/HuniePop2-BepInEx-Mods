namespace Hp2BaseMod;

public interface IPuzzleStatusGirlState
{
    RelativeId Id { get; }

    /// <summary>
    /// Legacy support, if condition is met by this state
    /// </summary>
    bool SatisfiesCondition(AbilityStepConditionType abilityStepConditionType);

    /// <summary>
    /// Legacy support, if condition is met by this state
    /// </summary>
    bool SatisfiesCondition(GirlConditionType girlConditionType);

    void OnEnter(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion);
    void OnExit(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion);

    RelativeId OnStaminaChanged(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int newStamina);
    RelativeId OnBrokenHeartMatched(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int brokenCount);
    RelativeId OnMoveCompleted(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion);
    RelativeId OnRevive(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion);
}