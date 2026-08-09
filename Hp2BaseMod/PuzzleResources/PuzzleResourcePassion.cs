using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

public class PuzzleResourcePassion : BasePuzzleResource
{
    public override RelativeId Id => PuzzleResourceId.Passion;

    public PuzzleResourcePassion(string name, string sign) 
        : base(name, sign) {}

    public override bool AddResourceValue(int value, 
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl)
    {
        if (value == 0) return false;
		puzzleStatusGirl.passion += value;
		return true;
    }

    public override int CalculateMatchReward(ExpandedUiPuzzleGrid uiPuzzleGrid, 
        ExpandedAilmentManager ailmentManager, 
        ExpandedPuzzleSet puzzleSet, 
        PlayerFile playerFile, 
        PuzzleMatch match, 
        MatchModifier matchModifier, 
        PuzzleStatusGirl statusGirl, 
        List<UiPuzzleSlot> orderedMatchSlots)
    {
        var args = NotifyPreMatch(uiPuzzleGrid,
            ailmentManager,
            puzzleSet,
            match, 
            matchModifier,
            statusGirl, 
            orderedMatchSlots);

        var reward = args.OrderedSlots.Count;

        if (!match.flatMatch)
        {
            reward *= Mathf.Max(reward - 2, 1);
        }

        return reward * 5;
    }
}