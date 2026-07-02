namespace Hp2BaseMod;

/// <summary>
/// A session-level modifier applied to a <see cref="UiPuzzleGrid"/> for the duration
/// of a puzzle. Unlike <see cref="IScriptedAilment"/>, modifiers have no game data
/// representation, do not appear in the ailment UI, and are not subject to ailment
/// filtering or enable/disable lifecycle.
///
/// Modifiers are registered on <see cref="ExpandedUiPuzzleGrid"/> before
/// <see cref="UiPuzzleGrid.StartPuzzle"/> runs, applied automatically in the
/// StartPuzzle postfix, and removed automatically in the EndPuzzle postfix.
/// </summary>
public interface IUiPuzzleGridModifier
{
    /// <summary>
    /// Called once when the puzzle starts, after ailments have been enabled.
    /// Apply flags, subscribe to events, and perform any setup here.
    /// </summary>
    void OnApply(UiPuzzleGrid grid, ExpandedUiPuzzleGrid expanded, PuzzleStatus status);

    /// <summary>
    /// Called once when the puzzle ends, before the grid is torn down.
    /// Reverse everything applied in OnApply.
    /// </summary>
    void OnRemove(UiPuzzleGrid grid, ExpandedUiPuzzleGrid expanded, PuzzleStatus status);
}