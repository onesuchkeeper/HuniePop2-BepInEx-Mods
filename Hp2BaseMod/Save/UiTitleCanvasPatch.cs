using System.Reflection;
using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiTitleCanvas), nameof(UiTitleCanvas.LoadGame))]
public static class UiTitleCanvasPatch_LoadGame
{
    public static void Prefix(UiTitleCanvas __instance, int saveFileIndex, string loadSceneName = "MainScene")
    {
        ModInterface.Events.NotifyPreLoadSaveFile(Game.Persistence.playerData.files[saveFileIndex]);

        Game.Persistence.loadedFileIndex = saveFileIndex;
        foreach (var girl in Game.Data.Girls.GetAll())
        {
            var body = girl.Expansion().GetCurrentBody();

            var bodyName = body == null
                ? "BODY_NULL"
                : body.BodyName;

            ModInterface.Log.LogInfo($"Initializing body {bodyName} for {girl.girlName}");
            body?.Apply(girl);
        }
    }
}
