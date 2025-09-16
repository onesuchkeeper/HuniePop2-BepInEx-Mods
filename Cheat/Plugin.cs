using System.Reflection;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;

namespace Cheat;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private static readonly FieldInfo _testMode = AccessTools.Field(typeof(GameManager), "_testMode");

    private void Awake()
    {
        //ModInterface.Events.PostPersistenceReset += On_PostPersistenceReset;
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    // private void On_PostPersistenceReset(SaveData data)
    // {
    //     _testMode.SetValue(Game.Manager, true);
    // }
}

[HarmonyPatch(typeof(PuzzleStatus), "AddPuzzleReward")]
public static class PuzzleSetGetMatchRewards_Patch
{
    public static void Prefix(PuzzleStatus __instance)
    {
        if (__instance.bonusRound)
        {
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, 10000, false);
        }
        else
        {
            __instance.AddResourceValue(PuzzleResourceType.MOVES, 1, false);
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, 10000, false);
        }
    }
}