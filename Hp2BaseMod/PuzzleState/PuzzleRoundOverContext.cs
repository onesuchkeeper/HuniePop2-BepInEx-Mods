// Hp2BaseMod 2026, By OneSuchKeeper

namespace Hp2BaseMod;

/// <summary>
/// What <see cref="IPuzzleState.OnRoundOver"/> decides for the round that just ended. It starts out as the base game's
/// starting point for every kind of puzzle: the failure cutscene, no new round cutscene, and the game being over.
/// </summary>
public sealed class PuzzleRoundOverContext(bool roundSucceeded, CutsceneDefinition failureCutscene)
{
    /// <summary>
    /// Whether the round was won. A state (or a mod through <see cref="ModEvents.PuzzleRoundOver"/>) may change it.
    /// </summary>
    public bool RoundSucceeded { get; set; } = roundSucceeded;

    /// <summary>
    /// The cutscene played right now, before anything else happens.
    /// </summary>
    public CutsceneDefinition RoundOverCutscene { get; set; } = failureCutscene;

    /// <summary>
    /// The cutscene played when another round follows. Only used when <see cref="GameOver"/> is false.
    /// </summary>
    public CutsceneDefinition NewRoundCutscene { get; set; }

    /// <summary>
    /// True ends the puzzle after the round over cutscene. False plays <see cref="NewRoundCutscene"/> and starts the next round.
    /// </summary>
    public bool GameOver { get; set; } = true;
}