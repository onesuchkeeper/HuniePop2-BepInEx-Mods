using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(UiWindowActionBubbles))]
internal static class UiWindowActionBubblesPatch
{
    private static FieldInfo _highlightedBubble = AccessTools.Field(typeof(UiWindowActionBubbles), "_highlightedBubble");
    private static FieldInfo _selectedBubble = AccessTools.Field(typeof(UiWindowActionBubbles), "_selectedBubble");

    [HarmonyPatch("OnActionBubbleEnter")]
    [HarmonyPrefix]
    private static bool OnActionBubbleEnter(UiWindowActionBubbles __instance, UiActionBubble actionBubble)
    {
        //Show stamina hint for talking, since it still is used to limit how much the player can talk at a sim location
        //Hide hint for date, since it doesn't apply to single dates.
        if (State.IsSingleDate
            && (actionBubble.actionBubbleType == ActionBubbleType.DATE || actionBubble.actionBubbleType == ActionBubbleType.TALK))
        {
            _highlightedBubble.SetValue(__instance, actionBubble);

            if (actionBubble.actionBubbleType == ActionBubbleType.TALK)
            {
                Game.Session.Talk.ShowStaminaHint(2, 2);
            }

            Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxBubbleOver, null).audioSource.pitch = Random.Range(1f, 2f);

            return false;
        }

        return true;
    }

    [HarmonyPatch("OnActionBubblePressed")]
    [HarmonyPrefix]
    private static bool OnActionBubblePressed(UiWindowActionBubbles __instance, UiActionBubble actionBubble)
    {
        //go to random location when attracted but not single sex date
        if (actionBubble.actionBubbleType != ActionBubbleType.DATE
            || !(State.IsSingleDate || State.RequireLoversBeforeThreesome))
        {
            return true;
        }

        bool sexDate;
        var playerFileGirlPair = Game.Persistence.playerFile.GetPlayerFileGirlPair(Game.Session.Location.currentGirlPair);

        if (playerFileGirlPair == null)
        {
            return true;
        }

        if (!State.IsSingleDate)
        {
            TokenDefinition byResourceType = Game.Data.Tokens.GetByResourceType(PuzzleResourceType.STAMINA, PuzzleAffectionType.TALENT);
            if (Game.Session.Puzzle.puzzleStatus.girlStatusLeft.stamina < 6)
            {
                var num = Mathf.Min(6 - Game.Session.Puzzle.puzzleStatus.girlStatusLeft.stamina, 2);
                Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA, num, false);
                Object.Instantiate(Game.Session.Talk.energyTrailPrefab)
                    .Init(EnergyTrailFormat.START_AND_END, byResourceType.energyDefinition, null, Game.Session.gameCanvas.GetDoll(false), "+" + num + " Stamina");
            }

            if (Game.Session.Puzzle.puzzleStatus.girlStatusRight.stamina < 6)
            {
                var num2 = Mathf.Min(6 - Game.Session.Puzzle.puzzleStatus.girlStatusRight.stamina, 2);
                Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA, num2, true);
                Object.Instantiate(Game.Session.Talk.energyTrailPrefab)
                    .Init(EnergyTrailFormat.START_AND_END, byResourceType.energyDefinition, null, Game.Session.gameCanvas.GetDoll(true), "+" + num2 + " Stamina");
            }

            Game.Persistence.playerFile.staminaFoodLimit++;
            Game.Persistence.playerFile.relationshipPoints += 10;
            Game.Session.Puzzle.puzzleStatus.girlStatusLeft.playerFileGirl.relationshipPoints += 5;
            Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.relationshipPoints += 5;

            var girlSaveOne = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionOne.id);
            var girlSaveTwo = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            sexDate = girlSaveOne?.RelationshipLevel == State.MaxSingleGirlRelationshipLevel
                && girlSaveTwo?.RelationshipLevel == State.MaxSingleGirlRelationshipLevel
                && playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime;
        }
        else
        {
            Game.Persistence.playerFile.staminaFoodLimit++;
            Game.Persistence.playerFile.relationshipPoints += 5;
            Game.Session.Puzzle.puzzleStatus.girlStatusRight.playerFileGirl.relationshipPoints += 5;

            var girlSave = State.SaveFile.GetGirl(playerFileGirlPair.girlPairDefinition.girlDefinitionTwo.id);

            sexDate = playerFileGirlPair.relationshipType == GirlPairRelationshipType.ATTRACTED
                && girlSave.RelationshipLevel == State.MaxSingleGirlRelationshipLevel - 1
                && Game.Persistence.playerFile.daytimeElapsed % 4 == (int)playerFileGirlPair.girlPairDefinition.sexDaytime;
        }

        Game.Session.Puzzle.puzzleStatus.CheckChanges();

        if (sexDate)
        {
            Game.Session.Location.Depart(playerFileGirlPair.girlPairDefinition.sexLocationDefinition,
                Game.Session.Location.currentGirlPair,
                Game.Session.Location.currentSidesFlipped);
        }
        else
        {
            var dateLocationsInfo = Game.Session.Location.dateLocationsInfos[Game.Persistence.playerFile.daytimeElapsed % 4];

            Game.Session.Location.Depart(dateLocationsInfo.locationDefinitions[Random.Range(0, dateLocationsInfo.locationDefinitions.Length)],
                Game.Session.Location.currentGirlPair,
                Game.Session.Location.currentSidesFlipped);
        }

        Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxBubbleSelect, __instance.pauseDefinition);
        Game.Manager.Audio.Play(AudioCategory.SOUND,
            actionBubble.sfxSelect,
            (actionBubble.actionBubbleType != ActionBubbleType.PHONE)
                ? __instance.pauseDefinition
                : null);

        //normally this field gets assigned at the start of the function, then set to null at the end... so just set it to null at the end for consistency
        _selectedBubble.SetValue(__instance, null);

        return false;
    }
}
