using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

public class PuzzleResourceSentiment : BasePuzzleResource
{
    public override RelativeId Id => PuzzleResourceId.Sentiment;

    public PuzzleResourceSentiment(string name, string sign) : base(name, sign) {}

    public override bool AddResourceValue(int value, ExpandedPuzzleStatus puzzleStatus, PuzzleStatusGirl puzzleStatusGirl)
    {
        if (value == 0) return false;
        puzzleStatusGirl.sentiment += value;
        return true;
    }

    public override int GetResourceValue(ExpandedPuzzleStatus puzzleStatus, PuzzleStatusGirl puzzleStatusGirl, bool maxVal = false)
    {
        return maxVal ? 40 : puzzleStatusGirl.sentiment;
    }

    public override int CalculateMatchReward(ExpandedUiPuzzleGrid uiPuzzleGrid, ExpandedAilmentManager ailmentManager, ExpandedPuzzleSet puzzleSet, PlayerFile playerFile, PuzzleMatch match, MatchModifier matchModifier, PuzzleStatusGirl statusGirl, List<UiPuzzleSlot> orderedMatchSlots)
    {
        var args = NotifyPreMatch(uiPuzzleGrid, ailmentManager, puzzleSet, match, matchModifier, statusGirl, orderedMatchSlots);
        var reward = args.OrderedSlots.Count;
        if (!match.flatMatch) reward *= Mathf.Max(reward - 2, 1);
        return reward;
    }
}