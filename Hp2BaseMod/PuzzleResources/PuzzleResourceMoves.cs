using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

public class PuzzleResourceMoves : BasePuzzleResource
{
    public override RelativeId Id => PuzzleResourceId.Moves;

    public PuzzleResourceMoves(string name, string sign) : base(name, sign) {}

    public override bool AddResourceValue(int value, ExpandedPuzzleStatus puzzleStatus, PuzzleStatusGirl puzzleStatusGirl)
    {
        if (value == 0) return false;
        puzzleStatus.Core.movesRemaining += value;
        return true;
    }

    public override int GetResourceValue(ExpandedPuzzleStatus puzzleStatus, PuzzleStatusGirl puzzleStatusGirl, bool maxVal = false)
    {
        return maxVal ? puzzleStatus.Core.maxMovesRemaining : puzzleStatus.Core.movesRemaining;
    }

    public override int CalculateMatchReward(ExpandedUiPuzzleGrid uiPuzzleGrid, ExpandedAilmentManager ailmentManager, ExpandedPuzzleSet puzzleSet, PlayerFile playerFile, PuzzleMatch match, MatchModifier matchModifier, PuzzleStatusGirl statusGirl, List<UiPuzzleSlot> orderedMatchSlots)
    {
        var args = NotifyPreMatch(uiPuzzleGrid, ailmentManager, puzzleSet, match, matchModifier, statusGirl, orderedMatchSlots);
        var reward = args.OrderedSlots.Count;
        return reward + Mathf.Max(reward - 2, 0);
    }
}