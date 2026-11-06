using Hp2BaseMod;

namespace SingleDate;

/// <summary>
/// The rules of a single date puzzle: <see cref="PuzzleStateNormal"/> with the girl alone on the right, so focus never
/// moves. Used to build the puzzle status in <see cref="SingleDateSim"/> (which has no puzzle state to ask) and as the
/// base of <see cref="SingleDatePuzzleState"/>, so the sim and the date can't disagree about it.
/// </summary>
public class SingleDateRules(IGameStateLocation owner) : PuzzleStateNormal(owner)
{
    /// <summary>
    /// The right girl is always the focused girl and nothing can change that: the focus button, the grid after a match,
    /// ailments, gifts, talking and the stamina based pick are all held to it by the puzzle status.
    /// </summary>
    public override bool? LockedAltGirlFocus => true;
}
