using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// Carries per-match reward calculation state through the scripted ailment pipeline.
/// Created once per PuzzleMatch inside the GetMatchRewards replacement, passed through
/// OnPreMatchReward (annotation), the formula, and OnPostMatchReward (modification).
///
/// Stack-lifetime: created and consumed within a single GetMatchRewards call.
/// Do not store references to this object beyond the scope of a reward callback.
/// </summary>
public class PuzzleRewardContext
{
    /// <summary>
    /// The match being processed. tokenDefinition reflects any MatchModifier substitution
    /// already applied — this is the effective token, not necessarily the original.
    /// </summary>
    public PuzzleMatch Match { get; }

    /// <summary>
    /// Which girl receives the rewards. Reflects MatchModifier.absorb routing.
    /// May be modified in OnPreMatchReward to redirect rewards to the other girl.
    /// </summary>
    public bool AltGirl { get; set; }

    /// <summary>
    /// The MatchModifier produced by Game.Session.Ailment.Trigger(match).
    /// Token substitution and absorb routing have already been applied to
    /// Match.tokenDefinition and AltGirl. Available for inspection.
    /// </summary>
    public MatchModifier MatchModifier { get; }

    /// <summary>
    /// Semantic properties of this match used by the reward formula.
    /// Populated before OnPreMatchReward is dispatched.
    /// Ailments may modify these during OnPreMatchReward to influence formula output.
    /// </summary>
    public MatchRewardProperties Properties { get; } = new MatchRewardProperties();

    /// <summary>
    /// The ordered, shuffled list of slots participating in this match.
    /// Matches the slot ordering used for reward distribution — same order
    /// the formula iterates when assigning per-slot reward values.
    /// Read-only after OnPreMatchReward.
    /// </summary>
    public System.Collections.Generic.List<UiPuzzleSlot> OrderedSlots { get; }

    /// <summary>
    /// When true, all reward values are zeroed before distribution.
    /// The match still costs a move and stamina.
    /// </summary>
    public bool CancelRewards { get; set; }

    /// <summary>
    /// Flat bonus applied to the final calculated reward total before slot distribution,
    /// after pointsOp and pointsOp2. Negative values reduce the reward.
    /// Set during OnPostMatchReward.
    /// </summary>
    public int RewardBonus { get; set; }

    /// <summary>
    /// Additional PuzzleRewards to inject alongside the standard per-slot rewards.
    /// Use this to grant resources of a different type than the match token.
    /// Added by ailments during OnPostMatchReward, applied by ExpandedUiPuzzleGrid
    /// after all matches in the set are processed.
    /// </summary>
    public List<(UiPuzzleSlot slot, PuzzleReward reward)> AdditionalRewards { get; }
        = new List<(UiPuzzleSlot, PuzzleReward)>();

    public PuzzleRewardContext(
        PuzzleMatch match,
        bool altGirl,
        MatchModifier matchModifier,
        System.Collections.Generic.List<UiPuzzleSlot> orderedSlots)
    {
        Match = match;
        AltGirl = altGirl;
        MatchModifier = matchModifier;
        OrderedSlots = orderedSlots;
    }
}