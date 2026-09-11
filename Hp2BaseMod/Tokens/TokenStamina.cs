using UnityEngine;

namespace Hp2BaseMod;

    public class TokenStamina : BaseToken
    {
        public override bool BypassesStaminaCost(
            ExpandedPuzzleStatus puzzleStatus, 
            PuzzleStatusGirl puzzleStatusGirl, 
            TokenDefinition tokenDef) => true;
    }