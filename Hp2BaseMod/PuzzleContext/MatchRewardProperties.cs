using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// Semantic properties of a match used during reward calculation.
/// Populated before the formula runs and available to all ailments via
/// <see cref="PuzzleRewardContext.Properties"/> during <see cref="IScriptedAilment.OnPreMatchReward"/>.
///
/// Ailments may modify these properties during OnPreMatchReward to influence
/// how the formula treats this match. The formula reads from this object
/// rather than computing properties inline, so changes here are reflected
/// in the final reward.
///
/// Use <see cref="AdditionalProperties"/> with <see cref="RelativeId"/> keys
/// to communicate custom semantic flags between ailments without coupling them.
/// </summary>
public class MatchRewardProperties
{
    /// <summary>
    /// True if this match's affection type is the focused girl's most favourite.
    /// Results in a 3x affection multiplier in the formula.
    /// </summary>
    public bool IsMostFav;

    /// <summary>
    /// True if this match's affection type is the focused girl's least favourite.
    /// Results in a 1x affection multiplier in the formula (no bonus).
    /// </summary>
    public bool IsLeastFav;

    /// <summary>
    /// True if the girl is a boss character. Suppresses most/least fav multipliers
    /// regardless of IsMostFav/IsLeastFav.
    /// </summary>
    public bool IsBossCharacter;

    /// <summary>
    /// True if the match was created as a flat match (no length scaling).
    /// </summary>
    public bool FlatMatch;

    /// <summary>
    /// When true, IsMostFav will not apply its 3x multiplier even if set.
    /// Mirrors MatchModifier.skipMostFavFactor.
    /// </summary>
    public bool SkipMostFavFactor;

    /// <summary>
    /// When true, IsLeastFav will not suppress the standard 2x multiplier.
    /// Mirrors MatchModifier.skipLeastFavFactor.
    /// </summary>
    public bool SkipLeastFavFactor;

    /// <summary>
    /// Arbitrary named properties for inter-ailment communication.
    /// Use a RelativeId scoped to your mod as the key to avoid conflicts.
    ///
    /// Example — setting a property:
    ///   context.Properties.AdditionalProperties[new RelativeId(myPluginId, 0)] = true;
    ///
    /// Example — reading a property set by another ailment:
    ///   if (context.Properties.AdditionalProperties.TryGetValue(knownId, out var val)
    ///       &amp;&amp; val is bool b &amp;&amp; b) { ... }
    /// </summary>
    public Dictionary<RelativeId, object> AdditionalProperties
        = new Dictionary<RelativeId, object>();
}