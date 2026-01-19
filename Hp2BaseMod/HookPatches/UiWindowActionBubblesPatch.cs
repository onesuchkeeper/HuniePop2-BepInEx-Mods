using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiWindowActionBubbles))]
internal static class UiWindowActionBubblesPatch
{
    private static readonly FieldInfo f_selectedBubble = AccessTools.Field(typeof(UiWindowActionBubbles), "_selectedBubble");

    [HarmonyPatch("OnActionBubblePressed")]
    [HarmonyPrefix]
    private static bool OnActionBubblePressed(UiWindowActionBubbles __instance, UiActionBubble actionBubble)
    {
        if (actionBubble.actionBubbleType != ActionBubbleType.DATE)
        {
            return true;
        }

        var args = new DateLocationSelectedArgs()
        {
            PlayerPoints = 10,
            LeftPoints = 5,
            RightPoints = 5,

            LeftStaminaGain = 2,
            RightStaminaGain = 2,
            DenyDate = false
        };

        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);
        if (playerFileGirlPair != null && playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
            && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime)
        {
            ModInterface.Log.Message("Pair is targeting sex location");
            args.Location = playerFileGirlPair.girlPairDefinition.sexLocationDefinition;
        }

        ModInterface.Events.NotifyDateLocationSelected(args);

        if (args.DenyDate)
        {
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxReject, null);
            if (Game.Manager.Windows.IsWindowActive(null, includeShowing: true, includeHiding: false))
            {
                Game.Manager.Windows.ShowWindow(Game.Session.Location.actionBubblesWindow, shouldQueue: true);
                Game.Manager.Windows.HideWindow();
            }

            return false;
        }

        var staminaEnergy = Game.Data.Tokens.GetByResourceType(PuzzleResourceType.STAMINA).energyDefinition;
        if (args.LeftStaminaGain != 0
            && Game.Session.Puzzle.puzzleStatus.girlStatusLeft.stamina < 6)
        {
            var value = Mathf.Min(6 - Game.Session.Puzzle.puzzleStatus.girlStatusLeft.stamina, args.LeftStaminaGain);

            Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA,
                value,
                false);

            Object.Instantiate(Game.Session.Talk.energyTrailPrefab)
                .Init(EnergyTrailFormat.START_AND_END,
                    staminaEnergy,
                    null,
                    Game.Session.gameCanvas.GetDoll(altOrientation: false),
                    $"+{value} Stamina");
        }

        if (args.RightStaminaGain != 0
            && Game.Session.Puzzle.puzzleStatus.girlStatusRight.stamina < 6)
        {
            int value = Mathf.Min(6 - Game.Session.Puzzle.puzzleStatus.girlStatusRight.stamina, args.RightStaminaGain);

            Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA,
                value,
                true);

            Object.Instantiate(Game.Session.Talk.energyTrailPrefab)
                .Init(EnergyTrailFormat.START_AND_END,
                    staminaEnergy,
                    null,
                    Game.Session.gameCanvas.GetDoll(altOrientation: true),
                    $"+{value} Stamina");
        }

        Game.Session.Puzzle.puzzleStatus.CheckChanges();
        Game.Persistence.playerFile.staminaFoodLimit++;
        Game.Persistence.playerFile.relationshipPoints += args.PlayerPoints;
        Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.relationshipPoints += args.LeftPoints;
        Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.relationshipPoints += args.RightPoints;

        if (args.Location == null)
        {
            var time = (ClockDaytimeType)(Game.Persistence.playerFile.daytimeElapsed % 4);

            var locs = Game.Data.Locations.GetAllByLocationType(LocationType.DATE)
                .Where(x => x.Expansion().IsValidForNormalDate());

            ModInterface.Log.Message($"Choosing normal date Loc from pool: [{string.Join(", ", locs.Select(x => x.locationName))}]");

            args.Location = locs.ToList().PopRandom();
        }

        Game.Session.Location.Depart(args.Location, Game.Session.Location.currentGirlPair, Game.Session.Location.currentSidesFlipped);

        Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxBubbleSelect, __instance.pauseDefinition);
        Game.Manager.Audio.Play(AudioCategory.SOUND, actionBubble.sfxSelect, __instance.pauseDefinition);

        f_selectedBubble.SetValue(__instance, null);

        return false;
    }
}
