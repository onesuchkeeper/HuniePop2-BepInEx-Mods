namespace Hp2BaseMod;

/// <summary>
/// A script driven ailment rather than the data driven ailments in the base game.
/// 
/// Subscribe to events from the <see cref="ExpandedAilmentManager"> to respond to
/// and affect the puzzle grid.
///
/// Scripted ailments do NOT automatically trigger baggage dialog triggers. Call
/// UiDoll.ReadDialogTrigger if you need to
/// </summary>
public interface IScriptedAilment
{
    /// <summary>
    /// Enables the ailment.
    /// </summary>
    /// <param name="ailmentManager">Manager for this ailment. Subscribe to its events to implement behaviour.</param>
    /// <param name="owner">The owner of this ailment if there is one.</param>
    void Enable(ExpandedAilmentManager ailmentManager, object owner = null);

    /// <summary>
    /// Disabled the ailment. Also called at the end of the puzzle before 
    /// its reference is dropped so must be in a disposable state.
    /// </summary>
    void Disable();
}
