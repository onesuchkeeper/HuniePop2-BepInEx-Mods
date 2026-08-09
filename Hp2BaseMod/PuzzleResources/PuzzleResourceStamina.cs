using System.Collections.Generic;

namespace Hp2BaseMod;

public class PuzzleResourceStamina : BasePuzzleResource
{
    public override RelativeId Id => PuzzleResourceId.Stamina;

    public PuzzleResourceStamina(string name, string sign) 
        : base(name, sign) {}

    public override bool AddResourceValue(int value, 
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl)
    {
        if (value == 0) return false;
		puzzleStatusGirl.stamina += value;
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

        return args.OrderedSlots.Count;
    }
}