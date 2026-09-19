using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public abstract class GameStateLocation : IGameState
{
    public abstract RelativeId Id { get; }

    public abstract bool IsSavable { get; }

    /// <summary>
    /// Set by a state to request a transition on the next arrival (e.g. a UI
    /// selection or a hub departure hook). Shared here so the location manager
    /// can consume it uniformly for every state - see <see cref="ConsumePendingState"/>.
    /// </summary>
    protected RelativeId? _pendingState;


    public virtual void Enter()
    {
        ModInterface.Events.TitleCanvasReady += On_TitleCanvasReady;
    }

    public virtual void Exit()
    {
        ModInterface.Events.TitleCanvasReady -= On_TitleCanvasReady;
    }

    private void On_TitleCanvasReady(UiTitleCanvas canvas)
    {
        ModInterface.GameState.ChangeState(GameStateId.Title);
    }

    public abstract string GetProfileString(PlayerFile playerFile);

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

    public abstract void DepartTransition(RelativeId? nextStateId, Action continueTransitionCallback);

    public void DepartTransition(Action continueTransitionCallback) => DepartTransition(null, continueTransitionCallback);

    /// <summary>
    /// Called once by ExpandedLocationManager, immediately before it fires the
    /// single post-arrival event, so a queued transition takes effect first and
    /// the *new* active state - not this one - is the one that receives the
    /// arrival event and runs its setup.
    /// </summary>
    internal RelativeId? ConsumePendingState()
    {
        var next = _pendingState;
        _pendingState = null;
        return next;
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
}