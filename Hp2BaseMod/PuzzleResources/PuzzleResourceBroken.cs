using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod;

public class PuzzleResourceBroken : BasePuzzleResource
{
    public override RelativeId Id => PuzzleResourceId.Broken;

    public PuzzleResourceBroken(string name, string sign) : base(name, sign) {}

    public override bool AddResourceValue(int value, ExpandedPuzzleStatus puzzleStatus, PuzzleStatusGirl puzzleStatusGirl)
    {
        if (value == 0) return false;
        puzzleStatusGirl.stamina -= puzzleStatusGirl.stamina;
        puzzleStatusGirl.upset = true;
        
        // Notify the state machine that broken hearts were matched
        puzzleStatusGirl.GetExpansion().OnBrokenHeartMatched(value);
        return true;
    }

    public override int CalculateRewardPortion(int totalRewardAmount, int count, int tokenIndex) => totalRewardAmount;

    public override int CalculateMatchReward(ExpandedUiPuzzleGrid uiPuzzleGrid, ExpandedAilmentManager ailmentManager, ExpandedPuzzleSet puzzleSet, PlayerFile playerFile, PuzzleMatch match, MatchModifier matchModifier, PuzzleStatusGirl statusGirl, List<UiPuzzleSlot> orderedMatchSlots)
    {
        var args = NotifyPreMatch(uiPuzzleGrid, ailmentManager, puzzleSet, match, matchModifier, statusGirl, orderedMatchSlots);
        return args.OrderedSlots.Count;
    }

    public override (string splashText, string burstText) GetLabelText(int resourceValue, bool bonusRound = false)
    {
        // Standard broken heart match with no numeric value attached
        if (resourceValue == 0) return (null, _name);

        // When a custom penalty or value is assigned to broken rewards (e.g. -15), format as splash text
        string text = (resourceValue >= 0 ? "+" : "-") + Mathf.Abs(resourceValue) + " Affection";
        return (text, text);
    }
}
