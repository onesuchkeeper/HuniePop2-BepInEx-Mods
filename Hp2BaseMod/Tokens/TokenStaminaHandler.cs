// Hp2BaseMod 2026, By OneSuchKeeper

namespace Hp2BaseMod;

public class TokenStaminaHandler : BaseTokenHandler
{
    public override bool BypassesStaminaCost(
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl, 
        TokenDefinition tokenDef) => true;
}