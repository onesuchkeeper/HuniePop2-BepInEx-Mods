// Hp2BaseMod 2026, By OneSuchKeeper

namespace Hp2BaseMod;

public class TokenMovesHandler : BaseTokenHandler
{
    public override bool BypassesMovesCost(
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl, 
        TokenDefinition tokenDef)
    {
        return puzzleStatusGirl.forceMoveCost <= 0 || base.BypassesMovesCost(puzzleStatus, puzzleStatusGirl, tokenDef);
    }
}