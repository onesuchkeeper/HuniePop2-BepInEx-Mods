using HarmonyLib;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(UiTitleCanvas), nameof(UiTitleCanvas.LoadGame))]
public static class UiTitleCanvasPatch_LoadGame
{
    public static void Prefix(UiTitleCanvas __instance, int saveFileIndex, string loadSceneName = "MainScene")
    {
        var file = Game.Persistence.playerData.files[saveFileIndex];
        ModInterface.Events.NotifyPreLoadSaveFile(file);

        Game.Persistence.loadedFileIndex = saveFileIndex;

        using (ModInterface.Log.MakeIndent("Initializing bodies"))
        {
            foreach (var girl in Game.Data.Girls.GetAll())
            {
                var body = girl.GetExpansion().GetCurrentBody();

                ModInterface.Log.Message($"{girl.girlName} -> {body.BodyName ?? "null"}");
                body?.Apply(girl);
            }   
        }
    }
}
