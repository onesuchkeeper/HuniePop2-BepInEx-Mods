// using System.Linq;
// using Hp2BaseMod;
// using UnityEngine;

// namespace SingleDate;

// /// <summary>
// /// The scripted ailment that is active during every single date puzzle (added by
// /// <c>SingleDateStartPuzzlePatch</c>). It adapts the puzzle to one girl: stamina isn't spent or warned about, focus
// /// can't switch, stamina tokens can't be matched for the girl, and broken heart matches take away affection like in
// /// HuniePop 1.
// /// </summary>
// public class SingleDateAilment : IScriptedAilment
// {
//     // The puzzle grid, the stamina token, the right (date) girl and the ailment manager, kept so Disable can undo what Enable did.
//     private ExpandedUiPuzzleGrid _grid;
//     private TokenDefinition _staminaTokenDef;
//     private PuzzleStatusGirl _rightGirl;
//     private ExpandedAilmentManager _ailmentManager;

//     /// <summary>
//     /// Turns off the stamina cost and the stamina, exhaustion and upset warnings, stops focus switching, makes stamina
//     /// tokens invalid for the girl, and starts watching match rewards.
//     /// </summary>
//     public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
//     {
//         _ailmentManager = ailmentManager;
//         _grid = _ailmentManager._puzzleGrid.GetExpansion();

//         _grid.SuppressStaminaCost = true;
//         _grid.SuppressStaminaWarning = true;
//         _grid.SuppressExhaustionWarning = true;
//         _grid.SuppressUpsetWarning = true;
//         _grid.SuppressFocusSwitch();

//         _rightGirl = _grid._status.girlStatusRight;
//         _staminaTokenDef = Game.Data.Tokens.GetByResourceType(PuzzleResourceType.STAMINA);
//         if (_staminaTokenDef != null && !_rightGirl.invalidTokenDefs.Contains(_staminaTokenDef)) 
//         {
//             _rightGirl.invalidTokenDefs.Add(_staminaTokenDef);
//         }

//         _ailmentManager.PostMatchReward += OnPostMatchReward;
//     }

//     /// <summary>Undoes everything <see cref="Enable"/> did.</summary>
//     public void Disable()
//     {
//         if (_ailmentManager != null)
//         {
//             _ailmentManager.PostMatchReward -= OnPostMatchReward;
//         }

//         _grid = _ailmentManager?._puzzleGrid?.GetExpansion();
//         if (_grid != null)
//         {
//             _grid.SuppressStaminaCost = false;
//             _grid.SuppressStaminaWarning = false;
//             _grid.SuppressExhaustionWarning = false;
//             _grid.SuppressUpsetWarning = false;
//             _grid.UnsuppressFocusSwitch();
//         }

//         if (_staminaTokenDef != null && _rightGirl != null) 
//         {
//             _rightGirl.invalidTokenDefs.Remove(_staminaTokenDef);
//         }

//         _staminaTokenDef = null;
//         _rightGirl = null;
//         _grid = null;
//     }

//     /// <summary>
//     /// Turns broken heart matches into an affection loss. A broken heart token takes away a fraction of the girl's
//     /// current affection (<see cref="State.GetBrokenMult"/>, which sensitivity lowers), with a minimum loss of 1. The
//     /// reward values are set to that loss, and the affection is removed from the girl.
//     /// </summary>
//     private void OnPostMatchReward(AilmentTriggerArgs.PostMatchReward args)
//     {
//         if (!State.IsSingleDate || args?.Rewards == null) return;

//         var brokenRewards = args.Rewards
//             .Where(x => x.Reward?.TokenDefinition?.PuzzleResource?.Id == PuzzleResourceId.Broken)
//             .ToList();

//         if (brokenRewards.Count == 0) return;

//         var puzzleStatus = Game.Session.Puzzle.puzzleStatus;
//         var currentAffection = puzzleStatus.affection;

//         // Base game doesn't subtract affection for broken matches,
//         // on single dates use the Hp1-mechanic and subtract affection
//         var allotment = -Mathf.Max(1, Mathf.FloorToInt(currentAffection * State.GetBrokenMult()));

//         foreach (var entry in brokenRewards)
//         {
//             entry.Reward.ResourceValue = allotment;
//         }

//         _grid._status.GetExpansion().AddResourceValue(PuzzleResourceId.AffectionTalent, allotment * brokenRewards.Count, true);
//     }
// }
