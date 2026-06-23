namespace Hp2BaseMod;

/// <summary>
/// Defines code-driven behaviour for a scripted ailment instance.
/// Obtained via <see cref="ExpandedAilment.ScriptedAilment"/>.
/// </summary>
public interface IScriptedAilment
{
    /// <summary>
    /// Called after AilmentManager.OnAilmentEnable has applied the data-driven enable effect.
    /// </summary>
    void OnEnable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl);

    /// <summary>
    /// Called after AilmentManager.OnAilmentDisable has reversed the data-driven enable effect.
    /// </summary>
    void OnDisable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl);

    /// <summary>
    /// Called after all data-driven AilmentTriggers have been processed for a trigger event.
    /// Return true if the scripted trigger "succeeded" — this controls verbalization.
    ///
    /// Modifier objects are only non-null for their respective trigger types:
    /// moveModifier for PRE_MOVE, matchModifier for PRE_MATCH, giftModifier for PRE_GIFT.
    /// </summary>
    bool OnTrigger(
        AilmentTriggerType triggerType,
        Ailment ailment,
        PuzzleStatusGirl girl,
        bool unfocused,
        MoveModifier moveModifier,
        MatchModifier matchModifier,
        GiftModifier giftModifier);
}