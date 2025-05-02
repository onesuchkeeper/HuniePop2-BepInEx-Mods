using System.Reflection;
using HarmonyLib;

namespace SingleDate;

[HarmonyPatch(typeof(UiAppSaveFile))]
internal static class UiAppSaveFilePatch
{
    private static readonly FieldInfo _playerFile = AccessTools.Field(typeof(UiAppSaveFile), "_playerFile");

    [HarmonyPatch(nameof(UiAppSaveFile.Refresh))]
    [HarmonyPostfix]
    public static void Refresh(UiAppSaveFile __instance)
    {
        // the bitPair uses both girls in the pair, this replaces it with just girlTwo's name for single pairs
        var playerFile = (PlayerFile)_playerFile.GetValue(__instance);

        if (!playerFile.started
            || !State.IsSingle(playerFile.girlPairDefinition))
        {
            return;
        }

        __instance.bitPair.Populate(playerFile.girlPairDefinition.girlDefinitionTwo.girlName);
    }
}
