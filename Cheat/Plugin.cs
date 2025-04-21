using BepInEx;
using HarmonyLib;

namespace Cheat;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }
}

[HarmonyPatch(typeof(PuzzleStatus), "AddPuzzleReward")]
public static class PuzzleSetGetMatchRewards_Patch
{
    public static void Prefix(PuzzleStatus __instance)
    {
        if (__instance.bonusRound)
        {
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, 10, false);
        }
        else
        {
            __instance.AddResourceValue(PuzzleResourceType.MOVES, 1, false);
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, 10000, false);
        }
    }
}