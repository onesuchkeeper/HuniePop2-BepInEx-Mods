using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(SaveFile))]
public static class SaveFilePatch
{
    private static readonly FieldInfo _saveData = AccessTools.Field(typeof(GamePersistence), "_saveData");

    [HarmonyPatch(nameof(SaveFile.Reset))]
    [HarmonyPostfix]
    public static void Reset(SaveFile __instance)
    {
        var saveData = _saveData.GetValue<SaveData>(Game.Persistence);
        var fileIndex = saveData.files.IndexOf(__instance);

        if (fileIndex != -1
            && ModInterface.Save.ModFiles.Count > fileIndex)
        {
            ModInterface.Save.ModFiles[fileIndex] = new();
        }
    }
}