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
        foreach (var girl in Game.Data.Girls.GetAll())
        {
            var body = girl.Expansion().GetCurrentBody();

            var bodyName = body == null
                ? "BODY_NULL"
                : body.BodyName;

            ModInterface.Log.LogInfo($"Initializing body {bodyName} for {girl.girlName}");
            body?.Apply(girl);
        }

        if (file.locationDefinition.locationType == LocationType.SPECIAL)
        {
            file.locationDefinition = ModInterface.GameData.GetLocation(Locations.HotelRoom);
            file.girlPairDefinition = null;
        }
    }
}
