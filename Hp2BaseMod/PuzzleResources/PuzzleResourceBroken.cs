using System.Collections.Generic;

namespace Hp2BaseMod;

public class PuzzleResourceBroken : BasePuzzleResource
{
    public override RelativeId Id => PuzzleResourceId.Broken;

    public PuzzleResourceBroken(string name, string sign) 
        : base(name, sign) {}

    public override bool AddResourceValue(int value, 
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl)
    {
        if (value == 0) return false;
		puzzleStatusGirl.stamina -= puzzleStatusGirl.stamina;
		puzzleStatusGirl.upset = true;
		return true;
    }

    public override int CalculateRewardPortion(int totalRewardAmount, int count, int tokenIndex) => totalRewardAmount;

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

        return args.OrderedSlots.Count;
    }

    public override (string splashText, string burstText) GetLabelText(
        int resourceValue, 
        bool bonusRound = false)
    {
        if (resourceValue == 0) return (null, null);
        return (null, _name);
    }
}
