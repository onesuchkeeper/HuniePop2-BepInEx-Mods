using HarmonyLib;
using UnityEngine;

namespace Hp2BaseMod;

[Expansion(typeof(GiftManager))]
#pragma warning disable HP001 // Deprecated member usage
[Deprecates(nameof(GiftManager.GiftGivenEvent), "Was never used in the base game")]
[Deprecates(nameof(GiftManager.GetRandomFruit), $"Use {nameof(IAffection)}.{nameof(IAffection.GetRandomFruit)} or {nameof(ExpandedGirlDefinition)}.{nameof(ExpandedGirlDefinition.GetRandomFruit)} instead")]
#pragma warning restore HP001 // Deprecated member usage
public partial class ExpandedGiftManager
{
    [HarmonyPatch(typeof(GiftManager))]
    private static class Patch
    {
        [HarmonyPatch(nameof(GiftManager.GiveGift), [typeof(UiDoll), typeof(ItemDefinition)])]
        [HarmonyPrefix]
        private static bool GiveGift(GiftManager __instance, UiDoll doll, ItemDefinition itemDef, ref bool __result) 
            => ExpandedGiftManager.Get(__instance).GiveGift_Prefix(doll, itemDef, ref __result);

        [HarmonyPatch(nameof(GiftManager.GetDirectGiftGiveError))]
        [HarmonyPrefix]
        private static bool GetDirectGiftGiveError_Prefix(GiftManager __instance, ItemDefinition itemDef, ref string __result)
            => ExpandedGiftManager.Get(__instance).GetDirectGiftGiveError_Prefix(itemDef, ref __result);
    }

    private bool GetDirectGiftGiveError_Prefix(ItemDefinition itemDef, ref string __result)
    {
        if (itemDef == null)
        {
            __result = null;
        }
        else if (itemDef.itemType == ItemType.MISC)
        {
            __result = "This item can't be given as a gift.";
        }
        else if (itemDef.itemType == ItemType.DATE_GIFT)
        {
            __result = "Date gifts can't be given directly from the HunieBee.";
        }
        else
        {
            __result = ModInterface.GameState.CurrentState.GetGiftError(itemDef);
        }

        return false;
    }

    private bool GiveGift_Prefix(UiDoll doll, ItemDefinition itemDef, ref bool __result)
    {
        var isAltGirl = doll == Game.Session.gameCanvas.GetDoll(true);
		var girlDefinition = doll.girlDefinition;
		var girlExp = girlDefinition.GetExpansion();
		PlayerFileGirl playerFileGirl = Game.Persistence.playerFile.GetPlayerFileGirl(girlDefinition);
		PuzzleStatusGirl statusGirl = Game.Session.Puzzle.puzzleStatus.GetStatusGirl(isAltGirl);
        var itemExp = itemDef.GetExpansion();

        var canGive = ModInterface.GameState.CurrentState.AllowGifts
            && itemExp.GiftHandler.CanGive(itemExp, girlExp, statusGirl, playerFileGirl, isAltGirl);

        //notify

        if (canGive)
        {
            var splashText = itemExp.GiftHandler.OnGiveSucceed(itemExp, doll, girlExp, statusGirl, playerFileGirl, isAltGirl);
            if (itemDef.energyDefinition != null)
			{
				var mousePosition = Game.Manager.gameCamera.GetMousePosition();
				Object.Instantiate(_core.energyTrailPrefab).Init(EnergyTrailFormat.START_AND_END, 
                    itemDef.energyDefinition, 
                    mousePosition, 
                    doll, 
                    splashText, 
                    null);
			}
			Game.Manager.Audio.Play(AudioCategory.SOUND, _core.sfxGiftSuccess, doll.pauseDefinition);
			Game.Manager.Audio.Play(AudioCategory.SOUND, _core.sfxResourceFlourish, doll.pauseDefinition);
			Game.Session.Puzzle.puzzleStatus.CheckChanges();

            // literally nothing subscribes to this...
            // if (this.GiftGivenEvent != null)
            // {
            //     this.GiftGivenEvent(itemDef);
            // }

			__result = true;
        }
        else
        {
            itemExp.GiftHandler.OnGiveFailed(itemExp, girlExp, doll);
            Game.Manager.Audio.Play(AudioCategory.SOUND, _core.sfxGiftFailure, doll.pauseDefinition);
		    __result = false;
        }
        
		if (ModInterface.GameState.CurrentState.Id == GameStateId.Sim)
		{
			Game.Session.Puzzle.puzzleStatus.SetGirlFocus(isAltGirl);
		}

        return false;
    }
}