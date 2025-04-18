﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using Hp2BaseModTweaks.CellphoneApps;
using Newtonsoft.Json;
using UnityEngine;

namespace Hp2BaseModTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private static readonly int _extraHeadPadding = 10;
    private static readonly string _modConfig = "mod.json";
    private static readonly string _dacDir = Path.Combine(Paths.PluginPath, "..", "..", "Digital Art Collection");

    private static readonly Vector2 _cellphoneHeadSize = new Vector2(138, 126);
    private static readonly Vector2 _cellphoneMiniHeadSize = new Vector2(80, 80);
    private static readonly Vector2 _kyuCellphoneMiniHeadSize = new Vector2(92, 60);
    private static readonly Vector2 _cellphonePortraitSize = new Vector2(214, 270);

    private void Awake()
    {
        foreach (var modFolder in Directory.GetDirectories(Paths.PluginPath))
        {
            var fullPath = Path.Combine(modFolder, _modConfig);
            if (File.Exists(fullPath))
            {
                var isValid = true;
                var config = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(fullPath));
                if (config == null)
                {
                    ModInterface.Log.LogError($"Failed to deserialize {fullPath}");
                    isValid = false;
                }
                else
                {
                    config.ModImagePath = Path.Combine(modFolder, config.ModImagePath);
                    if (!File.Exists(config.ModImagePath))
                    {
                        ModInterface.Log.LogError($"Cannot find {config.ModImagePath}");
                        isValid = false;
                    }

                    foreach (var entry in config.CreditsEntries.OrEmptyIfNull())
                    {
                        entry.CreditButtonImagePath = Path.Combine(modFolder, entry.CreditButtonImagePath);
                        if (!File.Exists(entry.CreditButtonImagePath))
                        {
                            ModInterface.Log.LogError($"Cannot find {entry.CreditButtonImagePath}");
                            isValid = false;
                        }

                        entry.CreditButtonImageOverPath = Path.Combine(modFolder, entry.CreditButtonImageOverPath);
                        if (!File.Exists(entry.CreditButtonImageOverPath))
                        {
                            ModInterface.Log.LogError($"Cannot find {entry.CreditButtonImageOverPath}");
                            isValid = false;
                        }
                    }

                    foreach (var entry in config.LogoImages.OrEmptyIfNull())
                    {
                        var logoPath = Path.Combine(modFolder, entry);
                        if (File.Exists(logoPath))
                        {
                            Common.LogoPaths.Add(logoPath);
                        }
                        else
                        {
                            ModInterface.Log.LogError($"Unable to find {logoPath}");
                        }
                    }
                }

                if (isValid)
                {
                    ModInterface.Log.LogInfo($"{fullPath} Registered");
                    Common.Mods.Add(config);
                }
            }
        }

        var modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        var kyuEyesGlowNeutralPartId = new RelativeId(modId, 0);
        var kyuEyesGlowNeutralPart = new GirlPartDataMod(kyuEyesGlowNeutralPartId, InsertStyle.replace)
        {
            X = 590,
            Y = 854,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, "Hp2BaseModTweaks", "images", "kyu_eyesglow_neutral.png")
            }
        };

        var kyuEyesGlowAnnoyedPartId = new RelativeId(modId, 0);
        var kyuEyesGlowAnnoyedPart = new GirlPartDataMod(kyuEyesGlowAnnoyedPartId, InsertStyle.replace)
        {
            X = 592,
            Y = 852,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, "Hp2BaseModTweaks", "images", "kyu_eyesglow_annoyed.png")
            }
        };

        var kyuEyesGlowHornyPartId = new RelativeId(modId, 0);
        var kyuEyesGlowHornyPart = new GirlPartDataMod(kyuEyesGlowHornyPartId, InsertStyle.replace)
        {
            X = 590,
            Y = 851,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, "Hp2BaseModTweaks", "images", "kyu_eyesglow_horny.png")
            }
        };

        ModInterface.AddDataMod(new GirlDataMod(Girls.KyuId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>(){
                kyuEyesGlowNeutralPart,
                kyuEyesGlowAnnoyedPart,
                kyuEyesGlowHornyPart,
            },
            expressions = new List<IGirlSubDataMod<GirlExpressionSubDefinition>>() {
                new GirlExpressionDataMod(GirlExpressions.Neutral, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowNeutralPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Annoyed, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowAnnoyedPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Horny, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowHornyPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Disappointed, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowNeutralPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Excited, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowNeutralPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Confused, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowNeutralPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Inquisitive, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowNeutralPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Sarcastic, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowNeutralPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Shy, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowNeutralPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Exhausted, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowAnnoyedPartId
                },
                new GirlExpressionDataMod(GirlExpressions.Upset, InsertStyle.replace){
                    PartIdEyesGlow = kyuEyesGlowAnnoyedPartId
                },
            }
        });

        ModInterface.Assets.RequestInternalSprite([
            Common.Ui_PhotoAlbumSlot,
            Common.Ui_PhotoButtonLeft,
            Common.Ui_PhotoButtonRight,
            Common.Ui_AppSettingArrowLeft,
            Common.Ui_AppSettingArrowLeftOver,
            Common.Ui_AppSettingArrowRight,
            Common.Ui_AppSettingArrowRightOver
        ]);

        ModInterface.Assets.RequestInternalAudio(Common.Sfx_PhoneAppButtonPressed);

        ModInterface.Events.PreDataMods += On_PreDataMods;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PreDataMods()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.KyuId, InsertStyle.replace)
        {
            CellphoneMiniHead = new SpriteInfoPath()
            {
                Path = "ui_title_icon_kyu",
            }
        });

        if (Directory.Exists(_dacDir))
        {
            var jewn = new GirlDataMod(Girls.JewnId, InsertStyle.replace);
            AddUiCellphoneSprites("Jewn", jewn, new Vector2(2636, 1990), new Vector2(3911, 4711));
            ModInterface.AddDataMod(jewn);

            var moxie = new GirlDataMod(Girls.MoxieId, InsertStyle.replace);
            AddUiCellphoneSprites("Moxie", moxie, new Vector2(2411, 2287), new Vector2(3900, 5068));
            ModInterface.AddDataMod(moxie);
        }

        foreach (var expression in Game.Data.Girls.GetAllBySpecial(true).SelectMany(x => x.expressions))
        {
            if (expression.partIndexEyesGlow == -1)
            {
                expression.partIndexEyesGlow = expression.partIndexEyes;
            }
        }
    }

    private void AddUiCellphoneSprites(string name, GirlDataMod mod, Vector2 headSize, Vector2 portraitSize)
    {
        var texturePath = Path.Combine(_dacDir, "Heads", $"{name}.png");
        if (File.Exists(texturePath))
        {
            //offset to center in square
            var diff = Math.Abs(headSize.x - headSize.y);
            var padding = headSize.x > headSize.y
                ? new RectInt(0, (int)(diff / 2), 0, (int)(diff))
                : new RectInt((int)(diff / 2), 0, (int)(diff), 0);

            var scale = KeepRatio(headSize, _cellphoneHeadSize);
            mod.CellphoneHead = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = texturePath,
                TextureScale = scale,
                TexturePadding = new RectInt((int)(padding.x * scale.x) + _extraHeadPadding,
                    (int)(padding.y * scale.y) + _extraHeadPadding,
                    (int)(padding.width * scale.x) + (2 * _extraHeadPadding),
                    (int)(padding.height * scale.y) + (2 * _extraHeadPadding))
            };

            scale = KeepRatio(headSize, _cellphoneMiniHeadSize);
            mod.CellphoneMiniHead = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = texturePath,
                TextureScale = scale,
                TexturePadding = new RectInt((int)(padding.x * scale.x) + _extraHeadPadding,
                    (int)(padding.y * scale.y) + _extraHeadPadding,
                    (int)(padding.width * scale.x) + (2 * _extraHeadPadding),
                    (int)(padding.height * scale.y) + (2 * _extraHeadPadding))
            };
        }

        texturePath = Path.Combine(_dacDir, "Sprites", $"{name}.png");
        if (File.Exists(texturePath))
        {
            mod.CellphonePortrait = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = texturePath,
                TextureScale = KeepRatio(portraitSize, _cellphonePortraitSize)
            };
        }
    }

    private Vector2 KeepRatio(Vector2 size, Vector2 bounds)
    {
        var ratio = bounds / size;
        var min = Mathf.Min(ratio.x, ratio.y);
        return new Vector2(min, min);
    }
}