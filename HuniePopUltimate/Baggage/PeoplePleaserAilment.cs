using Hp2BaseMod;
using System;
using System.Collections.Generic;

namespace HuniePopUltimate;

/// <summary>
/// Gives 1/3rd of sentiment, stamina and passion to her date
/// </summary>
public class PeoplePleaserAilment : IScriptedAilment
{
    private PuzzleStatusGirl _owner;
    private ExpandedAilmentManager _ailmentManager;

    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _ailmentManager = ailmentManager;
        _owner = owner as PuzzleStatusGirl
            ?? throw new ArgumentException(nameof(owner));

        _ailmentManager.PostMatchReward += OnPostMatchReward;
    }

    public void Disable()
    {
        _ailmentManager.PostMatchReward -= OnPostMatchReward;
    }

    private void OnPostMatchReward(AilmentTriggerArgs.PostMatchReward args)
    {
        if (args.StatusGirl != _owner) return;
        var otherGirl = args.UiPuzzleGrid._status.GetStatusGirl(!args.StatusGirl.altGirl);

        var newRewards = new List<(UiPuzzleSlot, IPuzzleReward)>();
        foreach (var entry in args.Rewards)
        {
            // Only modify rewards earned by this girl
            if (entry.Reward.Girl != _owner) continue;

            switch (entry.Reward.TokenDefinition.PuzzleResource)
            {
                case PuzzleResourcePassion:
                case PuzzleResourceSentiment:
                case PuzzleResourceStamina:
                    var stolen = entry.Reward.ResourceValue / 3;

                    entry.Reward.ResourceValue -= stolen;

                    newRewards.Add((entry.Slot, new TokenPuzzleReward(entry.Reward.TokenDefinition, stolen, otherGirl)));
                    break;
            }
        }
        args.Rewards.AddRange(newRewards);
    }
}