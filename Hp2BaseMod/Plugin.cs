﻿using System;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod.Commands;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hp2BaseMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("HuniePop 2 - Double Date.exe")]
public class Plugin : BaseUnityPlugin
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void BeforeSplashScreen()
    {
        SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
    }

    private void Awake()
    {
        try
        {
            ModInterface.Init();
            ModInterface.Log.LogInfo("Mod interface initialized");
        }
        catch (Exception e)
        {
            ModInterface.Log.LogError("Failed to initialize mod interface", e);
            return;
        }

        try
        {
            // register defaults
            ModInterface.Log.LogInfo("Registering default data");
            using (ModInterface.Log.MakeIndent())
            {
                foreach (var ability in DefaultData.DefaultAbilityIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Ability, ability.LocalId);
                }
                foreach (var ailment in DefaultData.DefaultAilmentIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Ailment, ailment.LocalId);
                }
                foreach (var code in DefaultData.DefaultCodeIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Code, code.LocalId);
                }
                foreach (var cutscene in DefaultData.DefaultCutsceneIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Cutscene, cutscene.LocalId);
                }
                foreach (var dt in DefaultData.DefaultDialogTriggerIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.DialogTrigger, dt.LocalId);
                }
                foreach (var dlc in DefaultData.DefaultDlcIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Dlc, dlc.LocalId);
                }
                foreach (var energy in DefaultData.DefaultEnergyIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Energy, energy.LocalId);
                }
                foreach (var girl in DefaultData.DefaultGirlIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Girl, girl.LocalId);
                }
                foreach (var pair in DefaultData.DefaultGirlPairIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.GirlPair, pair.LocalId);
                }
                foreach (var item in DefaultData.DefaultItemIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Item, item.LocalId);
                }
                foreach (var location in DefaultData.DefaultLocationIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Location, location.LocalId);
                }
                foreach (var photo in DefaultData.DefaultPhotoIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Photo, photo.LocalId);
                }
                foreach (var question in DefaultData.DefaultQuestionIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Question, question.LocalId);
                }
                foreach (var token in DefaultData.DefaultTokenIds)
                {
                    ModInterface.Data.RegisterDefaultData(GameDataType.Token, token.LocalId);
                }

                ModInterface.Data.RegisterDefaultData(GameDataType.Fruit, FruitTypes.Talent.LocalId);
                ModInterface.Data.RegisterDefaultData(GameDataType.Fruit, FruitTypes.Flirtation.LocalId);
                ModInterface.Data.RegisterDefaultData(GameDataType.Fruit, FruitTypes.Romance.LocalId);
                ModInterface.Data.RegisterDefaultData(GameDataType.Fruit, FruitTypes.Sexuality.LocalId);

                ModInterface.Data.RegisterDefaultData(GameDataType.Affection, AffectionTypes.Talent.LocalId);
                ModInterface.Data.RegisterDefaultData(GameDataType.Affection, AffectionTypes.Flirtation.LocalId);
                ModInterface.Data.RegisterDefaultData(GameDataType.Affection, AffectionTypes.Romance.LocalId);
                ModInterface.Data.RegisterDefaultData(GameDataType.Affection, AffectionTypes.Sexuality.LocalId);

                ModInterface.Data.RegisterDefaultData(GameDataType.SpecialPart, SpecialParts.KyuWingId.LocalId);
                ModInterface.Data.RegisterDefaultData(GameDataType.SpecialPart, SpecialParts.MoxieWingId.LocalId);
                ModInterface.Data.RegisterDefaultData(GameDataType.SpecialPart, SpecialParts.JewnWingId.LocalId);
            }
        }
        catch (Exception e)
        {
            ModInterface.Log.LogError("Failed to handle existing game data", e);
            return;
        }

        try
        {
            ModInterface.Log.LogInfo("Applying Patches");
            using (ModInterface.Log.MakeIndent())
            {
                new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
            }
        }
        catch (Exception e)
        {
            ModInterface.Log.LogError("Failed to patch", e);
            return;
        }

        ModInterface.AddCommand(new HelpCommand());
        ModInterface.AddCommand(new EchoCommand());
        ModInterface.AddCommand(new ArtCommand());

        ModInterface.Log.LogInfo(Art.Random());
    }
}
