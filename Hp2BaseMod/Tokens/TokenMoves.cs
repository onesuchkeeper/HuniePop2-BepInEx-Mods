namespace Hp2BaseMod;

public class TokenMoves : BaseToken
{
    public override bool BypassesMovesCost(
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl, 
        TokenDefinition tokenDef)
    {
        return puzzleStatusGirl.forceMoveCost <= 0 || base.BypassesMovesCost(puzzleStatus, puzzleStatusGirl, tokenDef);
    }
}