using BepInEx;
using HarmonyLib;

namespace Cheat;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        new Harmony("Hp2BaseMod.Cheat").PatchAll();
    }
}

[HarmonyPatch(typeof(PuzzleStatus), "AddPuzzleReward")]
public static class PuzzleSetGetMatchRewards_Patch
{
    public static void Prefix(PuzzleStatus __instance)
    {
        //if (__instance.bonusRound) { return; }
        __instance.AddResourceValue(PuzzleResourceType.AFFECTION, 100000, false);
    }
}