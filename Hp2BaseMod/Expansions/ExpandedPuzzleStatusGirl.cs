namespace Hp2BaseMod;

[Expansion(typeof(PuzzleStatusGirl))]
#pragma warning disable HP001 // Deprecated member usage
[Deprecates(nameof(PuzzleStatusGirl.exhausted), $"Use {nameof(ExpandedPuzzleStatusGirl)}.{nameof(StateId)} instead")]
[Deprecates(nameof(PuzzleStatusGirl.upset), $"Use {nameof(ExpandedPuzzleStatusGirl)}.{nameof(StateId)} instead")]
#pragma warning restore HP001 // Deprecated member usage
public partial class ExpandedPuzzleStatusGirl
{
    private RelativeId _stateId = PuzzleStatusGirlStateId.Normal;

    public RelativeId StateId => _stateId;
    public IPuzzleStatusGirlState State => ModInterface.GameData.GetPuzzleStatusGirlState(_stateId);

    public void ChangeState(RelativeId targetStateId)
    {
        if (targetStateId == RelativeId.Default) return;

        // Allow sub-mods to redirect or override targetStateId
        var args = ModInterface.Events.NotifyRequestGirlStateTransition(_core, _stateId, targetStateId);
        targetStateId = args.TargetStateId;

        if (_stateId == targetStateId) return;

        var currentState = State;
        var nextState = ModInterface.GameData.GetPuzzleStatusGirlState(targetStateId);
        if (nextState == null)
        {
            ModInterface.Log.Error($"Could not find {nameof(IPuzzleStatusGirlState)} with id {targetStateId} to switch to.");
            return;
        }

        currentState?.OnExit(_core, this);
        _stateId = targetStateId;
        nextState.OnEnter(_core, this);
    }

    public void OnStaminaChanged(int newStamina)
    {
        var targetId = State?.OnStaminaChanged(_core, this, newStamina) ?? _stateId;
        if (targetId != _stateId) ChangeState(targetId);
    }

    public void OnBrokenHeartMatched(int brokenCount)
    {
        var targetId = State?.OnBrokenHeartMatched(_core, this, brokenCount) ?? _stateId;
        if (targetId != _stateId) ChangeState(targetId);
    }

    public void OnMoveCompleted()
    {
        var targetId = State?.OnMoveCompleted(_core, this) ?? _stateId;
        if (targetId != _stateId) ChangeState(targetId);
    }

    private void OnDestroy()
    {
        _stateId = PuzzleStatusGirlStateId.Normal;
        foreach (var ailment in _core.ailments)
        {
            ailment.DestroyExpansion();
        }
    }
}
