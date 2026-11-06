using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public abstract class GameStateLocation : IGameStateLocation
{
    public abstract RelativeId Id { get; }

    public abstract bool IsSavable { get; }

    public virtual bool AllowGifts => true;

    // ---- what the player can use here ----
    //
    // Abstract so that every state has to decide each of these. A state that doesn't answer would otherwise be handed a default
    // that turns an app or a button off, which looks like a bug in the UI instead of a missing answer in the state.
    // What each one means, who asks it and what the base states answer is on IGameState.

    /// <summary>The "there are no girls here" reason, for states with no girls to give a gift to.</summary>
    protected const string NoGirlsNearbyGiftError = "There are no girls nearby to give gifts to.";

    /// <summary>The reason for a sim when the action bubbles aren't showing.</summary>
    protected const string TalkingGiftError = "You can't give gifts while you're talking.";

    /// <summary>The reason during a date.</summary>
    protected const string DateGiftError = "Only date gifts can be given during a date.";

    /// <inheritdoc cref="IGameState.GetGiftError"/>
    public abstract string GetGiftError(ItemDefinition item);

    /// <inheritdoc cref="IGameState.HidesCellphone"/>
    public abstract bool HidesCellphone(UiCellphone cellphone);

    /// <inheritdoc cref="IGameState.GetFinderStatus"/>
    public abstract FinderStatus GetFinderStatus();

    /// <inheritdoc cref="IGameState.OnFinderSlotChosen"/>
    public abstract bool OnFinderSlotChosen(UiCellphoneAppFinder app, UiAppFinderSlot slot);

    /// <inheritdoc cref="IGameState.AllowsStore"/>
    public abstract bool AllowsStore { get; }

    /// <inheritdoc cref="IGameState.StoreShowsNotice"/>
    public abstract bool StoreShowsNotice { get; }

    /// <inheritdoc cref="IGameState.AllowsPairApp"/>
    public abstract bool AllowsPairApp { get; }

    /// <inheritdoc cref="IGameState.HasGirlsPresent"/>
    public abstract bool HasGirlsPresent { get; }

    /// <inheritdoc cref="IGameState.DialogIgnoresCellphone"/>
    public abstract bool DialogIgnoresCellphone { get; }

    /// <inheritdoc cref="IGameState.DollFocusIsInteractive"/>
    public abstract bool DollFocusIsInteractive { get; }

    /// <inheritdoc cref="IGameState.HintsSimStaminaCost"/>
    public abstract bool HintsSimStaminaCost { get; }

    /// <inheritdoc cref="IGameState.AllowsPause"/>
    public abstract bool AllowsPause { get; }

    public abstract bool AutosavesOnArrival { get; }
    public abstract bool ShowsHubMenu { get; }

    public abstract bool UseStylesOnDatesFlag {get;}

    /// <summary>
    /// Set by a state to request a transition on the next arrival (e.g. a UI
    /// selection or a hub departure hook). Shared here so the location manager
    /// can consume it uniformly for every state - see <see cref="TryConsumePendingState"/>.
    /// </summary>
    protected IGameStatePayload _pendingState;
    private bool _disposed;
    public GameStateLocation()
    {
        ModInterface.Events.TitleCanvasReady += On_TitleCanvasReady;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _disposed = true;

        if (disposing)
        {
            //managed
            ModInterface.Events.TitleCanvasReady -= On_TitleCanvasReady;
        }

        //unmanaged
    }

    private void On_TitleCanvasReady(UiTitleCanvas canvas)
    {
        ModInterface.GameState.ChangeState(new GameStateTitle.Payload());
    }

    public bool ShouldApply(float chance) =>
        chance >= 1f || (chance > 0f && UnityEngine.Random.Range(0f, 1f) <= chance);
 
    public int GetRandomValidIndex<T>(IReadOnlyList<T> collection) => collection
        .Select((item, index) => (item, index))
        .Where(x => x.item != null)
        .ToArray()
        .GetRandom()
        .index;
 
    public void ApplyStyleToDoll(GirlStyleInfo style, UiDoll doll, GirlDefinition def) =>
        style?.Apply(doll, def.defaultOutfitIndex, def.defaultHairstyleIndex);
 
    public void ResolveGirlDefinitions(
        GirlPairDefinition pair,
        bool sidesFlipped,
        out GirlDefinition left,
        out GirlDefinition right)
    {
        left = sidesFlipped ? pair.girlDefinitionTwo : pair.girlDefinitionOne;
        right = sidesFlipped ? pair.girlDefinitionOne : pair.girlDefinitionTwo;
    }
 
    public GirlStyleInfo BuildStyleFromFile(PlayerFileGirl file)
    {
        var exp = file.girlDefinition.GetExpansion();
 
        return new GirlStyleInfo(
            exp.OutfitLookup.GetId(file.outfitIndex),
            exp.HairstyleLookup.GetId(file.hairstyleIndex));
    }
 
    /// <summary>
    /// Meeting styles for a pair whose relationship is still UNKNOWN. Shared by any
    /// state that can encounter a first meeting (Sim, Date).
    /// </summary>
    public (GirlStyleInfo left, GirlStyleInfo right, bool isCutsceneStyle) ResolveMeetingStyles(
        GirlPairDefinition pair, bool sidesFlipped)
    {
        var pairStyle = pair.GetExpansion().PairStyle;
        if (pairStyle == null) return (null, null, false);
 
        return sidesFlipped
            ? (pairStyle.MeetingGirlTwo, pairStyle.MeetingGirlOne, pair.introductionPair)
            : (pairStyle.MeetingGirlOne, pairStyle.MeetingGirlTwo, pair.introductionPair);
    }
 
    /// <summary>
    /// Resolution shared by any non-Hub, non-Date state (Sim, Special): meeting
    /// styles for a not-yet-met pair, else the location's default style, else none.
    /// </summary>
    public (GirlStyleInfo left, GirlStyleInfo right, bool isCutsceneStyle) ResolveDefaultStyles(
        GirlPairDefinition pair,
        PlayerFileGirlPair playerPair,
        LocationDefinition location,
        bool sidesFlipped)
    {
        if (playerPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
        {
            return ResolveMeetingStyles(pair, sidesFlipped);
        }
 
        var locationExp = location.GetExpansion();
        if (locationExp.DefaultStyle.HasValue)
        {
            var style = new GirlStyleInfo(locationExp.DefaultStyle.Value);
            return (style, style, false);
        }
 
        return (null, null, false);
    }
 
    public GirlStyleInfo ApplyStyleOverride(
        GirlDefinition def, LocationDefinition loc, GirlStyleInfo style, bool isCutsceneStyle)
    {
        var args = ModInterface.Events.NotifyRequestStyleChange(def, loc, 0f, style, isCutsceneStyle);
        return ShouldApply(args.ApplyChance) ? args.Style : style;
    }
 
    /// <summary>
    /// Full "resolve defaults, allow mod override, apply to both dolls" pipeline
    /// used verbatim by both SimGameState and SpecialGameState.
    /// </summary>
    public void ApplyDefaultDollStyles(ResolveDollStylesArgs args)
    {
        if (args.GirlPair == null) return;
 
        var playerPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(args.GirlPair);
        if (playerPair == null) return;
 
        ResolveGirlDefinitions(args.GirlPair, args.SidesFlipped, out var leftDef, out var rightDef);
 
        var (leftStyle, rightStyle, isCutsceneStyle) = ResolveDefaultStyles(args.GirlPair, playerPair, args.Location, args.SidesFlipped);
 
        leftStyle = ApplyStyleOverride(leftDef, args.Location, leftStyle, isCutsceneStyle);
        rightStyle = ApplyStyleOverride(rightDef, args.Location, rightStyle, isCutsceneStyle);
 
        ApplyStyleToDoll(leftStyle, Game.Session.gameCanvas.dollLeft, leftDef);
        ApplyStyleToDoll(rightStyle, Game.Session.gameCanvas.dollRight, rightDef);
    }

    public abstract void DepartTransition(IGameStatePayload payload, Action continueTransitionCallback);

    public void DepartTransition(Action continueTransitionCallback) => DepartTransition(null, continueTransitionCallback);

    /// <summary>
    /// Requests a transition into <paramref name="stateId"/> on the next arrival. The location manager applies it before it raises
    /// PreLocationArrive, so the new state is entered (and mods are notified via GameStateChanged) before any arrival event.
    /// The last request before the arrival wins. Use this when code departs on behalf of the current state.
    /// </summary>
    public void QueueTransition(IGameStatePayload stateId) => _pendingState = stateId;

    /// <summary>
    /// Called once by ExpandedLocationManager, immediately before it fires the
    /// single post-arrival event, so a queued transition takes effect first and
    /// the new active state is the one that receives the
    /// arrival event and runs its setup.
    /// </summary>
    public bool TryConsumePendingState(out IGameStatePayload payload)
    {
        if (_pendingState == null)
        {
            payload = null;
            return false;
        }

        payload = _pendingState;
        _pendingState = null;
        return true;
    }

    protected static void FinishArrival(LocationArriveArgs args, bool processArriveBundles = true)
    {
        Game.Session.Location.ResetDolls(args.girlPairDef == Game.Session.Puzzle.bossGirlPairDefinition);
        Game.Session.Hub.PrepHub();
        Game.Session.gameCanvas.header.Refresh(hard: true);
        Game.Session.gameCanvas.cellphone.Refresh(hard: true);
        args.arrivalCutscene = null;

        if (processArriveBundles) Game.Session.Logic.ProcessBundleList(args.locationDef.arriveBundleList);
    }

    public abstract IGameStatePayload GeneratePayload();
}