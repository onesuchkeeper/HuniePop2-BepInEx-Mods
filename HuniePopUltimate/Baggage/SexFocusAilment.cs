using System.Collections.Generic;
using Hp2BaseMod;

namespace MyMod;

/// <summary>
// When a sexuality token is matched while this girl is focused, focus switching
// is suppressed until a non-sexuality match is made (any resource type other than
// sexuality affection) or the girl becomes exhausted or upset.
/// </summary>
public class SexualityFocusLockAilment : IScriptedAilment
{
    // Tracks whether this ailment instance is currently holding a suppression count.
    // We only ever hold at most one count at a time.
    private bool _isSuppressing;

    public void OnEnable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
    {
        // Ensure we start clean.
        _isSuppressing = false;
    }

    public void OnDisable(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl)
    {
        ClearSuppression();
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
        if (triggerType != AilmentTriggerType.ON_RESOURCE_CHANGED) return false;
        // Re-enable focus switching if the girl is now exhausted or upset.
        if (girl.exhausted) ClearSuppression();
        return false;
    }

    public void OnPreMatchReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleRewardContext context) { }

    public void OnPostMatchReward(
        Ailment ailment,
        PuzzleStatusGirl girl,
        PuzzleRewardContext context,
        Dictionary<UiPuzzleSlot, PuzzleReward> rewards)
    {
        // Only react to matches received by this girl while she is focused.
        if (context.AltGirl != girl.altGirl) return;

        var grid = GetGrid();
        if (grid == null) return;

        bool isSexuality = context.Match.tokenDefinition.resourceType == PuzzleResourceType.AFFECTION
            && context.Match.tokenDefinition.affectionType == PuzzleAffectionType.SEXUALITY;

        if (isSexuality) 
        {
            SetSuppression(grid);
        } else 
        {
            ClearSuppression();
        }
    }

    public void OnPostSetReward(Ailment ailment, PuzzleStatusGirl girl, PuzzleConsumeContext context) { }

    private void SetSuppression(ExpandedUiPuzzleGrid grid)
    {
        if (_isSuppressing) return;
        _isSuppressing = true;
        grid.SuppressFocusSwitch();
    }

    private void ClearSuppression()
    {
        if (!_isSuppressing) return;
        _isSuppressing = false;
        GetGrid()?.UnsuppressFocusSwitch();
    }

    private static ExpandedUiPuzzleGrid GetGrid()
    {
        var grid = Game.Session.Puzzle?.puzzleGrid;
        return grid != null ? ExpandedUiPuzzleGrid.Get(grid) : null;
    }
}