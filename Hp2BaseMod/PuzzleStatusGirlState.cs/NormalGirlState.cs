namespace Hp2BaseMod;

public class NormalGirlState : IPuzzleStatusGirlState
{
    public RelativeId Id => PuzzleStatusGirlStateId.Normal;

    public bool SatisfiesCondition(AbilityStepConditionType abilityStepConditionType) => false;
    public bool SatisfiesCondition(GirlConditionType girlConditionType) => false;

    public void OnEnter(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion) { }
    public void OnExit(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion) { }

    public RelativeId OnStaminaChanged(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int newStamina)
    {
        if (newStamina <= 0)
        {
            return PuzzleStatusGirlStateId.Exhausted;
        }
        return Id;
    }

    public RelativeId OnBrokenHeartMatched(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion, int brokenCount)
    {
        return PuzzleStatusGirlStateId.Upset;
    }

    public RelativeId OnMoveCompleted(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion) => Id;
    public RelativeId OnRevive(PuzzleStatusGirl statusGirl, ExpandedPuzzleStatusGirl expansion) => Id;
}