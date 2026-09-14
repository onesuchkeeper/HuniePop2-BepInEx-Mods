using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace Hp2BaseMod;

[Expansion(typeof(LocationManager))]
public partial class ExpandedLocationManager
{
    [HarmonyPatch(typeof(LocationManager))]
    private static class Patch
    {
        [HarmonyPatch(nameof(LocationManager.Arrive))]
        [HarmonyPrefix]
        private static void PreArrive(LocationManager __instance,
            ref LocationDefinition locationDef,
            ref GirlPairDefinition girlPairDef,
            ref bool sidesFlipped,
            ref bool initialArrive)
            => ExpandedLocationManager.Get(__instance).Arrive_Prefix(
                ref locationDef,
                ref girlPairDef,
                ref sidesFlipped,
                ref initialArrive);

        [HarmonyPatch(nameof(LocationManager.Arrive))]
        [HarmonyPostfix]
        private static void PostArrive(LocationManager __instance)
            => ExpandedLocationManager.Get(__instance).Arrive_Postfix();

        [HarmonyPatch(nameof(LocationManager.Depart))]
        [HarmonyPrefix]
        private static bool PreDepart(LocationManager __instance, ref LocationDefinition locationDef, ref GirlPairDefinition girlPairDef, ref bool sidesFlipped)
            => ExpandedLocationManager.Get(__instance).Depart_Prefix(ref locationDef, ref girlPairDef, ref sidesFlipped);

        [HarmonyPatch("OnLocationSettled")]
        [HarmonyPrefix]
        private static bool OnLocationSettled(LocationManager __instance)
            => ExpandedLocationManager.Get(__instance).LocationSettled_Prefix();

        [HarmonyPatch(nameof(LocationManager.ResetDolls))]
        [HarmonyPostfix]
        private static void ResetDolls(LocationManager __instance, bool unload = false)
            => ExpandedLocationManager.Get(__instance).ResetDolls(unload);

        [HarmonyPatch("OnDestroy")]
        [HarmonyPostfix]
        private static void OnDestroy(LocationManager __instance)
            => ExpandedLocationManager.Destroy(__instance);
    }

    //TODO, move to expanded ui puzzle grid
    [HarmonyPatch(typeof(UiPuzzleGrid))]
    internal static class UiPuzzleGridCellphonePatch
    {
        [HarmonyPatch(nameof(UiPuzzleGrid.RefreshGirlDolls))]
        [HarmonyPostfix]
        private static void RefreshGirlDolls(UiPuzzleGrid __instance)
            => ExpandedLocationManager.RefreshGirlDolls();
    }

    private CutsceneDefinition _baseCutsceneMeeting;
    private UiWindow _actionBubblesWindow;

    /// <summary>
    /// The default anchored position of the puzzle grid, captured on first arrival
    /// before any CellphoneOnLeft repositioning is applied.
    /// </summary>
    private Vector2? _defaultPuzzleGridPosition;

    private void OnInit()
    {
        _actionBubblesWindow = _core.actionBubblesWindow;
        _baseCutsceneMeeting = _core.cutsceneMeeting; 
    }

    private bool Arrive_Prefix(ref LocationDefinition locationDef,
        ref GirlPairDefinition girlPairDef,
        ref bool sidesFlipped,
        ref bool initialArrive)
    {
        var previousLocation = _currentLocation;

        // Fallback if locationDef is null before checking its type
        if (locationDef == null)
        {
            ModInterface.Log.Error("Location def was set to null pre arrival, defaulting to hub");
            locationDef = ModInterface.GameData.GetLocation(Locations.HotelRoom);
            girlPairDef = null;
        }

        var args = new LocationArriveArgs()
        {
            locationDef = locationDef,
            girlPairDef = girlPairDef,
            sidesFlipped = sidesFlipped,
            initialArrive = initialArrive,
            cellphoneOnLeft = locationDef.locationType == LocationType.HUB,
            meetingCutscene = _baseCutsceneMeeting,
            previousLocationDef = previousLocation,
            Canceled = false
        };

        ModInterface.Events.NotifyPreLocationArrive(args);

        if (args.Canceled)
        {
            ModInterface.Log.Message("Arrive canceled");
            return false; // Skips base LocationManager.Arrive
        }

        _currentLocation = args.locationDef;
        _core.cutsceneMeeting = args.meetingCutscene ?? _baseCutsceneMeeting;

        locationDef = args.locationDef;
        girlPairDef = args.girlPairDef;
        sidesFlipped = args.sidesFlipped;
        initialArrive = args.initialArrive;
        ModInterface.State.CellphoneOnLeft = args.cellphoneOnLeft;

        var strBuilder = new StringBuilder($"Arriving at {locationDef.locationName}");
        if (girlPairDef == null) strBuilder.Append(" with no pair.");
        else strBuilder.Append($" with {girlPairDef.girlDefinitionOne.girlName} and {girlPairDef.girlDefinitionTwo.girlName}");
        ModInterface.Log.Message(strBuilder.ToString());

        return true;
    }

    /// <summary>
    /// Updates the position of the cellphone and puzzle grid based on
    /// <see cref="ModInterface.State.CellphoneOnLeft"/>.
    /// When the cellphone moves to the left the header shifts, and the puzzle
    /// grid must shift right by the same delta to stay on screen.
    /// </summary>
    private void Arrive_Postfix()
    {
        var header = Game.Session.gameCanvas.header;
        var cellphone = Game.Session.gameCanvas.cellphone;
        var puzzleGrid = Game.Session.Puzzle.puzzleGrid;

        // Capture the default grid position the first time we see the grid,
        // before any CellphoneOnLeft offset is applied.
        if (puzzleGrid != null && _defaultPuzzleGridPosition == null)
        {
            _defaultPuzzleGridPosition = puzzleGrid.GetComponent<RectTransform>().anchoredPosition;
        }

        if (ModInterface.State.CellphoneOnLeft)
        {
            header.rectTransform.anchoredPosition = new Vector2(
                header.xValues.y,
                header.rectTransform.anchoredPosition.y);

            cellphone.rectTransform.anchoredPosition = new Vector2(
                cellphone.xValues.y,
                cellphone.rectTransform.anchoredPosition.y);

            if (puzzleGrid != null && _defaultPuzzleGridPosition.HasValue)
            {
                var delta = header.xValues.y - header.xValues.x;
                puzzleGrid.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                    _defaultPuzzleGridPosition.Value.x + delta,
                    _defaultPuzzleGridPosition.Value.y);
            }
        }
        else
        {
            header.rectTransform.anchoredPosition = new Vector2(
                header.xValues.x,
                header.rectTransform.anchoredPosition.y);

            cellphone.rectTransform.anchoredPosition = new Vector2(
                cellphone.xValues.x,
                cellphone.rectTransform.anchoredPosition.y);

            if (puzzleGrid != null && _defaultPuzzleGridPosition.HasValue)
            {
                puzzleGrid.GetComponent<RectTransform>().anchoredPosition =
                    _defaultPuzzleGridPosition.Value;
            }
        }
    }

    private bool Depart_Prefix(ref LocationDefinition locationDef, ref GirlPairDefinition girlPairDef, ref bool sidesFlipped)
    {
        var args = new LocationDepartArgs()
        {
            from = _currentLocation,
            to = locationDef,
            girlPairDef = girlPairDef,
            sidesFlipped = sidesFlipped,
            Canceled = false
        };

        ModInterface.Events.NotifyPreLocationDepart(args);

        if (args.Canceled)
        {
            return false; // Skips base LocationManager.Depart entirely
        }

        locationDef = args.to;
        girlPairDef = args.girlPairDef;
        sidesFlipped = args.sidesFlipped;
        return true; // Proceed with normal departure
    }

    /// <summary>
    /// Called from <see cref="UiPuzzleGridCellphonePatch.RefreshGirlDolls"/> postfix.
    /// When <see cref="ModInterface.State.CellphoneOnLeft"/> is active the left doll
    /// is hidden. Its focus button is disabled and any hover state is cleared after
    /// every RefreshGirlDolls call, RefreshGirlDolls may re-enable it internally,
    /// and without the explicit OnPointerExit the tooltip can persist.
    /// </summary>
    private static void RefreshGirlDolls()
    {
        if (!ModInterface.State.CellphoneOnLeft) return;
        var focusButton = Game.Session.gameCanvas.dollLeft.focusButton;
        focusButton.Disable();
        focusButton.OnPointerExit(null);
    }

    /// <summary>
    /// Notifies of location settling, allowing location type and ui to be overwritten.
    /// Notifies of a random doll selection, allowing selection to be overwritten.
    /// </summary>
    private bool LocationSettled_Prefix()
    {
        var locationSettledArgs = new LocationSettledArgs()
        {
            locationType = _core.currentLocation.locationType,
            actionBubblesWindow = _actionBubblesWindow
        };

        ModInterface.Events.NotifyPreLocationSettled(locationSettledArgs);
        _core.actionBubblesWindow = locationSettledArgs.actionBubblesWindow ?? _actionBubblesWindow;

        _isLocked = false;
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
        _arrivalCutscene = null;

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
    private void ResetDolls(bool unload)
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
        var expansion = hubDef.GetExpansion();

        int outfitIndex = GetRandomValidIndex(hubDef.outfits);
        var outfit = hubDef.outfits[outfitIndex];

        var style = new GirlStyleInfo
        {
            OutfitId = expansion.OutfitLookup[outfitIndex],
            HairstyleId = outfit.pairHairstyleIndex != -1 
                ? expansion.HairstyleLookup[outfit.pairHairstyleIndex]
                : expansion.HairstyleLookup[GetRandomValidIndex(hubDef.hairstyles)]
        };

        //for the normal kyu randomization don't do nsfw outfits
        if (outfit.GetExpansion().IsNSFW) return;

        var args = ModInterface.Events.NotifyRequestStyleChange(hubDef, location, 0.1f, style, false);

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
        var (leftStyle, rightStyle, isCutsceneStyle) = ResolveBaseStyles(pair, playerPair, location, leftDef, rightDef);

        // Allow mod overrides
        leftStyle = ApplyStyleOverride(leftDef, location, leftStyle, isCutsceneStyle);
        rightStyle = ApplyStyleOverride(rightDef, location, rightStyle, isCutsceneStyle);

        // Apply to dolls
        ApplyStyleToDoll(leftStyle, Game.Session.gameCanvas.dollLeft, leftDef);
        ApplyStyleToDoll(rightStyle, Game.Session.gameCanvas.dollRight, rightDef);
    }

    /// <summary>
    /// Determines initial styles before mod overrides.
    /// </summary>
    private (GirlStyleInfo left, GirlStyleInfo right, bool isCutsceneStyle) ResolveBaseStyles(
        GirlPairDefinition pair,
        PlayerFileGirlPair playerPair,
        LocationDefinition location,
        GirlDefinition leftDef,
        GirlDefinition rightDef)
    {
        var locationExp = location.GetExpansion();

        // UNKNOWN relationship use meeting styles
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
            return (new GirlStyleInfo(locationExp.DefaultStyle.Value),new GirlStyleInfo(locationExp.DefaultStyle.Value), false);
        }

        return (null, null, false);
    }

    private (GirlStyleInfo, GirlStyleInfo, bool) ResolveDateStyles(
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
    private (GirlStyleInfo, GirlStyleInfo, bool) GetPairSexStyles(
        GirlPairDefinition pair,
        LocationDefinition location,
        GirlDefinition leftDef,
        GirlDefinition rightDef)
    {
        var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, pair.id);
        var pairStyle = pair.GetExpansion().PairStyle;

        if (pairStyle == null) return (null, null, false);

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

        return (left, right, false);
    }

    /// <summary>
    /// Resolves pair-based styles (Meeting / Sex).
    /// </summary>
    private (GirlStyleInfo, GirlStyleInfo, bool) GetPairMeetingStyles(GirlPairDefinition pair)
    {
        var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, pair.id);
        var pairStyle = pair.GetExpansion().PairStyle;

        if (pairStyle == null) return (null, null, false);

        bool flipped = f_currentSidesFlipped.GetValue<bool>(_core);

        return flipped
            ? (pairStyle.MeetingGirlTwo, pairStyle.MeetingGirlOne, pair.introductionPair)
            : (pairStyle.MeetingGirlOne, pairStyle.MeetingGirlTwo, pair.introductionPair);
    }

    /// <summary>
    /// Resolves styles based on player file data.
    /// </summary>
    private (GirlStyleInfo, GirlStyleInfo, bool) ResolveFileStyles()
    {
        var leftFile = Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl;
        var rightFile = Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl;

        return (
            BuildStyleFromFile(leftFile),
            BuildStyleFromFile(rightFile),
            false
        );
    }

    /// <summary>
    /// Builds a GirlStyleInfo from player file indices.
    /// </summary>
    private GirlStyleInfo BuildStyleFromFile(PlayerFileGirl file)
    {
        var exp = file.girlDefinition.GetExpansion();

        return new GirlStyleInfo(
            exp.OutfitLookup.GetId(file.outfitIndex),
            exp.HairstyleLookup.GetId(file.hairstyleIndex)
        );
    }

    /// <summary>
    /// Applies mod override logic.
    /// </summary>
    private GirlStyleInfo ApplyStyleOverride(GirlDefinition def, LocationDefinition loc, GirlStyleInfo style, bool isCutsceneStyle)
    {
        var args = ModInterface.Events.NotifyRequestStyleChange(def, loc, 0, style, isCutsceneStyle);

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
    private (GirlStyleInfo left, GirlStyleInfo right, bool) ResolveLocationStyles(
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

        return (left, right, false);
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
        // If stylesOnDates is disabled use location-based style
        if (!playerFile.stylesOnDates)
        {
            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girlDef.id);
            var expansion = girlDef.GetExpansion();

            if (expansion.GetCurrentBody()
                .LocationIdToOutfitId
                .TryGetValue(locationId, out var style))
            {
                ModInterface.Log.Message($"Using location style for {sideLabel} girl: {style}");
                return style;
            }

            // No mapping found fall through (returns null)
            ModInterface.Log.Message($"No location style found for {sideLabel} girl");
            return null;
        }

        // Otherwise use player file style
        var exp = playerFile.girlDefinition.GetExpansion();

        var fileStyle = new GirlStyleInfo(
            exp.OutfitLookup.GetId(playerFile.outfitIndex),
            exp.HairstyleLookup.GetId(playerFile.hairstyleIndex)
        );

        ModInterface.Log.Message($"Using file style for {sideLabel} girl: {fileStyle}");

        return fileStyle;
    }
}