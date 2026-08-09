// using System.Collections.Generic;

// namespace Hp2BaseMod;

// /// <summary>
// /// A session-level modifier applied to a <see cref="UiPuzzleGrid"/> for the
// /// duration of a puzzle. Unlike <see cref="IScriptedAilment"/>, modifiers have
// /// no game data representation, do not appear in the ailment UI, and are not
// /// subject to ailment filtering or enable/disable lifecycle.
// ///
// /// Modifiers are registered on <see cref="ExpandedUiPuzzleGrid"/> before
// /// <see cref="UiPuzzleGrid.StartPuzzle"/> runs, applied automatically in the
// /// StartPuzzle postfix, and removed automatically in the EndPuzzle postfix.
// ///
// /// All modifiers receive every notification and veto/modify as needed and ignore
// /// the rest.
// /// </summary>
// public interface IPuzzleGridModifier
// {
//     /// <summary>
//     /// Called once when the puzzle starts, after ailments have been enabled.
//     /// Apply flags, subscribe to events, and perform any setup here.
//     /// </summary>
//     void OnApply(UiPuzzleGrid grid, PuzzleStatus status);

//     /// <summary>
//     /// Called once when the puzzle ends, before the grid is torn down.
//     /// Reverse everything applied in OnApply.
//     /// </summary>
//     void OnRemove(UiPuzzleGrid grid, PuzzleStatus status);

//     /// <summary>
//     /// Called before an ailment is enabled, whether at puzzle start or mid-puzzle.
//     /// Return false to prevent the ailment from being enabled.
//     /// All modifiers are checked, if any returns false the ailment is blocked.
//     /// </summary>
//     bool CanEnableAilment(Ailment ailment, PuzzleStatusGirl girl, PuzzleStatusGirl otherGirl);

//     /// <summary>
//     /// Called for every ailment trigger event, mirroring <see cref="IScriptedAilment.OnTrigger"/>.
//     /// Modifier objects are non-null only for their respective trigger types:
//     /// <paramref name="move"/> and <paramref name="moveModifier"/> for PRE_MOVE,
//     /// <paramref name="match"/> and <paramref name="matchModifier"/> for PRE_MATCH.
//     /// <paramref name="triggerType"/> may be any existing <see cref="AilmentTriggerType"/>
//     /// or a mod-defined extension value.
//     /// </summary>
//     void OnTrigger(
//         RelativeId triggerId,
//         PuzzleSet move,
//         MoveModifier moveModifier,
//         PuzzleMatch match,
//         MatchModifier matchModifier,
//         PuzzleStatus status);

//     /// <summary>
//     /// Called once per <see cref="PuzzleMatch"/> before the reward formula runs.
//     /// Modify <see cref="PuzzleRewardContext.Properties"/> to influence formula output.
//     /// </summary>
//     void OnPreMatchReward(PuzzleRewardContext context, PuzzleStatus status);

//     /// <summary>
//     /// Called once per <see cref="PuzzleMatch"/> after the reward formula runs.
//     /// Modify reward values or add entries to
//     /// <see cref="PuzzleRewardContext.AdditionalRewards"/>.
//     /// </summary>
//     void OnPostMatchReward(
//         PuzzleRewardContext context,
//         Dictionary<UiPuzzleSlot, PuzzleReward> rewards,
//         PuzzleStatus status);

//     /// <summary>
//     /// Called once per <see cref="PuzzleSet"/> after all per-match rewards have
//     /// been calculated. Modify <see cref="PuzzleConsumeContext.Rewards"/> or set
//     /// <see cref="PuzzleConsumeContext.CancelConsume"/> here.
//     /// </summary>
//     void OnPostSetReward(PuzzleConsumeContext context, PuzzleStatus status);

//     /// <summary>
//     /// Called when focus switching is attempted.
//     /// Return false to veto the switch. The <see cref="ExpandedUiPuzzleGrid"/>
//     /// suppression counter is checked separately. Both must pass for the switch
//     /// to proceed.
//     /// </summary>
//     bool OnAttemptFocusSwitch(PuzzleStatus status);

//     /// <summary>
//     /// Called when a new round starts (including the first round).
//     /// </summary>
//     void OnRoundStart(PuzzleStatus status);

//     /// <summary>
//     /// Called after the round ends and before results are committed.
//     /// Modify <paramref name="context"/> to change whether the round is a
//     /// success, failure, or game over. All modifiers receive the same instance
//     /// and see each other's modifications in registration order.
//     /// </summary>
//     void OnRoundEnd(PuzzleRoundContext context, PuzzleStatus status);

//     /// <summary>
//     /// Called after any resource value changes on either girl.
//     /// </summary>
//     void OnResourceChanged(PuzzleStatus status);

//     /// <summary>
//     /// Called after tokens settle following a move.
//     /// </summary>
//     void OnSettled(PuzzleStatus status);
// }