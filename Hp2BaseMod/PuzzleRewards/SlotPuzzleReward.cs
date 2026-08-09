using System.Collections.Generic;

namespace Hp2BaseMod;

public class SlotPuzzleReward : ISlotReward
{
    public TokenDefinition ReplaceDefinition { get; set; }

    public bool ReplaceUpgraded { get; set; }

    public List<IPuzzleReward> Rewards => _rewards;
    private readonly List<IPuzzleReward> _rewards = new();
}
