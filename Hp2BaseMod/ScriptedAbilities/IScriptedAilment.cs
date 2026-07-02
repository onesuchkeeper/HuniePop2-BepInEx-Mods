namespace Hp2BaseMod;

/// <summary>
/// Defines code-driven behaviour for a scripted ailment instance.
/// Obtained via <see cref="ExpandedAilment.ScriptedAilment"/>.
///
/// All methods are called AFTER the data-driven pipeline has already run,
/// so data-driven effects (enableType, triggers, etc.) remain fully active.
///
/// Use <see cref="Ailment.flags"/> on the provided ailment for persistent runtime state.
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

    /// <summary>
    /// Called once per PuzzleMatch inside GetMatchRewards, before the affection/resource
    /// formula runs. Use this to modify context.AltGirl, set context.CancelRewards,
    /// or prepare state for OnPostMatchReward.
    ///
    /// The MatchModifier has already been applied to context.Match.tokenDefinition
    /// and context.AltGirl at this point.
    /// </summary>
    void OnPreMatchReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleRewardContext context);

    /// <summary>
    /// Called once per PuzzleMatch inside GetMatchRewards, after the formula runs
    /// and after context.RewardBonus has been applied.
    /// The reward dictionary is fully populated for this match at this point.
    /// Add entries to context.AdditionalRewards to inject extra resource grants.
    /// </summary>
    void OnPostMatchReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleRewardContext context,
        System.Collections.Generic.Dictionary<UiPuzzleSlot, PuzzleReward> rewards);

    /// <summary>
    /// Called once per ConsumePuzzleSet after all per-match rewards have been calculated
    /// and all OnPostMatchReward callbacks have run across the full set.
    /// The full reward dictionary is available on context.Rewards for final modification.
    /// Additional rewards from all matches are available on context.AdditionalRewards.
    /// Set context.CancelConsume to suppress all reward application and token destruction.
    /// </summary>
    void OnPostSetReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleConsumeContext context);
}