namespace Hp2BaseMod;

/// <summary>
/// Defines code-driven behaviour for a scripted ability instance.
/// Obtained via <see cref="ExpandedAbility.ScriptedAbility"/>.
/// </summary>
public interface IScriptedAbility
{
    /// <summary>
    /// Called before any steps execute (data-driven or replaced).
    /// Return false to abort the entire ability immediately.
    /// Return true to continue to the next stage.
    /// </summary>
    bool PrePerform(Ability ability, bool altGirl);

    /// <summary>
    /// When non-null, replaces the data-driven step pipeline entirely.
    /// Return true if the ability succeeded, false if it failed.
    /// Leave this returning null to run the original pipeline.
    /// </summary>
    bool? ReplacePerform(Ability ability, bool altGirl);

    /// <summary>
    /// Called after the pipeline completes (whether original or replaced).
    /// Receives the result of the pipeline and may return a different value.
    /// Use this to apply additional effects on success, clean up on failure,
    /// or force a specific outcome regardless of what the pipeline returned.
    /// </summary>
    bool PostPerform(Ability ability, bool altGirl, bool pipelineResult);
}