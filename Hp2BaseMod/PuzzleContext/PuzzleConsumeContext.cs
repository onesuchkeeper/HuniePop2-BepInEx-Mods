using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// Carries set-level reward state through the scripted ailment pipeline.
/// Created in the ConsumePuzzleSet prefix, populated as each match is processed,
/// and passed to OnPostSetReward after all matches in the set have been resolved.
///
/// Stack-lifetime: created and consumed within a single ConsumePuzzleSet call.
/// Do not store references to this object beyond the scope of a reward callback.
/// </summary>
public class PuzzleConsumeContext
{
    /// <summary>
    /// The PuzzleSet being consumed.
    /// </summary>
    public PuzzleSet PuzzleSet { get; }

    /// <summary>
    /// Whether this set is being consumed with token destruction (andDestroy parameter).
    /// </summary>
    public bool AndDestroy { get; }

    /// <summary>
    /// The full reward dictionary after all per-match rewards have been calculated
    /// and all OnPostMatchReward callbacks have run.
    /// Populated during ConsumePuzzleSet, available in OnPostSetReward.
    /// Modifications here directly affect what gets applied to game state.
    /// </summary>
    public Dictionary<UiPuzzleSlot, PuzzleReward> Rewards { get; }
        = new Dictionary<UiPuzzleSlot, PuzzleReward>();

    /// <summary>
    /// Additional rewards accumulated from PuzzleRewardContext.AdditionalRewards
    /// across all matches in the set.
    /// Applied to game state alongside the main reward dictionary.
    /// </summary>
    public List<(UiPuzzleSlot slot, PuzzleReward reward)> AdditionalRewards { get; }
        = new List<(UiPuzzleSlot, PuzzleReward)>();

    /// <summary>
    /// When true, no rewards are applied to game state and no tokens are destroyed.
    /// The move cost has already been paid at this point.
    /// </summary>
    public bool CancelConsume { get; set; }

    public PuzzleConsumeContext(PuzzleSet puzzleSet, bool andDestroy)
    {
        PuzzleSet = puzzleSet;
        AndDestroy = andDestroy;
    }
}