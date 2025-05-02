using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Utility;

namespace Hp2BaseModTweaks;

[HarmonyPatch(typeof(PuzzleManager), "OnRoundOver")]
internal static class PuzzleManager_OnRoundOver
{
    private static PuzzleStatus GetPuzzleStatus(this PuzzleManager puzzleManager) => (PuzzleStatus)x_puzzleStatus.GetValue(puzzleManager);
    private static FieldInfo x_puzzleStatus = AccessTools.Field(typeof(PuzzleManager), "_puzzleStatus");

    private static SaveFile GetSaveDataFile(this GamePersistence gamePersistence) => ((SaveData)x_saveData.GetValue(gamePersistence)).files[(int)x_loadedFileIndex.GetValue(gamePersistence)];
    private static FieldInfo x_saveData = AccessTools.Field(typeof(GamePersistence), "_saveData");
    private static FieldInfo x_loadedFileIndex = AccessTools.Field(typeof(GamePersistence), "_loadedFileIndex");

    public static void Postfix(PuzzleManager __instance)
    {
        var status = __instance.GetPuzzleStatus();

        if (status.statusType == PuzzleStatusType.BOSS
            && status.bonusRound
            && Game.Persistence.playerFile.storyProgress >= 12)
        {
            var moxieFile = GetPlayerFileGirl(Game.Persistence.playerFile, ModInterface.GameData.GetGirl(Girls.MoxieId));
            moxieFile.playerMet = true;
            moxieFile.UnlockOutfit(0);
            moxieFile.UnlockHairstyle(0);
            var unlocked = StyleUnlockUtility.UnlockRandomStyle(moxieFile, false);

            var jewnFile = GetPlayerFileGirl(Game.Persistence.playerFile, ModInterface.GameData.GetGirl(Girls.JewnId));
            jewnFile.playerMet = true;
            jewnFile.UnlockOutfit(0);
            jewnFile.UnlockHairstyle(0);
            StyleUnlockUtility.UnlockRandomStyle(jewnFile, unlocked);

            var kyuFile = GetPlayerFileGirl(Game.Persistence.playerFile, ModInterface.GameData.GetGirl(Girls.KyuId));
            kyuFile.playerMet = true;
            kyuFile.UnlockOutfit(1);
            kyuFile.UnlockHairstyle(1);
            StyleUnlockUtility.UnlockRandomStyle(kyuFile, true);
        }
    }

    private static void UnlockRandomStyle(GirlDefinition def, PlayerFileGirl playerFileGirl)
    {
        var lockedOutfitIndexes = new List<int>();
        for (var i = 0; i < def.outfits.Count; i++)
        {
            if (!playerFileGirl.unlockedOutfits.Contains(i))
            {
                lockedOutfitIndexes.Add(i);
            }
        }

        int unlockedOutfit = -1;
        if (lockedOutfitIndexes.Any())
        {
            unlockedOutfit = lockedOutfitIndexes[UnityEngine.Random.Range(0, lockedOutfitIndexes.Count - 1)];
            playerFileGirl.UnlockOutfit(unlockedOutfit);
        }

        if (unlockedOutfit != -1 && unlockedOutfit < def.hairstyles.Count)
        {
            playerFileGirl.UnlockHairstyle(unlockedOutfit);
        }
        else
        {
            var lockedHairstyleIndexes = new List<int>();
            for (var i = 0; i < def.hairstyles.Count; i++)
            {
                if (!playerFileGirl.unlockedHairstyles.Contains(i))
                {
                    lockedHairstyleIndexes.Add(i);
                }
            }

            if (lockedHairstyleIndexes.Any())
            {
                playerFileGirl.UnlockHairstyle(lockedHairstyleIndexes[UnityEngine.Random.Range(0, lockedHairstyleIndexes.Count - 1)]);
            }
        }
    }

    private static PlayerFileGirl GetPlayerFileGirl(PlayerFile playerFile, GirlDefinition girlDef)
    {
        if (girlDef == null)
        {
            return null;
        }

        for (int i = 0; i < playerFile.girls.Count; i++)
        {
            if (playerFile.girls[i].girlDefinition == girlDef)
            {
                return playerFile.girls[i];
            }
        }

        PlayerFileGirl playerFileGirl = new PlayerFileGirl(girlDef);
        playerFileGirl.hairstyleIndex = girlDef.defaultHairstyleIndex;
        playerFileGirl.outfitIndex = girlDef.defaultOutfitIndex;

        // if (!girlDef.specialCharacter)
        // {
        playerFile.girls.Add(playerFileGirl);
        // }

        return playerFileGirl;
    }
}