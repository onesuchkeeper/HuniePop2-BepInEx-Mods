using System;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using System.Linq;

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
            ModInterface.Log.LogInfo($"Adding special characters to player file");

            var moxieDef = Game.Data.Girls.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, Girls.MoxieId));
            var jewnDef = Game.Data.Girls.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, Girls.JewnId));
            var kyuDef = Game.Data.Girls.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, Girls.KyuId));

            var moxieFile = GetPlayerFileGirl(Game.Persistence.playerFile, moxieDef);
            moxieFile.playerMet = true;
            moxieFile.UnlockOutfit(0);
            moxieFile.UnlockHairstyle(0);

            var jewnFile = GetPlayerFileGirl(Game.Persistence.playerFile, jewnDef);
            jewnFile.playerMet = true;
            jewnFile.UnlockOutfit(0);
            jewnFile.UnlockHairstyle(0);

            var kyuFile = GetPlayerFileGirl(Game.Persistence.playerFile, kyuDef);
            kyuFile.playerMet = true;
            kyuFile.UnlockOutfit(1);
            kyuFile.UnlockHairstyle(1);

            ModInterface.Log.LogInfo($"Moxie met?: {moxieFile.playerMet}");
            ModInterface.Log.LogInfo($"Jewn met?: {jewnFile.playerMet}");
            ModInterface.Log.LogInfo($"Kyu met?: {kyuFile.playerMet}");

            var metGirls = Game.Persistence.playerFile.girls.Where(x => x.playerMet).OrderBy(x => x.girlDefinition.id).ToArray();

            ModInterface.Log.LogInfo($"Met girls count: {metGirls.Length}");
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