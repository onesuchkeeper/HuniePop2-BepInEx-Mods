using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(GiftManager))]
public static class GiftManagerPatch
{
    [HarmonyPatch(nameof(GiftManager.GiveGift), [typeof(UiDoll), typeof(ItemDefinition)])]
    [HarmonyPrefix]
    public static bool GiveGift(GiftManager __instance, UiDoll doll, ItemDefinition itemDef, ref bool __result)
    {
        var id = ModInterface.Data.GetDataId(GameDataType.Item, itemDef.id);

        if (id != ItemSensitivitySmoothie.SmoothieId
            || !Game.Session.Location.AtLocationType(LocationType.SIM))
        {
            return true;
        }

        var altGirl = doll == Game.Session.gameCanvas.GetDoll(true);

        var girlDefinition = doll.girlDefinition;
        var playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDefinition);
        var statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(altGirl);
        __result = true;
        string text = null;

        Game.Session.Puzzle.puzzleStatus.SetGirlFocus(altGirl);

        if (!itemDef.noStaminaCost && statusGirl.stamina <= 0)
        {
            __result = false;
        }
        else if (itemDef.itemType == ItemType.FOOD
            && (!__instance.CheckGiveCondition(playerFileGirl, itemDef) || Game.Session.Puzzle.puzzleStatus.foodGiftCount >= 2))
        {
            __result = false;
        }

        if (__result)
        {
            if (State.Save.SensitivityExp < 24)
            {
                text = $"+1 Sensitivity EXP";

                var affectionLevel = State.Save.SensitivityExp / 6;

                State.Save.SensitivityExp++;

                var updatedAffectionLevel = State.Save.SensitivityExp / 6;
                if (updatedAffectionLevel != affectionLevel)
                {
                    doll.notificationBox.Show($"Sensitivity Level {updatedAffectionLevel} achieved!", 0f, false);
                }

                doll.ReadDialogTrigger(__instance.dtSmoothieAccept, DialogLineFormat.PASSIVE, -1);
            }
            else
            {
                doll.ReadDialogTrigger(__instance.dtSmoothieFull, DialogLineFormat.PASSIVE, -1);
                __result = false;
            }

            if (__result)
            {
                Game.Persistence.playerFile.relationshipPoints++;
                playerFileGirl.relationshipPoints++;

                if (!itemDef.noStaminaCost)
                {
                    Game.Session.Puzzle.puzzleStatus.AddResourceValue(PuzzleResourceType.STAMINA, -1, altGirl);
                }

                if (itemDef.energyDefinition != null)
                {
                    Object.Instantiate(__instance.energyTrailPrefab)
                        .Init(EnergyTrailFormat.START_AND_END, itemDef.energyDefinition, Game.Manager.gameCamera.GetMousePosition(), doll, text, null);
                }

                Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxGiftSuccess, doll.pauseDefinition);
                Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxResourceFlourish, doll.pauseDefinition);
                Game.Session.Puzzle.puzzleStatus.CheckChanges();

                // literally nothing subscribes to this...
                // if (this.GiftGivenEvent != null)
                // {
                //     this.GiftGivenEvent(itemDef);
                // }
            }
            else
            {
                Game.Manager.Audio.Play(AudioCategory.SOUND, __instance.sfxGiftFailure, doll.pauseDefinition);
            }
        }

        return false;
    }
}
