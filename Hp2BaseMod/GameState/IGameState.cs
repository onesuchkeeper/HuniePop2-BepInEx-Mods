using System;

namespace Hp2BaseMod;

/// <summary>
/// A state of the game: what the player is doing and what the game lets them do while they do it (the title, the hub, a sim,
/// a puzzle...). Exactly one is current at a time (<c>ModInterface.GameState.CurrentState</c>).
/// </summary>
/// <remarks>
/// A state is not a kind of place. The base game's UI used to ask <c>LocationManager.AtLocationType(SIM / DATE / HUB / SPECIAL)</c>
/// what to show, and a state could only take part by pretending to be one of those. The UI now asks the current state directly,
/// one question for each thing it decides (see "what the player can use"), so a new state answers each question for itself
/// and never has to impersonate another.
/// <para>The questions in "what the player can use" are read by the base game's UI at the moments listed on each member. They
/// are read when needed, not pushed: a state that answers differently later is asked again the next time the UI needs it, but
/// should not expect the UI that already read an answer to change its mind.</para>
/// </remarks>
public interface IGameState : IDisposable
{
    // ---- identity ----

    /// <summary>
    /// Unique relative ID identifying this game state.
    /// </summary>
    RelativeId Id { get; }

    /// <summary>
    /// Whether the game can be saved while in this state. When true, <see cref="GeneratePayload"/> must produce a payload that
    /// restores the state.
    /// </summary>
    bool IsSavable { get; }

    // ---- leaving ----

    /// <summary>
    /// The player is leaving this state for another one. Do whatever the state does on the way out (a valediction line,
    /// a cutscene), then call <paramref name="continueTransitionCallback"/> exactly once; the departure waits for it.
    /// Call it straight away if there is nothing to do.
    /// </summary>
    /// <param name="payload">
    /// The state to arrive in, when the caller knows it. Null leaves it to whatever was queued earlier
    /// (<see cref="IGameStateLocation.QueueTransition"/>); a non-null payload replaces what was queued.
    /// </param>
    /// <param name="continueTransitionCallback">Continues the departure. Must be called once.</param>
    void DepartTransition(IGameStatePayload payload, Action continueTransitionCallback);

    /// <summary>
    /// <see cref="DepartTransition(IGameStatePayload, Action)"/> for a departure that doesn't say where it is going, so
    /// whatever was queued earlier is used.
    /// </summary>
    void DepartTransition(Action continueTransitionCallback);

    /// <summary>
    /// Produces a serializable payload that can be saved and used to restore the state.
    /// </summary>
    /// <returns></returns>
    IGameStatePayload GeneratePayload();

    // ---- what the player can use ----
    //
    // Everything below used to be answered from the kind of place the player was at. There is no kind of place, and
    // LocationManager.AtLocationType throws: nothing may ask it. Each member names the base game code that asks it, what true and
    // false do there, and what the base game's own states answer, so a state can copy the answer of the one it is most like.
    //
    // Two things the UI decides aren't members, because the state already says them by being an IPuzzleState, as
    // UiCellphoneAppHeaderPatch and UiCellphoneAppSettingsPatch already treat it: whether a puzzle is being played (the puzzle
    // grid, the status app's layout, the stamina hint's puzzle branch) and whether a relationship slot waits for the puzzle to start.
    //
    // Date greeting lines aren't a question either: ExpandedUiDoll.ReadDialogTrigger picks them by the location's id.

    // -- gifts --

    /// <summary>
    /// Whether gifts can be given right now. This is the yes/no: the gift manager refuses to give when it is false, and the
    /// gifts app dims the girls' heads (its "can't give" look) when it is false.
    /// </summary>
    /// <remarks>
    /// Keep it consistent with <see cref="GetGiftError"/>: when this is false, <see cref="GetGiftError"/> should say why.
    /// Base states: true at a sim, at the hub and at special locations (there, <see cref="GetGiftError"/> is what says there is nobody to give to), the puzzle state's <c>AllowsGift</c> at a date, false at the title.
    /// </remarks>
    bool AllowGifts { get; }

    /// <summary>
    /// The reason an item can't be given as a gift right now, shown where the player drags an item over a girl; null when it
    /// can be given. Only called for items that are not misc items or date gifts: those two have fixed reasons that are
    /// the same in every state and are checked first.
    /// </summary>
    /// <remarks>
    /// Asked by <c>GiftManager.GetDirectGiftGiveError</c>. This is text only; what actually stops the gift is
    /// <see cref="AllowGifts"/>. Base states: at a sim, "There are no girls nearby to give gifts to." when there is no pair,
    /// "You can't give gifts while you're talking." while the action bubbles are not showing, else null. At a date,
    /// "Only date gifts can be given during a date.". At the hub, at special locations and at the title,
    /// "There are no girls nearby to give gifts to.".
    /// </remarks>
    string GetGiftError(ItemDefinition item);

    // -- the cellphone --

    /// <summary>
    /// Whether the cellphone is kept off screen while it is closed. When true the phone's closed position is its hidden one
    /// instead of its resting one, and it doesn't load its closed app.
    /// </summary>
    /// <param name="cellphone">The phone asking. The hub answers with its <c>hideOnHub</c> setting.</param>
    /// <remarks>
    /// Asked when the phone closes (<c>UiCellphone.Close</c>) and when it loads an app (<c>UiCellphone.LoadApp</c>).
    /// Base states: <c>cellphone.hideOnHub</c> at the hub, true at special locations, false at a sim and at a date.
    /// </remarks>
    bool HidesCellphone(UiCellphone cellphone);

    /// <summary>
    /// What the finder app shows: whether it is there at all, whether its slots can be picked, and the message for why not.
    /// </summary>
    /// <remarks>
    /// Asked once when the finder app starts, so it is how things are when the app opens. The app itself turns this into
    /// <see cref="FinderStatus.Unavailable"/> before the tutorial is over, so this doesn't need to. Slots that aren't ready are
    /// shown but can't be used, with the app's message number <see cref="FinderStatus.MessageIndex"/> displayed.
    /// <para>Base states: at a sim, present and ready once the action bubbles are showing
    /// (<c>Game.Manager.Windows.IsWindowActive(Game.Session.Location.actionBubblesWindow, true, false)</c>), message 0.
    /// At a date, present and ready once the puzzle is over (<c>puzzleStatus.gameOver</c>), message 1. At the hub, present
    /// and ready while the root menu is showing
    /// (<c>Hub.hubStepType == HubStepType.ROOT &amp;&amp; IsWindowActive(Dialog.dialogOptionsWindow, true, false)</c>) or while the
    /// first location hasn't been picked (<c>GetFlagValue(Hub.firstLocationFlag) &lt; 0</c>), message 2. At special locations and
    /// at the title, <see cref="FinderStatus.Unavailable"/> with message 3.</para>
    /// </remarks>
    FinderStatus GetFinderStatus();

    /// <summary>
    /// The player picked a place in the finder. Return true when the state handled the pick completely, which stops the game
    /// from doing anything else with it; return false to let the player leave for the place as usual (the day advances, the
    /// select sound plays and <c>LocationManager.Depart</c> is called).
    /// </summary>
    /// <remarks>
    /// Asked by the finder app after the <c>FinderSlotSelected</c> event has been raised, so a state that queues a transition
    /// from that event has already done so. Use <paramref name="app"/>.cellphone to reach the phone.
    /// <para>Base states: only the hub does anything. If the first location flag (<c>Hub.firstLocationFlag</c>) is unset it
    /// sets the flag to <c>slot.locationDefinition.id</c>, closes the phone (<c>app.cellphone.Close()</c>) and returns true.
    /// Otherwise it calls <c>Hub.BailHub()</c> and returns false so the player leaves. Every other state returns false.</para>
    /// </remarks>
    bool OnFinderSlotChosen(UiCellphoneAppFinder app, UiAppFinderSlot slot);

    /// <summary>
    /// Whether the store app works. When false the store is greyed out with its message 1.
    /// </summary>
    /// <remarks>
    /// Asked when the store app starts, and again when it is destroyed (to know whether it subscribed to anything), so the
    /// answer must not change while the app is open. The app itself is also off before the tutorial is over.
    /// Base states: true at a sim, at a date and at the hub; false at special locations and at the title.
    /// </remarks>
    bool AllowsStore { get; }

    /// <summary>
    /// Whether a working store (<see cref="AllowsStore"/>) also shows its message 0, which is how it says it can only be
    /// browsed right now. Ignored when <see cref="AllowsStore"/> is false.
    /// </summary>
    /// <remarks>Base states: true at a date, false everywhere else.</remarks>
    bool StoreShowsNotice { get; }

    /// <summary>
    /// Whether the pair app works. When false it is greyed out with its message 0.
    /// </summary>
    /// <remarks>
    /// Asked when the pair app starts. The app is also off before the tutorial is over.
    /// Base states: true at a sim, at a date and at the hub; false at special locations and at the title.
    /// </remarks>
    bool AllowsPairApp { get; }

    /// <summary>
    /// Whether the player is with the girls of the current pair: the pair app shows the pair (relationship slot and both
    /// girls) and the gifts app shows the girls' heads. Also decides whether a successful gift outside of a puzzle focuses
    /// the girl it was given to.
    /// </summary>
    /// <remarks>
    /// Asked when the pair app and the gifts app start, and after a gift is given. Base states: true at a sim and at a date;
    /// false at the hub, at special locations and at the title. Answering true needs the puzzle status to have been built for
    /// the pair, as the apps read the girls' stamina from it.
    /// </remarks>
    bool HasGirlsPresent { get; }

    // -- dolls --

    /// <summary>
    /// Whether a doll's dialog box is always the wide one. When false it is wide only while the cellphone is closed, and
    /// narrow beside the open phone.
    /// </summary>
    /// <remarks>
    /// Asked by <c>UiDoll.ReadDialogLine</c> each time a line starts. Base states: true at the hub, false everywhere else.
    /// </remarks>
    bool DialogIgnoresCellphone { get; }

    /// <summary>
    /// Whether the dolls have a focused girl the player can change, which is how a doll's focus button and drop zone behave.
    /// True: the unfocused doll's focus button can be clicked and gifts can only be dropped on the focused doll. False: the
    /// focus buttons can't be clicked and gifts can be dropped on either doll.
    /// </summary>
    /// <remarks>
    /// Asked by <c>UiDoll.SetFocus</c> each time a doll's focus changes. A state whose focus is locked
    /// (<see cref="IPuzzleStatusRules.LockedAltGirlFocus"/>) must answer false, or the player is given a button that does nothing.
    /// Base states: true at a date whose focus isn't locked; false at a locked date and everywhere else.
    /// </remarks>
    bool DollFocusIsInteractive { get; }

    // -- hints and pausing --

    /// <summary>
    /// Whether the girls' stamina bars hint at the stamina a talk or a gift would cost (what is about to be spent and whether
    /// the item being dragged would cost any). When false the bars show nothing for that.
    /// </summary>
    /// <remarks>
    /// Asked by <c>UiPuzzleStaminaHint.Refresh</c> whenever a bar refreshes. If this is false and the current state is a
    /// puzzle state (<see cref="IPuzzleState"/>), the bar hints at the stamina the move in progress costs instead; if neither
    /// applies it shows no hint. Base states: true at a sim, false everywhere else.
    /// </remarks>
    bool HintsSimStaminaCost { get; }

    /// <summary>
    /// Whether the game is saved when the player arrives in this state, after a departure (not the first arrival of a session).
    /// </summary>
    /// <remarks>
    /// Asked by <c>ExpandedLocationManager.Arrive_Prefix</c> of the state being arrived in, once the state has changed.
    /// A state that is saved this way must be one that can be loaded back (<see cref="IsSavable"/>), otherwise the save
    /// has nothing to restore. Base states: true at the hub, at a sim and at special locations, false at a date and at the title.
    /// </remarks>
    bool AutosavesOnArrival { get; }

    /// <summary>
    /// Whether the hub's own menu (where the player picks what to do next) is up. It is the way out of the state when the cellphone
    /// is hidden (<see cref="HidesCellphone"/>), so a state that hides the phone for the hub's menu must show it.
    /// </summary>
    /// <remarks>
    /// Asked by <c>ExpandedHubManager.PrepHub_Prefix</c> and by the finder's hub slot, which can't be picked from the hub.
    /// The state decides, not where the player is: the hub state is the hub wherever its location is.
    /// Base states: true at the hub, false everywhere else.
    /// </remarks>
    bool ShowsHubMenu { get; }

    /// <summary>
    /// Whether the player may pause (open the cellphone and the menus) right now.
    /// </summary>
    /// <remarks>
    /// Asked by <c>HubManager.CanPause</c>, which <c>GameSession.CanPause</c> combines with the puzzle's own
    /// <c>PuzzleManager.CanPause</c>; both have to allow it. So a puzzle state only answers what its own rules can't: true.
    /// Base states: at the hub, only while the root menu is showing (<c>Hub.hubStepType == HubStepType.ROOT</c>); true everywhere else.
    /// </remarks>
    bool AllowsPause { get; }

    /// <summary>
    /// If the player saves UseStylesOnDates flag should be considered when populating outfits
    /// </summary>
    bool UseStylesOnDatesFlag { get; }
}