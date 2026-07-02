using Hp2BaseMod;
using System.Collections.Generic;

namespace HuniePopUltimate;

public class RomancePoisonAilment : IScriptedAilment
{
    public void OnEnable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl) { }
    public void OnDisable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl) { }

    public bool OnTrigger(
        AilmentTriggerType triggerType,
        Ailment ailment,
        PuzzleStatusGirl girl,
        bool unfocused,
        MoveModifier moveModifier,
        MatchModifier matchModifier,
        GiftModifier giftModifier) => false;

    public void OnPreMatchReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleRewardContext context)
    {
        // Only active when this girl receives the reward.
        if (context.AltGirl != girl.altGirl)
        {
            return;
        }

        // Only romance token matches.
        if (context.Match.tokenDefinition.resourceType != PuzzleResourceType.AFFECTION
            || context.Match.tokenDefinition.affectionType != PuzzleAffectionType.ROMANCE)
        {
            return;
        }

        ModInterface.Log.Message("Romance match made least fav");
        // Mark this match as least-fav. The formula will apply the 1x multiplier.
        context.Properties.IsLeastFav = true;
    }

    public void OnPostMatchReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleRewardContext context,
        Dictionary<UiPuzzleSlot, PuzzleReward> rewards) { }

    public void OnPostSetReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleConsumeContext context) { }
}