using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod.EnumExpansion
{
    [HarmonyPatch(typeof(LocationManager), nameof(LocationManager.ResetDolls))]
    public static class LocationManager_ResetDolls
    {
        private static readonly FieldInfo _currentGirlPair = AccessTools.Field(typeof(LocationManager), "_currentGirlPair");
        private static readonly FieldInfo _currentSidesFlipped = AccessTools.Field(typeof(LocationManager), "_currentSidesFlipped");
        private static readonly FieldInfo _currentLocation = AccessTools.Field(typeof(LocationManager), "_currentLocation");

        public static void Postfix(LocationManager __instance, bool unload = false)
        {
            if (unload) { return; }

            var currentLocation = _currentLocation.GetValue(__instance) as LocationDefinition;

            if (currentLocation.locationType == LocationType.HUB)
            {
                var args = ModInterface.Events.NotifyRequestStyleChange(Game.Session.Hub.hubGirlDefinition, currentLocation, 0f, GirlStyleInfo.Default);

                if (UnityEngine.Random.Range(0f, 1f) <= args.ApplyChance)
                {
                    ApplyStyleInfo(Game.Session.gameCanvas.dollRight,
                        args.Style,
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
                    var pairStyleInfo = ModInterface.Data.GetPairStyleInfo(pairId);

                    leftStyle = pairStyleInfo.MeetingGirlOne;
                    rightStyle = pairStyleInfo.MeetingGirlTwo;
                }
                else if (currentLocation.locationType == LocationType.DATE)
                {
                    if (playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                        && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime)
                    {
                        var pairId = ModInterface.Data.GetDataId(GameDataType.GirlPair, currentGirlPair.id);
                        var pairStyleInfo = ModInterface.Data.GetPairStyleInfo(pairId);

                        leftStyle = pairStyleInfo.SexGirlOne;
                        rightStyle = pairStyleInfo.SexGirlTwo;
                    }
                    else if (!Game.Session.Puzzle.puzzleStatus.isEmpty
                        && currentLocation != Game.Session.Puzzle.bossLocationDefinition)
                    {
                        var locationId = ModInterface.Data.GetDataId(GameDataType.Location, currentLocation.id);

                        if (!Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.stylesOnDates)
                        {
                            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, leftGirlDef.id);

                            if (ModInterface.Data.TryGetLocationStyleInfo(locationId, girlId, out var leftStyleOverride))
                            {
                                leftStyle = leftStyleOverride;
                            }
                        }

                        if (!Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.stylesOnDates)
                        {
                            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, rightGirlDef.id);

                            if (ModInterface.Data.TryGetLocationStyleInfo(locationId, girlId, out var rightStyleOverride))
                            {
                                rightStyle = rightStyleOverride;
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

                ApplyStyleInfo(Game.Session.gameCanvas.dollLeft, leftStyle, leftGirlDef.defaultOutfitIndex, leftGirlDef.defaultHairstyleIndex);
                ApplyStyleInfo(Game.Session.gameCanvas.dollRight, rightStyle, rightGirlDef.defaultOutfitIndex, rightGirlDef.defaultHairstyleIndex);
            }
        }

        private static void ApplyStyleInfo(UiDoll doll, GirlStyleInfo girlStyleInfo, int defaultOutfitIndex, int defaultHairstyleIndex)
        {
            if (girlStyleInfo == null
                || !ModInterface.Data.TryGetDataId(GameDataType.Girl, (doll.soulGirlDefinition ?? doll.girlDefinition).id, out var girlId))
            {
                return;
            }

            if (girlStyleInfo.OutfitId.HasValue)
            {
                doll.ChangeOutfit(ModInterface.Data.TryGetOutfitIndex(girlId, girlStyleInfo.OutfitId.Value, out var index)
                    ? index
                    : defaultOutfitIndex);
            }

            if (girlStyleInfo.HairstyleId.HasValue)
            {
                doll.ChangeHairstyle(ModInterface.Data.TryGetHairstyleIndex(girlId, girlStyleInfo.HairstyleId.Value, out var index)
                    ? index
                    : defaultHairstyleIndex);
            }
        }
    }
}
