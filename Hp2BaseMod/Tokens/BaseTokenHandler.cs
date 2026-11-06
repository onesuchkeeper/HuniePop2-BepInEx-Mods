// Hp2BaseMod 2026, By OneSuchKeeper

using UnityEngine;

namespace Hp2BaseMod;

public class BaseTokenHandler : ITokenHandler
{
    public virtual bool BypassesStaminaCost(
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl, 
        TokenDefinition tokenDef)
    {
        return puzzleStatusGirl.noStaminaCostTokenDefs.Contains(tokenDef);
    }

    public virtual bool BypassesMovesCost(
        ExpandedPuzzleStatus puzzleStatus, 
        PuzzleStatusGirl puzzleStatusGirl, 
        TokenDefinition tokenDef)
    {
        return puzzleStatusGirl.noMoveCostTokenDefs.Contains(tokenDef);
    }

    public virtual int GetStaminaCost(PuzzleMatch match) => Mathf.Max(match.slots.Count - 2, 1);

    public virtual int GetMovesCost(PuzzleMatch match) => 1;

    public virtual bool CanSpawn(PuzzleStatus puzzleStatus) => true;
}