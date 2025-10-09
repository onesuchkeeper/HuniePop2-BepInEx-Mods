using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiTitleCanvas), nameof(UiTitleCanvas.LoadGame))]
public static class UiTitleCanvasPatch_LoadGame
{
    public static void Prefix(UiTitleCanvas __instance, int saveFileIndex, string loadSceneName = "MainScene")
    {
        ModInterface.Events.NotifyPreLoadSaveFile(Game.Persistence.playerData.files[saveFileIndex]);
    }
}
