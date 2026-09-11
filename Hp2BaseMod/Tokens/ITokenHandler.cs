using System;
using UnityEngine;

namespace Hp2BaseMod
{
    public interface ITokenHandler
    {
        bool BypassesStaminaCost(
            ExpandedPuzzleStatus puzzleStatus, 
            PuzzleStatusGirl puzzleStatusGirl, 
            TokenDefinition tokenDef);

        bool BypassesMovesCost(
            ExpandedPuzzleStatus puzzleStatus, 
            PuzzleStatusGirl puzzleStatusGirl, 
            TokenDefinition tokenDef);

        int GetStaminaCost(PuzzleMatch match);

        int GetMovesCost(PuzzleMatch match);
        bool CanSpawn(PuzzleStatus puzzleStatus);
    }
}
