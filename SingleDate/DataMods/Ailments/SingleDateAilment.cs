// Example: SingleDateGridModifier
//
// Replaces the SingleDate mod's UiPuzzleGrid and Ailment patches entirely.
// Registered as an IUiPuzzleGridModifier on the ExpandedUiPuzzleGrid before
// StartPuzzle runs via a UiPuzzleGrid.StartPuzzle prefix in the SingleDate mod.
//
// Behaviours:
//   - No stamina cost for moves
//   - Exhaustion and upset silently reverted after baggage triggers fire
//   - Focus switching suppressed
//   - Stamina, exhaustion, and upset warning tooltips suppressed
//   - Stamina tokens invalidated for the right girl

using Hp2BaseMod;

namespace SingleDate;

public class SingleDateGridModifier : IUiPuzzleGridModifier
{
    private TokenDefinition _staminaTokenDef;
    private PuzzleStatusGirl _rightGirl;

    public void OnApply(UiPuzzleGrid grid, ExpandedUiPuzzleGrid expanded, PuzzleStatus status)
    {
        // Suppress all stamina and focus behaviour.
        expanded.SuppressStaminaCost = true;
        expanded.SuppressFocusSwitch = true;
        expanded.SuppressStaminaWarning = true;
        expanded.SuppressExhaustionWarning = true;
        expanded.SuppressUpsetWarning = true;

        // Invalidate stamina tokens for the right girl.
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
        expanded.SuppressFocusSwitch = false;
        expanded.SuppressStaminaWarning = false;
        expanded.SuppressExhaustionWarning = false;
        expanded.SuppressUpsetWarning = false;

        if (_staminaTokenDef != null && _rightGirl != null)
        {
            _rightGirl.invalidTokenDefs.Remove(_staminaTokenDef);
        }

        _staminaTokenDef = null;
        _rightGirl       = null;
    }
}