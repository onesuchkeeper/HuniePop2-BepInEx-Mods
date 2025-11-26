using System.Reflection;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;

namespace Cheat;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private static readonly FieldInfo f_testMode = AccessTools.Field(typeof(GameManager), "_testMode");

    private void Awake()
    {
        ModInterface.Log.ShowDebug = true;
        //ModInterface.Events.PostPersistenceReset += On_PostPersistenceReset;
        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PreLoadPlayerFile(PlayerFile file)
    {
        using (ModInterface.Log.MakeIndent())
        {
            foreach (var girl in Game.Data.Girls.GetAllBySpecial(false))
            {
                file.GetPlayerFileGirl(girl).playerMet = true;
            }

            foreach (var fileGirl in file.girls)
            {
                var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id);
                var expansion = ExpandedGirlDefinition.Get(girlId);
                foreach (var body in expansion.Bodies.Values)
                {
                    foreach (var outfit in expansion.OutfitIndexToId.Keys)
                    {
                        fileGirl.UnlockOutfit(outfit);
                    }

                    foreach (var hairstyleId_index in expansion.HairstyleIdToIndex)
                    {
                        fileGirl.UnlockHairstyle(hairstyleId_index.Value);
                    }
                }
            }
        }
    }
}

[HarmonyPatch(typeof(PuzzleStatus), "AddPuzzleReward")]
public static class PuzzleSetGetMatchRewards_Patch
{
    public static void Prefix(PuzzleStatus __instance)
    {
        if (__instance.bonusRound)
        {
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, 50, false);
        }
        else
        {
            __instance.AddResourceValue(PuzzleResourceType.MOVES, 1, false);
            __instance.AddResourceValue(PuzzleResourceType.AFFECTION, 50000, false);
        }
    }
}

public enum TestEnum
{
    a,
    b,
    c
}