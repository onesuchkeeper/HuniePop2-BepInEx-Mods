using Hp2BaseMod;

namespace SingleDate;

public class SingleDateAilment : IScriptedAilment
{
    private ExpandedUiPuzzleGrid _grid;
    private TokenDefinition _staminaTokenDef;
    private PuzzleStatusGirl _rightGirl;
    private ExpandedAilmentManager _ailmentManager;
    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _ailmentManager = ailmentManager;
        _grid = _ailmentManager._puzzleGrid.GetExpansion();
        _grid.SuppressStaminaCost = true;
        _grid.SuppressStaminaWarning = true;
        _grid.SuppressExhaustionWarning = true;
        _grid.SuppressUpsetWarning = true;
        _grid.AutoRevertExhaustion = true;
        _grid.SuppressFocusSwitch();

        _rightGirl = _grid._status.girlStatusRight;
        _staminaTokenDef = Game.Data.Tokens.GetByResourceType(PuzzleResourceType.STAMINA);
        if (_staminaTokenDef != null && !_rightGirl.invalidTokenDefs.Contains(_staminaTokenDef)) 
        {
            _rightGirl.invalidTokenDefs.Add(_staminaTokenDef);
        }
    }

    public void Disable()
    {
        _grid = _ailmentManager._puzzleGrid.GetExpansion();
        _grid.SuppressStaminaCost = false;
        _grid.SuppressStaminaWarning = false;
        _grid.SuppressExhaustionWarning = false;
        _grid.SuppressUpsetWarning = false;
        _grid.AutoRevertExhaustion = false;
        _grid.UnsuppressFocusSwitch();

        if (_staminaTokenDef != null && _rightGirl != null) 
        {
            _rightGirl.invalidTokenDefs.Remove(_staminaTokenDef);
        }

        _staminaTokenDef = null;
        _rightGirl = null;
        _grid = null;
    }
}