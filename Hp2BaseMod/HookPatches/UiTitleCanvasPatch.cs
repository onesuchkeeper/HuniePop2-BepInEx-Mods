using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiTitleCanvas), nameof(UiTitleCanvas.LoadGame))]
public static class UiTitleCanvasPatch_LoadGame
{
    private static FieldInfo _saveData = AccessTools.Field(typeof(GamePersistence), "_saveData");

    public static void Prefix(UiTitleCanvas __instance, int saveFileIndex, string loadSceneName = "MainScene")
    {
        var saveData = (SaveData)_saveData.GetValue(Game.Persistence);

        ModInterface.Events.NotifyPreLoadSaveFile(Game.Persistence.playerData.files[saveFileIndex]);
    }
}
