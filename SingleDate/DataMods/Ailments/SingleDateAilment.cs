using System.Linq;
using Hp2BaseMod;
using UnityEngine;

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
        _grid.SuppressFocusSwitch();

        _rightGirl = _grid._status.girlStatusRight;
        _staminaTokenDef = Game.Data.Tokens.GetByResourceType(PuzzleResourceType.STAMINA);
        if (_staminaTokenDef != null && !_rightGirl.invalidTokenDefs.Contains(_staminaTokenDef)) 
        {
            _rightGirl.invalidTokenDefs.Add(_staminaTokenDef);
        }

        _ailmentManager.PostMatchReward += OnPostMatchReward;
    }

    public void Disable()
    {
        if (_ailmentManager != null)
        {
            _ailmentManager.PostMatchReward -= OnPostMatchReward;
        }

        _grid = _ailmentManager?._puzzleGrid?.GetExpansion();
        if (_grid != null)
        {
            _grid.SuppressStaminaCost = false;
            _grid.SuppressStaminaWarning = false;
            _grid.SuppressExhaustionWarning = false;
            _grid.SuppressUpsetWarning = false;
            _grid.UnsuppressFocusSwitch();
        }

        if (_staminaTokenDef != null && _rightGirl != null) 
        {
            _rightGirl.invalidTokenDefs.Remove(_staminaTokenDef);
        }

        _staminaTokenDef = null;
        _rightGirl = null;
        _grid = null;
    }

    private void OnPostMatchReward(AilmentTriggerArgs.PostMatchReward args)
    {
        if (!State.IsSingleDate || args?.Rewards == null) return;

        var brokenRewards = args.Rewards
            .Where(x => x.Reward?.TokenDefinition?.PuzzleResource?.Id == PuzzleResourceId.Broken)
            .ToList();

        if (brokenRewards.Count == 0) return;

        var puzzleStatus = Game.Session.Puzzle.puzzleStatus;
        var currentAffection = puzzleStatus.affection;

        var allotment = -Mathf.Max(1, Mathf.FloorToInt(currentAffection * State.GetBrokenMult()));

        foreach (var entry in brokenRewards)
        {
            entry.Reward.ResourceValue = allotment;
        }

        _grid._status.GetExpansion().AddResourceValue(PuzzleResourceId.AffectionTalent, allotment * brokenRewards.Count, true);
    }
}