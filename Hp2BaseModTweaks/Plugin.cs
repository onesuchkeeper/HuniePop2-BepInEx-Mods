using System;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo;
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

        ModInterface.Ui.AddMainAppController(typeof(UiCellphoneAppFinder), (x) => new ExpandedUiCellphoneFinderApp(x as UiCellphoneAppFinder));
        ModInterface.Ui.AddMainAppController(typeof(UiCellphoneAppGirls), (x) => new ExpandedUiCellphoneGirlsApp(x as UiCellphoneAppGirls));
        ModInterface.Ui.AddMainAppController(typeof(UiCellphoneAppPairs), (x) => new ExpandedUiCellphonePairsApp(x as UiCellphoneAppPairs));
        ModInterface.Ui.AddMainAppController(typeof(UiCellphoneAppProfile), (x) => new ExpandedUiCellphoneProfileApp(x as UiCellphoneAppProfile));
        ModInterface.Ui.AddMainAppController(typeof(UiCellphoneAppWardrobe), (x) => new ExpandedUiCellphoneWardrobeApp(x as UiCellphoneAppWardrobe));
        ModInterface.Ui.AddTitleAppController(typeof(UiCellphoneAppCredits), (x) => new ExpandedUiCellphoneCreditsApp(x as UiCellphoneAppCredits));
        ModInterface.Ui.AddUiWindowController(typeof(UiWindowPhotos), (x) => new ExpandedUiWindowPhotos(x as UiWindowPhotos));

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

        new Harmony("Hp2BaseMod.Hp2BaseModTweaks").PatchAll();
    }

    private void On_PreDataMods()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.KyuId, Hp2BaseMod.Utility.InsertStyle.replace)
        {
            CellphoneMiniHead = new SpriteInfoPath()
            {
                Path = "ui_title_icon_kyu",
            }
        });

        if (Directory.Exists(_dacDir))
        {
            var jewn = new GirlDataMod(Girls.JewnId, Hp2BaseMod.Utility.InsertStyle.replace);
            AddUiCellphoneSprites("Jewn", jewn, new Vector2(2636, 1990), new Vector2(3911, 4711));
            ModInterface.AddDataMod(jewn);

            var moxie = new GirlDataMod(Girls.MoxieId, Hp2BaseMod.Utility.InsertStyle.replace);
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