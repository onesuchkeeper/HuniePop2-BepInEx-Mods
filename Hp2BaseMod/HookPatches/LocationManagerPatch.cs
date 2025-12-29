using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(LocationManager))]
internal static class LocationManagerPatch
{
    private static readonly FieldInfo f_isLocked = AccessTools.Field(typeof(LocationManager), "_isLocked");
    private static readonly FieldInfo f_arrivalCutscene = AccessTools.Field(typeof(LocationManager), "_arrivalCutscene");
    private static readonly FieldInfo f_currentGirlPair = AccessTools.Field(typeof(LocationManager), "_currentGirlPair");
    private static readonly FieldInfo f_currentSidesFlipped = AccessTools.Field(typeof(LocationManager), "_currentSidesFlipped");
    private static readonly FieldInfo f_currentLocation = AccessTools.Field(typeof(LocationManager), "_currentLocation");

    [HarmonyPatch("OnLocationSettled")]
    [HarmonyPrefix]
    private static bool OnLocationSettled(LocationManager __instance)
    {
        if (__instance.currentLocation.locationType != LocationType.SIM)
        {
            return true;
        }

        var arrivalCutscene = f_arrivalCutscene.GetValue<CutsceneDefinition>(__instance);

        if (arrivalCutscene != null)
        {
            return true;
        }

        f_isLocked.SetValue(__instance, false);

        Game.Session.Logic.ProcessBundleList(__instance.currentLocation.departBundleList, false);

        Game.Manager.Windows.ShowWindow(__instance.actionBubblesWindow, false);

        var args = new RandomDollSelectedArgs();
        ModInterface.Events.NotifyRandomDollSelected(args);
        var uiDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

        var greetingIndex = Mathf.Clamp(Game.Persistence.playerFile.daytimeElapsed % 4, 0, __instance.dtGreetings.Length - 1);

        uiDoll.ReadDialogTrigger(__instance.dtGreetings[greetingIndex], DialogLineFormat.PASSIVE, -1);

        f_arrivalCutscene.SetValue(__instance, null);
        return false;
    }

    [HarmonyPatch(nameof(LocationManager.ResetDolls))]
    [HarmonyPostfix]
    public static void ResetDolls(LocationManager __instance, bool unload = false)
    {
        if (unload) { return; }//if the hub girl does not have the needed indexes

        var currentLocation = f_currentLocation.GetValue(__instance) as LocationDefinition;

        if (__instance.AtLocationType(LocationType.HUB))
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

            if (UnityEngine.Random.Range(0f, 1f) <= args.ApplyChance)
            {
                args.Style.Apply(Game.Session.gameCanvas.dollRight,
                    Game.Session.Hub.hubGirlDefinition.defaultOutfitIndex,
                    Game.Session.Hub.hubGirlDefinition.defaultHairstyleIndex);
            }
        }
        else
        {
            var currentGirlPair = f_currentGirlPair.GetValue(__instance) as GirlPairDefinition;
            var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(currentGirlPair);

            if (playerFileGirlPair == null) { return; }

            var flipped = (bool)f_currentSidesFlipped.GetValue(__instance);
            var leftGirlDef = flipped ? currentGirlPair.girlDefinitionTwo : currentGirlPair.girlDefinitionOne;
            var rightGirlDef = flipped ? currentGirlPair.girlDefinitionOne : currentGirlPair.girlDefinitionTwo;

            var locationExp = currentLocation.Expansion();

            GirlStyleInfo leftStyle = new GirlStyleInfo(locationExp.DefaultStyle);
            GirlStyleInfo rightStyle = leftStyle;

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
            else if (__instance.AtLocationType(LocationType.DATE))
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
                        leftStyle = pairStyle.SexGirlOne;
                        rightStyle = pairStyle.SexGirlTwo;
                    }
                }
                else if (args.Style == PreDateDollResetArgs.StyleType.Location)
                {
                    var locationId = ModInterface.Data.GetDataId(GameDataType.Location, currentLocation.id);

                    if (!Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.stylesOnDates)
                    {
                        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, leftGirlDef.id);
                        var girlExpansion = ExpandedGirlDefinition.Get(girlId);
                        girlExpansion.GetCurrentBody().LocationIdToOutfitId.TryGetValue(locationId, out leftStyle);
                    }

                    if (!Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.stylesOnDates)
                    {
                        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, rightGirlDef.id);
                        var girlExpansion = ExpandedGirlDefinition.Get(girlId);
                        girlExpansion.GetCurrentBody().LocationIdToOutfitId.TryGetValue(locationId, out rightStyle);
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
