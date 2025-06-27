using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(LocationManager))]
internal static class LocationManagerPatch
{
    private static FieldInfo _isLocked = AccessTools.Field(typeof(LocationManager), "_isLocked");
    private static FieldInfo _arrivalCutscene = AccessTools.Field(typeof(LocationManager), "_arrivalCutscene");
    private static readonly FieldInfo _currentGirlPair = AccessTools.Field(typeof(LocationManager), "_currentGirlPair");
    private static readonly FieldInfo _currentSidesFlipped = AccessTools.Field(typeof(LocationManager), "_currentSidesFlipped");
    private static readonly FieldInfo _currentLocation = AccessTools.Field(typeof(LocationManager), "_currentLocation");

    [HarmonyPatch("OnLocationSettled")]
    [HarmonyPrefix]
    private static bool OnLocationSettled(LocationManager __instance)
    {
        if (__instance.currentLocation.locationType != LocationType.SIM)
        {
            return true;
        }

        var arrivalCutscene = _arrivalCutscene.GetValue<CutsceneDefinition>(__instance);

        if (arrivalCutscene != null)
        {
            return true;
        }

        _isLocked.SetValue(__instance, false);

        Game.Session.Logic.ProcessBundleList(__instance.currentLocation.departBundleList, false);

        Game.Manager.Windows.ShowWindow(__instance.actionBubblesWindow, false);

        var args = new RandomDollSelectedArgs();
        ModInterface.Events.NotifyRandomDollSelected(args);
        var uiDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

        var greetingIndex = Mathf.Clamp(Game.Persistence.playerFile.daytimeElapsed % 4, 0, __instance.dtGreetings.Length - 1);

        uiDoll.ReadDialogTrigger(__instance.dtGreetings[greetingIndex], DialogLineFormat.PASSIVE, -1);

        _arrivalCutscene.SetValue(__instance, null);
        return false;
    }

    [HarmonyPatch(nameof(LocationManager.ResetDolls))]
    [HarmonyPostfix]
    public static void ResetDolls(LocationManager __instance, bool unload = false)
    {
        if (unload) { return; }

        var currentLocation = _currentLocation.GetValue(__instance) as LocationDefinition;

        if (currentLocation.locationType == LocationType.HUB)
        {
            // the base method already randomizes hub girl outfit, so just use default
            var girlExpansion = Game.Session.Hub.hubGirlDefinition.Expansion();

            var outfitIndex = UnityEngine.Random.Range(0, Game.Session.Hub.hubGirlDefinition.outfits.Count);
            var outfit = Game.Session.Hub.hubGirlDefinition.outfits[outfitIndex];

            var hubStyle = new GirlStyleInfo()
            {
                OutfitId = girlExpansion.OutfitIndexToId[outfitIndex],
            };

            if (outfit.pairHairstyleIndex != -1)
            {
                hubStyle.HairstyleId = girlExpansion.HairstyleIndexToId[outfit.pairHairstyleIndex];
            }
            else
            {
                hubStyle.HairstyleId = girlExpansion.HairstyleIndexToId[UnityEngine.Random.Range(0, Game.Session.Hub.hubGirlDefinition.hairstyles.Count)];
            }

            var args = ModInterface.Events.NotifyRequestStyleChange(Game.Session.Hub.hubGirlDefinition, currentLocation, 0.1f, new GirlStyleInfo());

            if (UnityEngine.Random.Range(0f, 1f) <= args.ApplyChance)
            {
                args.Style.Apply(Game.Session.gameCanvas.dollRight,
                    Game.Session.Hub.hubGirlDefinition.defaultOutfitIndex,
                    Game.Session.Hub.hubGirlDefinition.defaultHairstyleIndex);
            }
        }
        else
        {
            var currentGirlPair = _currentGirlPair.GetValue(__instance) as GirlPairDefinition;
            var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(currentGirlPair);

            if (playerFileGirlPair == null) { return; }

            var flipped = (bool)_currentSidesFlipped.GetValue(__instance);
            var leftGirlDef = flipped ? currentGirlPair.girlDefinitionTwo : currentGirlPair.girlDefinitionOne;
            var rightGirlDef = flipped ? currentGirlPair.girlDefinitionOne : currentGirlPair.girlDefinitionTwo;

            GirlStyleInfo leftStyle = null;
            GirlStyleInfo rightStyle = null;

            if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.UNKNOWN)
            {
                var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, currentGirlPair.id);
                var pairStyle = ExpandedGirlPairDefinition.Get(pairId).PairStyle;

                if (pairStyle != null)
                {
                    leftStyle = pairStyle.MeetingGirlOne;
                    rightStyle = pairStyle.MeetingGirlTwo;
                }
            }
            else if (currentLocation.locationType == LocationType.DATE)
            {
                if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                    && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime)
                {
                    var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, currentGirlPair.id);
                    var pairStyle = ExpandedGirlPairDefinition.Get(pairId).PairStyle;

                    if (pairStyle != null)
                    {
                        leftStyle = pairStyle.SexGirlOne;
                        rightStyle = pairStyle.SexGirlTwo;
                    }
                }
                else if (!Game.Session.Puzzle.puzzleStatus.isEmpty
                    && currentLocation != Game.Session.Puzzle.bossLocationDefinition)
                {
                    var locationId = ModInterface.Data.GetDataId(GameDataType.Location, currentLocation.id);

                    if (!Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.stylesOnDates)
                    {
                        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, leftGirlDef.id);
                        ExpandedLocationDefinition.Get(locationId).GirlIdToLocationStyleInfo?.TryGetValue(girlId, out leftStyle);
                    }

                    if (!Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.stylesOnDates)
                    {
                        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, rightGirlDef.id);
                        ExpandedLocationDefinition.Get(locationId).GirlIdToLocationStyleInfo?.TryGetValue(girlId, out rightStyle);
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
}
