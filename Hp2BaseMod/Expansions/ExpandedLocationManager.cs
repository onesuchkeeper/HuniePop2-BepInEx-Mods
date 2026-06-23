using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(LocationManager))]
internal static class LocationManagerPatch
{
    [HarmonyPatch(nameof(LocationManager.Arrive))]
    [HarmonyPrefix]
    private static void PreArrive(LocationManager __instance,
        ref LocationDefinition locationDef,
        ref GirlPairDefinition girlPairDef,
        ref bool sidesFlipped,
        ref bool initialArrive)
        => ExpandedLocationManager.Get(__instance).PreArrive(
            ref locationDef,
            ref girlPairDef,
            ref sidesFlipped,
            ref initialArrive);

    [HarmonyPatch(nameof(LocationManager.Arrive))]
    [HarmonyPostfix]
    private static void PostArrive(LocationManager __instance)
        => ExpandedLocationManager.Get(__instance).PostArrive();

    [HarmonyPatch("OnLocationSettled")]
    [HarmonyPrefix]
    private static bool OnLocationSettled(LocationManager __instance)
        => ExpandedLocationManager.Get(__instance).PreLocationSettled();

    [HarmonyPatch(nameof(LocationManager.ResetDolls))]
    [HarmonyPostfix]
    private static void ResetDolls(LocationManager __instance, bool unload = false)
        => ExpandedLocationManager.Get(__instance).ResetDolls(unload);

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    private static void OnDestroy(LocationManager __instance)
        => ExpandedLocationManager.Get(__instance).OnDestroy();
}

public class ExpandedLocationManager
{
    private static Dictionary<LocationManager, ExpandedLocationManager> _expansions
     = new Dictionary<LocationManager, ExpandedLocationManager>();

    public static ExpandedLocationManager Get(LocationManager core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedLocationManager(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_isLocked = AccessTools.Field(typeof(LocationManager), "_isLocked");
    private static readonly FieldInfo f_arrivalCutscene = AccessTools.Field(typeof(LocationManager), "_arrivalCutscene");
    private static readonly FieldInfo f_currentGirlPair = AccessTools.Field(typeof(LocationManager), "_currentGirlPair");
    private static readonly FieldInfo f_currentSidesFlipped = AccessTools.Field(typeof(LocationManager), "_currentSidesFlipped");
    private static readonly FieldInfo f_currentLocation = AccessTools.Field(typeof(LocationManager), "_currentLocation");

    private CutsceneDefinition _baseCutsceneMeeting;
    private UiWindow _actionBubblesWindow;
    private LocationManager _core;
    private ExpandedLocationManager(LocationManager core)
    {
        _core = core;
        _actionBubblesWindow = _core.actionBubblesWindow;
        _baseCutsceneMeeting = _core.cutsceneMeeting;
    }

    /// <summary>
    /// Notifies and allows the overriding of arrival parameters
    /// </summary>
    /// <param name="locationDef">The location to arrive to</param>
    /// <param name="girlPairDef">The pair at the location</param>
    /// <param name="sidesFlipped">If the pairs have their sides flipped</param>
    /// <param name="initialArrive">If this is the session's first arrival</param>
    public void PreArrive(ref LocationDefinition locationDef,
        ref GirlPairDefinition girlPairDef,
        ref bool sidesFlipped,
        ref bool initialArrive)
    {
        // set here so the AtLocationType is working with the correct location
        // if it changes it'll overwrite later on
        f_currentLocation.SetValue(_core, locationDef);

        ModInterface.State.CellphoneOnLeft = _core.AtLocationType(LocationType.HUB);

        var args = new LocationArriveArgs()
        {
            locationDef = locationDef,
            girlPairDef = girlPairDef,
            sidesFlipped = sidesFlipped,
            initialArrive = initialArrive,
            cellphoneOnLeft = ModInterface.State.CellphoneOnLeft,
            meetingCutscene = _baseCutsceneMeeting
        };
        ModInterface.Events.NotifyPreLocationArrive(args);

        _core.cutsceneMeeting = args.meetingCutscene ?? _baseCutsceneMeeting;

        locationDef = args.locationDef;
        girlPairDef = args.girlPairDef;
        sidesFlipped = args.sidesFlipped;
        initialArrive = args.initialArrive;
        ModInterface.State.CellphoneOnLeft = args.cellphoneOnLeft;

        if (locationDef == null)
        {
            ModInterface.Log.Error("Location def was set to null pre arrival, defaulting to hub");
            locationDef = ModInterface.GameData.GetLocation(Locations.HotelRoom);
            girlPairDef = null;
        }

        var strBuilder = new StringBuilder($"Arriving at {locationDef.locationName}");
        if (girlPairDef == null) strBuilder.Append(" with no pair.");
        else strBuilder.Append($" with {girlPairDef.girlDefinitionOne.girlName} and {girlPairDef.girlDefinitionTwo.girlName}");
        ModInterface.Log.Message(strBuilder.ToString());
    }

    /// <summary>
    /// Updates the position of the cellphone based on <see cref="ModInterface.State.CellphoneOnLeft">
    /// </summary>
    public void PostArrive()
    {
        if (ModInterface.State.CellphoneOnLeft)
        {
            Game.Session.gameCanvas.header.rectTransform.anchoredPosition = new Vector2(Game.Session.gameCanvas.header.xValues.y,
                Game.Session.gameCanvas.header.rectTransform.anchoredPosition.y);

            Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition = new Vector2(Game.Session.gameCanvas.cellphone.xValues.y,
                Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition.y);
        }
        else
        {
            Game.Session.gameCanvas.header.rectTransform.anchoredPosition = new Vector2(Game.Session.gameCanvas.header.xValues.x,
                Game.Session.gameCanvas.header.rectTransform.anchoredPosition.y);

            Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition = new Vector2(Game.Session.gameCanvas.cellphone.xValues.x,
                Game.Session.gameCanvas.cellphone.rectTransform.anchoredPosition.y);
        }
    }

    /// <summary>
    /// Notifies of location settling, allowing location type and ui to be overwritten.
    /// Notifies of a random doll selection, allowing selection to be overwritten.
    /// </summary>
    public bool PreLocationSettled()
    {
        var locationSettledArgs = new LocationSettledArgs()
        {
            locationType = _core.currentLocation.locationType,
            actionBubblesWindow = _actionBubblesWindow
        };

        ModInterface.Events.NotifyPreLocationSettled(locationSettledArgs);
        _core.actionBubblesWindow = locationSettledArgs.actionBubblesWindow ?? _actionBubblesWindow;

        f_isLocked.SetValue(_core, false);
        Game.Session.Logic.ProcessBundleList(_core.currentLocation.departBundleList, false);
        var arrivalCutscene = f_arrivalCutscene.GetValue<CutsceneDefinition>(_core);
        switch (locationSettledArgs.locationType)
        {
            case LocationType.SIM:
                ModInterface.Log.Message("Location settled as sim - Starting sim");
                Game.Manager.Windows.ShowWindow(_core.actionBubblesWindow, false);
                if (arrivalCutscene == null)
                {
                    var randomDollArgs = new RandomDollSelectedArgs();
                    ModInterface.Events.NotifyRandomDollSelected(randomDollArgs);

                    var uiDoll = randomDollArgs.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());
                    var greetingIndex = Mathf.Clamp(Game.Persistence.playerFile.daytimeElapsed % 4, 0, _core.dtGreetings.Length - 1);

                    uiDoll.ReadDialogTrigger(_core.dtGreetings[greetingIndex], DialogLineFormat.PASSIVE, -1);
                }
                break;
            case LocationType.DATE:
                ModInterface.Log.Message("Location settled as date - Starting puzzle");
                Game.Session.Puzzle.StartPuzzle();
                break;
            case LocationType.HUB:
                if (arrivalCutscene == null)
                {
                    Game.Session.gameCanvas.GetDoll(DollOrientationType.RIGHT).ReadDialogTrigger(Game.Session.Hub.GetGreeting(), DialogLineFormat.PASSIVE, -1);
                }
                ModInterface.Log.Message("Location settled as hub - Starting hub");
                Game.Session.Hub.StartHub();
                break;
        }
        f_arrivalCutscene.SetValue(_core, null);

        return false;
    }

    /// <summary>
    /// Resets doll styles based on current location and context.
    /// 
    /// HUB:
    /// - Uses randomized outfit with paired/random hairstyle.
    /// 
    /// DATE / OTHER:
    /// - Resolves styles based on relationship, location, or player file.
    /// - Allows mod hooks to override styles.
    /// </summary>
    public void ResetDolls(bool unload)
    {
        // Early exit: nothing to do if unloading
        if (unload) return;

        var currentLocation = f_currentLocation.GetValue(_core) as LocationDefinition;

        if (_core.AtLocationType(LocationType.HUB))
        {
            ApplyHubStyle(currentLocation);
            return;
        }

        ApplyNonHubStyles(currentLocation);
    }

    /// <summary>
    /// Applies randomized HUB style to the hub girl.
    /// </summary>
    private void ApplyHubStyle(LocationDefinition location)
    {
        var hubDef = Game.Session.Hub.hubGirlDefinition;
        var expansion = hubDef.Expansion();

        int outfitIndex = GetRandomValidIndex(hubDef.outfits);
        var outfit = hubDef.outfits[outfitIndex];

        var style = new GirlStyleInfo
        {
            OutfitId = expansion.OutfitLookup[outfitIndex],
            HairstyleId = outfit.pairHairstyleIndex != -1 
                ? expansion.HairstyleLookup[outfit.pairHairstyleIndex]
                : expansion.HairstyleLookup[GetRandomValidIndex(hubDef.hairstyles)]
        };

        var args = ModInterface.Events.NotifyRequestStyleChange(hubDef, location, 0.1f, style);

        if (ShouldApply(args.ApplyChance))
        {
            args.Style.Apply(
                Game.Session.gameCanvas.dollRight,
                hubDef.defaultOutfitIndex,
                hubDef.defaultHairstyleIndex
            );
        }
    }

    /// <summary>
    /// Handles all non-HUB doll reset logic (DATE, etc).
    /// </summary>
    private void ApplyNonHubStyles(LocationDefinition location)
    {
        var pair = f_currentGirlPair.GetValue(_core) as GirlPairDefinition;
        var playerPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(pair);

        if (playerPair == null) return;

        ResolveGirlDefinitions(pair, out var leftDef, out var rightDef);

        // Resolve base styles
        var (leftStyle, rightStyle) = ResolveBaseStyles(pair, playerPair, location, leftDef, rightDef);

        // Allow mod overrides
        leftStyle = ApplyStyleOverride(leftDef, location, leftStyle);
        rightStyle = ApplyStyleOverride(rightDef, location, rightStyle);

        // Apply to dolls
        ApplyStyleToDoll(leftStyle, Game.Session.gameCanvas.dollLeft, leftDef);
        ApplyStyleToDoll(rightStyle, Game.Session.gameCanvas.dollRight, rightDef);
    }

    /// <summary>
    /// Determines initial styles before mod overrides.
    /// </summary>
    private (GirlStyleInfo left, GirlStyleInfo right) ResolveBaseStyles(
        GirlPairDefinition pair,
        PlayerFileGirlPair playerPair,
        LocationDefinition location,
        GirlDefinition leftDef,
        GirlDefinition rightDef)
    {
        var locationExp = location.Expansion();

        // UNKNOWN relationship → meeting styles
        if (playerPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
        {
            return GetPairMeetingStyles(pair);
        }

        // DATE-specific logic
        if (_core.AtLocationType(LocationType.DATE))
        {
            return ResolveDateStyles(pair, playerPair, location, leftDef, rightDef);
        }

        // Location default style
        if (locationExp.DefaultStyle.HasValue)
        {
            return (new GirlStyleInfo(locationExp.DefaultStyle.Value),new GirlStyleInfo(locationExp.DefaultStyle.Value));
        }

        return (null, null);
    }

    private (GirlStyleInfo, GirlStyleInfo) ResolveDateStyles(
        GirlPairDefinition pair,
        PlayerFileGirlPair playerPair,
        LocationDefinition location,
        GirlDefinition leftDef,
        GirlDefinition rightDef)
    {
        var args = BuildPreDateArgs(playerPair, location);
        ModInterface.Events.NotifyPreDateDollReset(args);

        return args.Style switch
        {
            PreDateDollResetArgs.StyleType.Sex =>
                GetPairSexStyles(pair, location, leftDef, rightDef),

            PreDateDollResetArgs.StyleType.Location =>
                ResolveLocationStyles(location, leftDef, rightDef),

            _ =>
                ResolveFileStyles()
        };
    }

    /// <summary>
    /// Resolves pair-based styles (Meeting / Sex).
    /// </summary>
    private (GirlStyleInfo, GirlStyleInfo) GetPairSexStyles(
        GirlPairDefinition pair,
        LocationDefinition location,
        GirlDefinition leftDef,
        GirlDefinition rightDef)
    {
        var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, pair.id);
        var pairStyle = ExpandedGirlPairDefinition.Get(pairId).PairStyle;

        if (pairStyle == null) return (null, null);

        bool flipped = f_currentSidesFlipped.GetValue<bool>(_core);

        var left = flipped
            ? pairStyle.SexGirlTwo
            : pairStyle.SexGirlOne;

        var right = flipped
            ? pairStyle.SexGirlOne
            : pairStyle.SexGirlTwo;

        var locationStyles = ResolveLocationStyles(location, leftDef, rightDef);

        if (left != null &&
            left.OutfitId == RelativeId.Default &&
            left.HairstyleId == RelativeId.Default)
        {
            left = locationStyles.left;
        }

        if (right != null &&
            right.OutfitId == RelativeId.Default &&
            right.HairstyleId == RelativeId.Default)
        {
            right = locationStyles.right;
        }

        return (left, right);
    }

    /// <summary>
    /// Resolves pair-based styles (Meeting / Sex).
    /// </summary>
    private (GirlStyleInfo, GirlStyleInfo) GetPairMeetingStyles(GirlPairDefinition pair)
    {
        var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, pair.id);
        var pairStyle = ExpandedGirlPairDefinition.Get(pairId).PairStyle;

        if (pairStyle == null) return (null, null);

        bool flipped = f_currentSidesFlipped.GetValue<bool>(_core);

        return flipped
            ? (pairStyle.MeetingGirlTwo, pairStyle.MeetingGirlOne)
            : (pairStyle.MeetingGirlOne, pairStyle.MeetingGirlTwo);
    }

    /// <summary>
    /// Resolves styles based on player file data.
    /// </summary>
    private (GirlStyleInfo, GirlStyleInfo) ResolveFileStyles()
    {
        var leftFile = Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl;
        var rightFile = Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl;

        return (
            BuildStyleFromFile(leftFile),
            BuildStyleFromFile(rightFile)
        );
    }

    /// <summary>
    /// Builds a GirlStyleInfo from player file indices.
    /// </summary>
    private GirlStyleInfo BuildStyleFromFile(PlayerFileGirl file)
    {
        var exp = ExpandedGirlDefinition.Get(file.girlDefinition.id);

        return new GirlStyleInfo(
            exp.OutfitLookup.GetId(file.outfitIndex),
            exp.HairstyleLookup.GetId(file.hairstyleIndex)
        );
    }

    /// <summary>
    /// Applies mod override logic.
    /// </summary>
    private GirlStyleInfo ApplyStyleOverride(GirlDefinition def, LocationDefinition loc, GirlStyleInfo style)
    {
        var args = ModInterface.Events.NotifyRequestStyleChange(def, loc, 0, style);

        return ShouldApply(args.ApplyChance) ? args.Style : style;
    }

    /// <summary>
    /// Determines if a probabilistic style should be applied.
    /// </summary>
    private bool ShouldApply(float chance) =>
        chance >= 1f || (chance > 0f && UnityEngine.Random.Range(0f, 1f) <= chance);

    /// <summary>
    /// Applies style safely to a doll.
    /// </summary>
    private void ApplyStyleToDoll(GirlStyleInfo style, UiDoll doll, GirlDefinition def)
    {
        style?.Apply(doll, def.defaultOutfitIndex, def.defaultHairstyleIndex);
    }

    /// <summary>
    /// Gets a random valid (non-null) index from a collection.
    /// </summary>
    private int GetRandomValidIndex<T>(IReadOnlyList<T> collection) => collection
        .Select((item, index) => (item, index))
        .Where(x => x.item != null)
        .ToArray()
        .GetRandom()
        .index;

    /// <summary>
    /// Resolves left/right definitions accounting for flipped state.
    /// </summary>
    private void ResolveGirlDefinitions(
        GirlPairDefinition pair,
        out GirlDefinition left,
        out GirlDefinition right)
    {
        bool flipped = (bool)f_currentSidesFlipped.GetValue(_core);

        left = flipped ? pair.girlDefinitionTwo : pair.girlDefinitionOne;
        right = flipped ? pair.girlDefinitionOne : pair.girlDefinitionTwo;
    }

    /// <summary>
    /// Builds PreDateDollResetArgs based on relationship state and gameplay context.
    /// Mirrors original branching logic.
    /// </summary>
    private PreDateDollResetArgs BuildPreDateArgs(
        PlayerFileGirlPair playerPair,
        LocationDefinition currentLocation)
    {
        var args = new PreDateDollResetArgs();

        // Sex condition:
        // - Must be ATTRACTED
        // - Must match the scheduled "sex daytime"
        if (playerPair.relationshipType == GirlPairRelationshipType.ATTRACTED &&
            Game.Persistence.playerFile.daytimeElapsed % 4 ==
            (int)playerPair.girlPairDefinition.sexDaytime)
        {
            args.Style = PreDateDollResetArgs.StyleType.Sex;
        }
        // Location condition:
        // - Puzzle is active
        // - Not at boss location
        else if (!Game.Session.Puzzle.puzzleStatus.isEmpty &&
                currentLocation != Game.Session.Puzzle.bossLocationDefinition)
        {
            args.Style = PreDateDollResetArgs.StyleType.Location;
        }
        // Default fallback
        else
        {
            args.Style = PreDateDollResetArgs.StyleType.File;
        }

        return args;
    }

    /// <summary>
    /// Resolves styles based on location mappings or player file preferences.
    /// Handles both left and right girls symmetrically.
    /// </summary>
    private (GirlStyleInfo left, GirlStyleInfo right) ResolveLocationStyles(
        LocationDefinition location,
        GirlDefinition leftDef,
        GirlDefinition rightDef)
    {
        var locationId = ModInterface.Data.GetDataId(GameDataType.Location, location.id);

        var puzzle = Game.Session.Puzzle.puzzleStatus;

        var left = ResolveSingleLocationStyle(
            puzzle.girlStatusLeft.playerFileGirl,
            leftDef,
            locationId,
            "left");

        var right = ResolveSingleLocationStyle(
            puzzle.girlStatusRight.playerFileGirl,
            rightDef,
            locationId,
            "right");

        return (left, right);
    }

    /// <summary>
    /// Resolves a single girl's style using either location mapping or file data.
    /// </summary>
    private GirlStyleInfo ResolveSingleLocationStyle(
        PlayerFileGirl playerFile,
        GirlDefinition girlDef,
        RelativeId locationId,
        string sideLabel)
    {
        // If stylesOnDates is disabled → use location-based style
        if (!playerFile.stylesOnDates)
        {
            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girlDef.id);
            var expansion = ExpandedGirlDefinition.Get(girlId);

            if (expansion.GetCurrentBody()
                .LocationIdToOutfitId
                .TryGetValue(locationId, out var style))
            {
                ModInterface.Log.Message($"Using location style for {sideLabel} girl: {style}");
                return style;
            }

            // No mapping found → fall through (returns null)
            ModInterface.Log.Message($"No location style found for {sideLabel} girl");
            return null;
        }

        // Otherwise → use player file style
        var exp = ExpandedGirlDefinition.Get(playerFile.girlDefinition.id);

        var fileStyle = new GirlStyleInfo(
            exp.OutfitLookup.GetId(playerFile.outfitIndex),
            exp.HairstyleLookup.GetId(playerFile.hairstyleIndex)
        );

        ModInterface.Log.Message($"Using file style for {sideLabel} girl: {fileStyle}");

        return fileStyle;
    }

    internal void OnDestroy()
    {
        _expansions.Remove(_core);
    }
}
