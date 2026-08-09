using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// 50% affection until given a gift
/// </summary>
public class MaterialisticAilment : IScriptedAilment
{
    private PuzzleStatusGirl _owner;
    //handling given gift here instead of using PuzzleStatusGirl.GivenGift() so we can support
    //it as a global modifier
    private bool _givenGift = false;
    private ExpandedAilmentManager _ailmentManager;

    public void Enable(ExpandedAilmentManager ailmentManager, object owner = null)
    {
        _owner = owner as PuzzleStatusGirl;
        _ailmentManager = ailmentManager;
        _ailmentManager.PostGift += On_AilmentManager_PostGift;
        _ailmentManager.PostMatchReward += On_AilmentManager_PostMatchReward;
    }

    public void Disable()
    {
        _ailmentManager.PostGift -= On_AilmentManager_PostGift;
        _ailmentManager.PostMatchReward -= On_AilmentManager_PostMatchReward;
    }

    private void On_AilmentManager_PostGift(AilmentTriggerArgs.PostGift args)
    {
        if (_owner != null && _owner != args.StatusGirl) return;
        _givenGift = true;
    }

    private void On_AilmentManager_PostMatchReward(AilmentTriggerArgs.PostMatchReward args)
    {
        if (_givenGift || (_owner != null && _owner != args.StatusGirl)) return;

        foreach (var entry in args.Rewards)
        {
            if (entry.Reward.TokenDefinition.Id == PuzzleResourceId.Passion)
            {
                entry.Reward.ResourceValue /=2;
            }
        }
    }
}
