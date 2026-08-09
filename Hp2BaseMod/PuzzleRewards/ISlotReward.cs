using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// All rewards for a single slot
/// </summary>
public interface ISlotReward
{
    /// <summary>
    /// All rewards applied at the slot
    /// </summary>
    List<IPuzzleReward> Rewards { get; }

    /// <summary>
    /// Definition of token to replace with
    /// </summary>
    TokenDefinition ReplaceDefinition { get; set; }

    /// <summary>
    /// If the replaced token is upgraded
    /// </summary>
    bool ReplaceUpgraded { get; set; }
}