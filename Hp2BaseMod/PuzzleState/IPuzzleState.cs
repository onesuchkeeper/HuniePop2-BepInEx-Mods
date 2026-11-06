// Hp2BaseMod 2026, By OneSuchKeeper

using System;
using System.Collections.Generic;

namespace Hp2BaseMod;

/// <summary>
/// The behavior of <see cref="GameStatePuzzle"/> that differs between kinds of puzzle (normal date, tutorial, boss date, non-stop...).
/// GameStatePuzzle owns what every puzzle shares (event subscriptions, ailments activation, finishing the arrival, leaving the state)
/// and picks one implementation per arrival; anything that is unique to a kind of puzzle belongs in its implementation, and
/// nothing outside of it should ask what kind of puzzle it is. Derive from <see cref="PuzzleStateBase"/>, which has the rules of a normal date.
/// </summary>
public interface IFullPuzzleState : IPuzzleState, IDisposable
{
    /// <summary>
    /// Builds <c>Game.Session.Puzzle.puzzleStatus</c> for this kind of puzzle. Called at the start of the arrival, before the dolls
    /// are reset and before arrive bundles run.
    /// </summary>
    void OnLocationArrived(LocationArriveArgs args);

    /// <summary>
    /// Chooses the intro cutscene when no bundle has queued one (<paramref name="args"/>.ArrivalCutscene is null).
    /// </summary>
    void OnArrivalCompleted(ArrivalCompletedArgs args);

    /// <summary>
    /// The arrival cutscene (if any) is finished and the location has settled: start the puzzle.
    /// </summary>
    void OnLocationSettled(LocationSettledArgs args);

    /// <summary>
    /// Lets this kind of puzzle take over doll styling. Return true when handled, which skips the generic pair based styling.
    /// </summary>
    bool TryResolveDollStyles(ResolveDollStylesArgs args);
}

/// <summary>
/// Which icon the cellphone header shows next to its label.
/// </summary>
public enum PuzzleHeaderIcon { Daytime, Alpha, Round }

/// <summary>
/// What the cellphone header shows in place of the time of day while a puzzle is running.
/// </summary>
public class PuzzleHeaderLabel(PuzzleHeaderIcon icon, string text)
{
    public PuzzleHeaderIcon Icon = icon;
    public string Text = text;
}

/// <summary>
/// The numbers a <see cref="PuzzleStatus"/> is built from, which differ between kinds of puzzle. <see cref="ExpandedPuzzleStatus"/> asks the
/// current puzzle state for these instead of switching on a kind of puzzle.
/// </summary>
public interface IPuzzleStatusRules
{
    /// <summary>The moves the puzzle starts with.</summary>
    int GetInitialMoves(SettingDifficulty difficulty);

    /// <summary>The most moves the puzzle can hold.</summary>
    int GetMaxMoves(SettingDifficulty difficulty);

    /// <summary>The least moves the puzzle can hold. Moves cannot run out below this.</summary>
    int MinMovesRemaining { get; }

    /// <summary>Whether a girl's frozen stamina is used instead of the default when the puzzle starts.</summary>
    bool ChecksStaminaFreezeOnStart { get; }

    /// <summary>Whether each girl's ailments are picked at random from what she has learned, instead of her active one.</summary>
    bool PopulatesRandomAilments { get; }

    /// <summary>Whether the round that is starting is a bonus round. <paramref name="girlsRemaining"/> is how many girls are still waiting to play.</summary>
    bool IsBonusRound(int roundIndex, int girlsRemaining);

    /// <summary>Fills <paramref name="roundGirls"/> with the two girls that play the round, taking them out of or cycling them through <paramref name="girlList"/>.</summary>
    void TakeRoundGirls(List<GirlDefinition> girlList, GirlDefinition[] roundGirls);

    /// <summary>The affection to reach to win the round, and how fast a bonus round drains (-1 when it does not).</summary>
    void GetRoundGoal(int roundIndex, bool bonusRound, out int affectionGoal, out float bonusDrainDelay);

    /// <summary>
    /// For <c>LogicConditionType.DATE_TYPE</c>, which stores a <see cref="PuzzleStatusType"/> in the asset. This is all the enum is still used for.
    /// </summary>
    PuzzleStatusType ConditionDateType { get; }

    /// <summary>
    /// Locks which girl is focused for the whole puzzle: <c>false</c> for the left girl, <c>true</c> for the right girl
    /// (the "alt" girl). <c>null</c> leaves the focus free, which is how a normal date works.
    /// </summary>
    /// <remarks>
    /// When this has a value no source can change the focus: the focus button, the grid after a match, ailments and abilities
    /// that switch focus, gifts, talking, and the stamina based pick. The puzzle status enforces it at the three places focus is
    /// ever written, so a state only has to return the value.
    /// <list type="bullet">
    /// <item><c>PuzzleStatus.NextRound</c> picks the locked girl instead of a random one when a round brings new girls.</item>
    /// <item><c>PuzzleStatus.SetGirlFocus</c> is held to it: a request for the other girl is changed to the locked girl rather than
    /// dropped, so whatever asked still finishes cleanly.</item>
    /// <item><c>PuzzleStatus.SetGirlFocusByStamina</c> is followed by putting the focus back on the locked girl. The base game's
    /// version writes the focus directly and never goes through <c>SetGirlFocus</c>.</item>
    /// <item><c>UiPuzzleGrid.AttemptGirlFocusSwitch</c> refuses outright, so ailments are not even asked.</item>
    /// </list>
    /// The lock overrides <see cref="IPuzzleState.SwitchesFocusInBonusRound"/>. A locked puzzle should also answer false to
    /// <see cref="IGameState.DollFocusIsInteractive"/> (a <see cref="GameStatePuzzle"/> does that for the state it holds).
    /// <para>Read whenever the status is built or focus is touched, from the rules the status was reset with, so it is also
    /// asked of the rules a sim uses to build the status before a date: the locked girl is already focused when the date starts.</para>
    /// </remarks>
    bool? LockedAltGirlFocus { get; }

    /// <summary>
    /// For <c>LogicConditionType.IS_TUTORIAL</c>: whether this is the tutorial date. With <paramref name="requireScripted"/>
    /// it is only true while the tutorial's script is running (the player has not skipped it).
    /// </summary>
    bool MatchesTutorialCondition(bool requireScripted);
}

/// <summary>
/// The rules of play and the lifecycle of a puzzle, as asked by the base game's puzzle code. A game state that implements this
/// (<see cref="GameStatePuzzle"/> does, by forwarding to the <see cref="IFullPuzzleState"/> it holds) is how the rest of the game
/// knows a puzzle is the thing being played: the puzzle grid only updates while the current state is an <see cref="IPuzzleState"/>,
/// and the stamina hint shows the move's stamina cost for one (see <see cref="IGameState.HintsSimStaminaCost"/>). Nothing
/// asks what <i>kind</i> of puzzle it is.
/// </summary>
public interface IPuzzleState : IPuzzleStatusRules
{
    // ---- rules of play ----

    /// <summary>
    /// Whether running out of moves (or both girls being exhausted) fails the round. A puzzle that cannot fail still revives the girls.
    /// </summary>
    bool FailsWhenOutOfMoves { get; }

    /// <summary>
    /// Whether the player may make this move.
    /// </summary>
    bool AllowsMove(UiPuzzleSlot from, UiPuzzleSlot to);

    /// <summary>
    /// Whether the player may give a gift right now.
    /// </summary>
    bool AllowsGift { get; }

    /// <summary>
    /// Whether the player may forfeit the puzzle (PuzzleManager.ForfeitDate and closing the cellphone after forfeiting).
    /// </summary>
    bool AllowsForfeit { get; }

    /// <summary>
    /// Whether the first match of a set can replace its token with an upgraded one (outside of bonus rounds, which never do).
    /// </summary>
    bool CreatesReplacementTokens { get; }

    /// <summary>
    /// Whether the focus switches to the other girl after a match during a bonus round.
    /// </summary>
    /// <remarks>
    /// Ignored when <see cref="IPuzzleStatusRules.LockedAltGirlFocus"/> has a value: a locked focus never switches.
    /// </remarks>
    bool SwitchesFocusInBonusRound { get; }

    /// <summary>
    /// Whether this puzzle is a repeat of a date the player already completed (<c>PuzzleManager.isRepeatDate</c>).
    /// Read once when the puzzle starts.
    /// </summary>
    bool IsRepeatDate { get; }

    /// <summary>
    /// Whether the player may do this kind of action right now (the tutorial only allows what its script is asking for).
    /// </summary>
    bool AllowsAction(PuzzleTutorialStepType action);

    /// <summary>
    /// Whether the player may pick up the token in this slot to start a move.
    /// </summary>
    bool AllowsTokenPickup(UiPuzzleSlot slot);

    /// <summary>
    /// Whether the dolls hold back their dialog lines right now.
    /// </summary>
    bool SuppressesDollDialog { get; }

    // ---- presentation ----

    /// <summary>
    /// The cellphone header's label and icon while this puzzle runs. False keeps the time of day.
    /// </summary>
    bool TryGetHeaderLabel(out PuzzleHeaderLabel label);

    /// <summary>
    /// The photo window has closed over the puzzle: put the dolls back into the mood the puzzle has them in.
    /// </summary>
    void OnPhotoWindowClosed();

    // ---- puzzle lifecycle (called from PuzzleManager, see ExpandedPuzzleManager) ----

    /// <summary>
    /// The puzzle is about to start: PuzzleManager.StartPuzzle has set up its own state and the grid is the next thing to start.
    /// Anything that has to be in place before the first tokens are created goes here.
    /// </summary>
    void OnPuzzleStarting();

    /// <summary>
    /// A round has ended (PuzzleManager.OnRoundOver). Decide the cutscenes, whether the puzzle is over, and apply everything a round
    /// ending of this kind of puzzle has on the save (relationship level ups, progress, records, achievements).
    /// </summary>
    void OnRoundOver(PuzzleRoundOverContext context);

    /// <summary>
    /// The round over cutscene is finished and another round follows: <c>PuzzleStatus.NextRound</c> has run, and the
    /// ailments, gifts and focus have been refreshed if the new round is not a bonus round. Do whatever else this kind of puzzle
    /// needs for its next round. The dolls are reset after this when the new round is not a bonus round.
    /// </summary>
    void OnNextRound();

    /// <summary>
    /// The grid changed state (PuzzleGrid.StateChangedEvent).
    /// </summary>
    void OnPuzzleStateChanged();

    /// <summary>
    /// The puzzle has ended (the base game's PuzzleManager.EndPuzzle bookkeeping is done). Take the player where this kind of
    /// puzzle leads (<c>Game.Session.Location.Depart</c>, or opening the cellphone), and queue the state that destination needs
    /// (<see cref="GameStateLocation.QueueTransition"/>) so it is applied before the arrival.
    /// <paramref name="succeeded"/> is whether the final round was won.
    /// </summary>
    void OnPuzzleEnded(bool succeeded);

    // ---- rewards after the puzzle ----

    /// <summary>
    /// Whether the styles the girls wear are unlocked once the rewards are given (and the puzzle did not fail).
    /// </summary>
    bool UnlocksStylesAfterRewards { get; }

    /// <summary>
    /// Whether the styles the girls wear are unlocked each time a round is cleared (<c>CutsceneStepSpecialRoundClear</c>).
    /// </summary>
    bool UnlocksStylesAfterRoundClear { get; }

    /// <summary>
    /// How many fruit the post rewards cutscene step hands out (two per reward). <paramref name="baseFruitCount"/> is the step's own
    /// default. <paramref name="failed"/> is set when the puzzle ended in failure: the step then unlocks no styles and does not cheer.
    /// Called once, before <see cref="GetPostRewardFruits"/>.
    /// </summary>
    int GetPostRewardFruitCount(int baseFruitCount, out bool failed);

    /// <summary>
    /// The fruit for one reward (rewardIndex counts from 0): one for the focused girl and one for the other. Either can be null for none.
    /// </summary>
    void GetPostRewardFruits(int rewardIndex, out ItemDefinition forFocused, out ItemDefinition forUnfocused);
}