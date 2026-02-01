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
    public static void ResetDolls(LocationManager __instance, bool unload = false)
        => ExpandedLocationManager.Get(__instance).ResetDolls(unload);

    [HarmonyPatch("OnDestroy")]
    [HarmonyPostfix]
    public static void OnDestroy(LocationManager __instance)
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

    public event Action OnRefreshUi;

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

        var strBuilder = new StringBuilder($"Arriving at {locationDef.locationName}");
        if (girlPairDef == null) strBuilder.Append(" with no pair.");
        else strBuilder.Append($" with {girlPairDef?.girlDefinitionOne.girlName} and {girlPairDef?.girlDefinitionTwo.girlName}");
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
                Game.Session.Puzzle.StartPuzzle();
                break;
            case LocationType.HUB:
                if (arrivalCutscene == null)
                {
                    Game.Session.gameCanvas.GetDoll(DollOrientationType.RIGHT).ReadDialogTrigger(Game.Session.Hub.GetGreeting(), DialogLineFormat.PASSIVE, -1);
                }
                Game.Session.Hub.StartHub();
                break;
        }
        f_arrivalCutscene.SetValue(_core, null);

        return false;
    }

    /// <summary>
    /// When dolls reset via location, use the location's
    /// default style and notify to allow overwriting
    /// </summary>
    /// <param name="unload"></param>
    public void ResetDolls(bool unload)
    {
        if (unload) { return; }// if the hub girl does not have the needed indexes

        var currentLocation = f_currentLocation.GetValue(_core) as LocationDefinition;

        if (_core.AtLocationType(LocationType.HUB))
        {
            // the base method already randomizes hub girl outfit, so just use default
            var girlExpansion = Game.Session.Hub.hubGirlDefinition.Expansion();

            var i = 0;
            var outfitIndex = Game.Session.Hub.hubGirlDefinition.outfits.Select(x => (x, i++)).Where(x => x.Item1 != null).ToArray().GetRandom().Item2;
            var outfit = Game.Session.Hub.hubGirlDefinition.outfits[outfitIndex];

            var hubStyle = new GirlStyleInfo()
            {
                OutfitId = girlExpansion.OutfitLookup[outfitIndex],
            };

            if (outfit.pairHairstyleIndex != -1)
            {
                hubStyle.HairstyleId = girlExpansion.HairstyleLookup[outfit.pairHairstyleIndex];
            }
            else
            {
                i = 0;
                var randomIndex = Game.Session.Hub.hubGirlDefinition.hairstyles.Select(x => (x, i++)).Where(x => x.Item1 != null).ToArray().GetRandom().Item2;
                hubStyle.HairstyleId = girlExpansion.HairstyleLookup[randomIndex];
            }

            var args = ModInterface.Events.NotifyRequestStyleChange(Game.Session.Hub.hubGirlDefinition, currentLocation, 0.1f, hubStyle);

            if (args.ApplyChance > 0f
                && args.ApplyChance <= 100f
                && UnityEngine.Random.Range(0f, 1f) <= args.ApplyChance)
            {
                args.Style.Apply(Game.Session.gameCanvas.dollRight,
                    Game.Session.Hub.hubGirlDefinition.defaultOutfitIndex,
                    Game.Session.Hub.hubGirlDefinition.defaultHairstyleIndex);
            }
        }
        else
        {
            var currentGirlPair = f_currentGirlPair.GetValue(_core) as GirlPairDefinition;
            var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(currentGirlPair);

            if (playerFileGirlPair == null) { return; }

            var flipped = (bool)f_currentSidesFlipped.GetValue(_core);
            var leftGirlDef = flipped ? currentGirlPair.girlDefinitionTwo : currentGirlPair.girlDefinitionOne;
            var rightGirlDef = flipped ? currentGirlPair.girlDefinitionOne : currentGirlPair.girlDefinitionTwo;

            var locationExp = currentLocation.Expansion();

            GirlStyleInfo leftStyle = null;
            GirlStyleInfo rightStyle = null;

            if (locationExp.DefaultStyle.HasValue)
            {
                leftStyle = rightStyle = new GirlStyleInfo(locationExp.DefaultStyle.Value);
            }

            if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, currentGirlPair.id);
                var pairStyle = ExpandedGirlPairDefinition.Get(pairId).PairStyle;

                if (pairStyle != null)
                {
                    leftStyle = flipped ? pairStyle.MeetingGirlTwo : pairStyle.MeetingGirlOne;
                    rightStyle = flipped ? pairStyle.MeetingGirlOne : pairStyle.MeetingGirlTwo;
                    ModInterface.Log.Message($"Using Pair Meeting Styles. Left {leftStyle}. Right {rightStyle}");
                }
            }
            else if (_core.AtLocationType(LocationType.DATE))
            {
                var args = new PreDateDollResetArgs();

                if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                    && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime)
                {
                    args.Style = PreDateDollResetArgs.StyleType.Sex;
                }
                else if (!Game.Session.Puzzle.puzzleStatus.isEmpty
                    && currentLocation != Game.Session.Puzzle.bossLocationDefinition)
                {
                    args.Style = PreDateDollResetArgs.StyleType.Location;
                }
                else
                {
                    args.Style = PreDateDollResetArgs.StyleType.File;
                }

                ModInterface.Events.NotifyPreDateDollReset(args);

                if (args.Style == PreDateDollResetArgs.StyleType.Sex)
                {
                    var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, currentGirlPair.id);
                    var pairStyle = ExpandedGirlPairDefinition.Get(pairId).PairStyle;

                    if (pairStyle != null)
                    {
                        leftStyle = flipped ? pairStyle.SexGirlTwo : pairStyle.SexGirlOne;
                        rightStyle = flipped ? pairStyle.SexGirlOne : pairStyle.SexGirlTwo;
                        ModInterface.Log.Message($"Using Pair Sex Styles. Left {leftStyle}. Right {rightStyle}");
                    }
                }
                else if (args.Style == PreDateDollResetArgs.StyleType.Location)
                {
                    var locationId = ModInterface.Data.GetDataId(GameDataType.Location, currentLocation.id);

                    if (!Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.stylesOnDates)
                    {
                        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, leftGirlDef.id);
                        var girlExpansion = ExpandedGirlDefinition.Get(girlId);
                        if (girlExpansion.GetCurrentBody().LocationIdToOutfitId.TryGetValue(locationId, out var girlStyle))
                        {
                            leftStyle = girlStyle;
                        }
                    }

                    if (!Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.stylesOnDates)
                    {
                        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, rightGirlDef.id);
                        var girlExpansion = ExpandedGirlDefinition.Get(girlId);
                        if (girlExpansion.GetCurrentBody().LocationIdToOutfitId.TryGetValue(locationId, out var girlStyle))
                        {
                            rightStyle = girlStyle;
                        }
                    }
                }
            }

            var leftArgs = ModInterface.Events.NotifyRequestStyleChange(leftGirlDef, currentLocation, 0, leftStyle);
            var rightArgs = ModInterface.Events.NotifyRequestStyleChange(rightGirlDef, currentLocation, 0, rightStyle);

            if (UnityEngine.Random.Range(0f, 1f) <= leftArgs.ApplyChance)
            {
                leftStyle = leftArgs.Style;
            }

            if (UnityEngine.Random.Range(0f, 1f) <= rightArgs.ApplyChance)
            {
                rightStyle = rightArgs.Style;
            }

            leftStyle?.Apply(Game.Session.gameCanvas.dollLeft, leftGirlDef.defaultOutfitIndex, leftGirlDef.defaultHairstyleIndex);
            rightStyle?.Apply(Game.Session.gameCanvas.dollRight, rightGirlDef.defaultOutfitIndex, rightGirlDef.defaultHairstyleIndex);
        }
    }

    public void RefreshUi()
    {
        OnRefreshUi?.Invoke();
    }

    internal void OnDestroy()
    {
        _expansions.Remove(_core);
    }
}
