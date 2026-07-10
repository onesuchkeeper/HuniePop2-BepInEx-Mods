namespace Hp2BaseMod;

/// <summary>
/// Mutable context passed to <see cref="IUiPuzzleGridModifier.OnRoundEnd"/>.
/// All modifiers receive the same instance in registration order and may
/// modify <see cref="IsSuccess"/> and <see cref="IsGameOver"/> before the
/// results are written back to the grid and puzzle manager.
///
/// Mirrors <see cref="PuzzleRoundOverArgs"/> from the existing PuzzleManager
/// patch but scoped to the modifier pipeline.
/// </summary>
public class PuzzleRoundContext
{
    /// <summary>
    /// Whether the round ended in success. Modifiers may change this.
    /// Written back to <see cref="UiPuzzleGrid._roundState"/> after all
    /// modifiers have been dispatched.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Whether the puzzle is now over (no new round follows).
    /// Modifiers may change this, e.g. to prevent game over on failure.
    /// Written back to <see cref="PuzzleStatus.gameOver"/> after dispatch.
    /// </summary>
    public bool IsGameOver { get; set; }

    /// <summary>
    /// Whether this is a bonus round.
    /// Read-only, informational for modifiers.
    /// </summary>
    public bool IsBonusRound { get; }

    /// <summary>
    /// The puzzle status type at round end.
    /// Read-only, informational for modifiers.
    /// </summary>
    public PuzzleStatusType StatusType { get; }

    public PuzzleRoundContext(bool isSuccess, bool isGameOver, bool isBonusRound, PuzzleStatusType statusType)
    {
        IsSuccess = isSuccess;
        IsGameOver = isGameOver;
        IsBonusRound = isBonusRound;
        StatusType = statusType;
    }
}