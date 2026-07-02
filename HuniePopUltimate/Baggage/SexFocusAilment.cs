using System.Collections.Generic;
using Hp2BaseMod;

namespace HuniePopUltimate;

/// <summary>
/// When a sexuality token is matched while this girl is focused, focus switching
/// is suppressed until a non-sexuality match is made (any resource type other than
/// sexuality affection) or the girl becomes exhausted or upset.
/// </summary>
public class SexualityFocusLockAilment : IScriptedAilment
{
    public void OnEnable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
    {
        ExpandedUiPuzzleGrid.Get().SuppressFocusSwitch = false;
    }

    public void OnDisable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
    {
        ExpandedUiPuzzleGrid.Get().SuppressFocusSwitch = false;
    }

    public bool OnTrigger(
        AilmentTriggerType triggerType,
        Ailment ailment,
        PuzzleStatusGirl girl,
        bool unfocused,
        MoveModifier moveModifier,
        MatchModifier matchModifier,
        GiftModifier giftModifier)
    {
        if (triggerType != AilmentTriggerType.ON_RESOURCE_CHANGED)
        {
            return false;
        }

        // Re-enable focus switching if the girl is now exhausted or upset.
        if (girl.exhausted)
        {
            ExpandedUiPuzzleGrid.Get().SuppressFocusSwitch = false;
        }

        return false;
    }

    public void OnPreMatchReward(
        Ailment ailment,
        PuzzleStatusGirl girl,
        PuzzleRewardContext context) { }

    public void OnPostMatchReward(
        Ailment ailment,
        PuzzleStatusGirl girl,
        PuzzleRewardContext context,
        Dictionary<UiPuzzleSlot, PuzzleReward> rewards)
    {
        // Only react to matches received by this girl while she is focused.
        if (context.AltGirl != girl.altGirl) return;

        var grid = ExpandedUiPuzzleGrid.Get();
        if (grid == null) ModInterface.Log.Warning($"Failed to get {nameof(ExpandedUiPuzzleGrid)}");

        bool isSexuality = context.Match.tokenDefinition.resourceType == PuzzleResourceType.AFFECTION
            && context.Match.tokenDefinition.affectionType == PuzzleAffectionType.SEXUALITY;

        if (isSexuality)
        {
            // Sexuality match while focused, lock focus switching.
            grid.SuppressFocusSwitch = true;
        }
        else
        {
            // Any other token type, release the lock.
            grid.SuppressFocusSwitch = false;
        }
    }

    public void OnPostSetReward(
        Ailment ailment,
        PuzzleStatusGirl girl,
        PuzzleConsumeContext context) { }
}