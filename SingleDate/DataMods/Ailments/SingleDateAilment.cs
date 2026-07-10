using System.Collections.Generic;
using Hp2BaseMod;

namespace SingleDate;

public class SingleDateGridModifier : IPuzzleGridModifier
{
    private ExpandedUiPuzzleGrid _grid;
    private TokenDefinition _staminaTokenDef;
    private PuzzleStatusGirl _rightGirl;

    public void OnApply(UiPuzzleGrid grid, ExpandedUiPuzzleGrid expanded, PuzzleStatus status)
    {
        _grid = expanded;
        expanded.SuppressStaminaCost = true;
        expanded.SuppressStaminaWarning = true;
        expanded.SuppressExhaustionWarning = true;
        expanded.SuppressUpsetWarning = true;
        expanded.SuppressFocusSwitch();

        _rightGirl = status.girlStatusRight;
        _staminaTokenDef = Game.Data.Tokens.GetByResourceType(PuzzleResourceType.STAMINA);
        if (_staminaTokenDef != null && !_rightGirl.invalidTokenDefs.Contains(_staminaTokenDef)) 
        {
            _rightGirl.invalidTokenDefs.Add(_staminaTokenDef);
        }
    }

    public void OnRemove(UiPuzzleGrid grid, ExpandedUiPuzzleGrid expanded, PuzzleStatus status)
    {
        expanded.SuppressStaminaCost = false;
        expanded.SuppressStaminaWarning = false;
        expanded.SuppressExhaustionWarning = false;
        expanded.SuppressUpsetWarning = false;
        expanded.UnsuppressFocusSwitch();

        if (_staminaTokenDef != null && _rightGirl != null) 
        {
            _rightGirl.invalidTokenDefs.Remove(_staminaTokenDef);
        }

        _staminaTokenDef = null;
        _rightGirl = null;
        _grid = null;
    }

    public bool CanEnableAilment(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl) => true;

    public void OnTrigger(
        AilmentTriggerType triggerType,
        PuzzleSet move,
        MoveModifier moveModifier,
        PuzzleMatch match,
        MatchModifier matchModifier,
        PuzzleStatus status) { }

    public void OnPreMatchReward(PuzzleRewardContext context, PuzzleStatus status) { }

    public void OnPostMatchReward(
        PuzzleRewardContext context,
        Dictionary<UiPuzzleSlot, PuzzleReward> rewards,
        PuzzleStatus status) { }

    public void OnPostSetReward(PuzzleConsumeContext context, PuzzleStatus status) { }

    public bool OnAttemptFocusSwitch(PuzzleStatus status) => true;

    public void OnRoundStart(PuzzleStatus status) { }

    public void OnRoundEnd(PuzzleRoundContext context, PuzzleStatus status) { }

    public void OnResourceChanged(PuzzleStatus status)
    {
        // Baggage triggers are allowed to fire on exhaustion, but the exhausted
        // and upset states must not persist when stamina costs are suppressed.
        _grid.RevertExhaustion(status.girlStatusLeft);
        _grid.RevertExhaustion(status.girlStatusRight);
    }

    public void OnSettled(PuzzleStatus status) { }
}