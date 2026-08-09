using System.Collections.Generic;

namespace Hp2BaseMod;

public abstract class AilmentTriggerArgs(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
{
    /// <summary>
    /// Status of the girl(s) focused when the girl 
    /// </summary>
    public PuzzleStatusGirl StatusGirl {get;} = statusGirl;
    public ExpandedUiPuzzleGrid UiPuzzleGrid {get;} = uiPuzzleGrid;

    public class PreMatch(PuzzleMatch match, MatchModifier matchModifier, ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl) 
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public bool Canceled = false;
        public PuzzleMatch Match {get;} = match;
        public MatchModifier Modifier {get;} = matchModifier;
    }
    public class PostMatch(MatchModifier matchModifier, ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl) 
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public MatchModifier Modifier {get;} = matchModifier;
    }

    public class PreMove(MoveModifier moveModifier, ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public bool Canceled = false;
        public MoveModifier Modifier {get;} = moveModifier;
    }
    public class PostMove(ExpandedUiPuzzleGrid uiPuzzleGrid, MoveModifier moveModifier, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public MoveModifier Modifier {get;} = moveModifier;
    }

    public class PreGift(GiftModifier giftModifier, ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public GiftModifier GiftModifier {get;} = giftModifier;
    }
    public class PostGift(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
    }

    public class PreExhaust(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {

    }

    public class PostExhaust(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {

    }

    public class PostSettled(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        
    }

    public class PreFocusSwitch(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public bool Canceled = false;
    }

    public class PostFocusSwitch(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {

    }

    public class ResourceChanged(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        
    }

    public class RoundSettled(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        
    }

    public class SettledPure(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        
    }

    public class Exhaustion(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        
    }

    public class PreConsume(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public bool Cancel = false;
    }

    public class PostConsume(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        
    }

    public class RoundStart(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        
    }

    public class PreAilmentEnable(Ailment ailment, ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public bool Cancel = false;
        public Ailment Ailment {get;} = ailment;
    }

    public class PostAilmentEnable(Ailment ailment, ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public Ailment Ailment {get;} = ailment;
    }

    public class PreAilmentDisable(Ailment ailment, ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public bool Cancel = false;
        public Ailment Ailment {get;} = ailment;
    }

    public class PostAilmentDisable(Ailment ailment, ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public Ailment Ailment {get;} = ailment;
    }

    public class PuzzleEnded(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        
    }

    public class CheckRoundOver(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public bool Canceled = false;
        public PuzzleRoundState RoundState;
        public bool RoundOver;
        public bool ReviveGirls;
        public bool CheckChanges;
    }

    public class PreMatchReward(ExpandedUiPuzzleGrid puzzleGrid, 
        PuzzleStatusGirl statusGirl,
        PuzzleMatch match,
        MatchModifier matchModifier, 
        List<UiPuzzleSlot> sortedSlots) 
        : AilmentTriggerArgs(puzzleGrid, statusGirl)
    {
        /// <summary>
        /// The match being processed. tokenDefinition reflects any MatchModifier substitution
        /// already applied — this is the effective token, not necessarily the original.
        /// </summary>
        public PuzzleMatch Match { get; } = match;

        /// <summary>
        /// The MatchModifier produced by Game.Session.Ailment.Trigger(match).
        /// Token substitution and absorb routing have already been applied to
        /// Match.tokenDefinition and AltGirl. Available for inspection.
        /// </summary>
        public MatchModifier MatchModifier { get; } = matchModifier;

        /// <summary>
        /// The ordered, shuffled list of slots participating in this match.
        /// Matches the slot ordering used for reward distribution — same order
        /// the formula iterates when assigning per-slot reward values.
        /// Read-only after OnPreMatchReward.
        /// </summary>
        public List<UiPuzzleSlot> OrderedSlots { get; } = sortedSlots;

        /// <summary>
        /// Slots to exclude from this match entirely: no reward, and the token is not
        /// destroyed by ConsumePuzzleSet. Populate during OnPreMatchReward. Excluded
        /// slots are removed from the working sorted-slot list before the reward formula
        /// runs, and from the owning PuzzleSet.allSlots before token destruction.
        /// </summary>
        public List<UiPuzzleSlot> ExcludedSlots { get; } = new List<UiPuzzleSlot>();
    }

    public class PostMatchReward(ExpandedUiPuzzleGrid uiPuzzleGrid, PuzzleStatusGirl statusGirl, List<(UiPuzzleSlot Slot, IPuzzleReward Reward)> rewards)
        : AilmentTriggerArgs(uiPuzzleGrid, statusGirl)
    {
        public List<(UiPuzzleSlot Slot, IPuzzleReward Reward)> Rewards { get; } = rewards;
    }

    public class PreAffectionMatchReward(ExpandedUiPuzzleGrid puzzleGrid, 
        PuzzleStatusGirl statusGirl,
        PuzzleMatch match,
        MatchModifier matchModifier,
        RelativeId affectionId,
        bool isMostFav,
        bool isLeastFav,
        List<UiPuzzleSlot> sortedSlots) 
        : PreMatchReward(puzzleGrid, statusGirl, match, matchModifier, sortedSlots)
    {
        public bool IsMostFav = isMostFav;
        public bool IsLeastFav = isLeastFav;
        public RelativeId AffectionId = affectionId;
    }
}