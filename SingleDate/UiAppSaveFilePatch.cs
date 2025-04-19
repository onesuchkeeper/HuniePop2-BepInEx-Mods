using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppSaveFile))]
public static class UiAppSaveFilePatch
{
    private static readonly FieldInfo _playerFile = AccessTools.Field(typeof(UiAppSaveFile), "_playerFile");

    [HarmonyPatch(nameof(UiAppSaveFile.Refresh))]
    [HarmonyPostfix]
    public static void Refresh(UiAppSaveFile __instance)
    {
        var playerFile = (PlayerFile)_playerFile.GetValue(__instance);

        if (!playerFile.started
            || !State.IsSingle(playerFile.girlPairDefinition))
        {
            return;
        }

        __instance.bitPair.Populate(playerFile.girlPairDefinition.girlDefinitionTwo.girlName);
    }
}
