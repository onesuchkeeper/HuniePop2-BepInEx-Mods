﻿using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace RepeatThreesome;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{

    private static string _pluginDir = Path.Combine(Paths.PluginPath, "RepeatThreesome");
    private static string _imagesDir = Path.Combine(_pluginDir, "images");

    private int _modId;

    private void Awake()
    {
        if (Chainloader.PluginInfos.ContainsKey("OSK.BepInEx.Hp2BaseModTweaks"))
        {
            var configs = ModInterface.GetInterModValue<Dictionary<string, (string ModImagePath, List<(string CreditButtonPath, string CreditButtonOverPath, string RedirectLink)> CreditEntries)>>("OSK.BepInEx.Hp2BaseModTweaks", "ModCredits");

            configs[MyPluginInfo.PLUGIN_GUID] = (
                Path.Combine(_imagesDir, "CreditsLogo.png"),
                new List<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>(){
                    (
                        Path.Combine(_imagesDir, "onesuchkeeper_credits.png"),
                        Path.Combine(_imagesDir, "onesuchkeeper_credits_over.png"),
                        "https://www.youtube.com/@onesuchkeeper8389"
                    ),
                }
            );
        }

        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);
        Constants.Init(_modId);

        ModInterface.AddDataMod(new CodeDataMod(Constants.LocalCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("OH THE PLACES YOU'LL GO"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Lovers' threesome location requirement off.",
            OffMessage = "Lovers' threesome location requirement on."
        });

        ModInterface.AddDataMod(new CodeDataMod(Constants.NudeCodeId, InsertStyle.replace)
        {
            CodeHash = MD5Utility.Encrypt("BEWBS"),
            CodeType = CodeType.TOGGLE,
            OnMessage = "Nudity during bonus rounds on.",
            OffMessage = "Nudity during bonus rounds off."
        });

        ModInterface.Events.PreDataMods += On_PreDataMods;
        ModInterface.Events.PreRoundOverCutscene += ThreesomeHandler.PreRoundOverCutscene;
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PreDataMods()
    {
        var pollyNudeOutfitPartAltId = new RelativeId(_modId, 1);

        var nudeOutfitPart = new GirlPartDataMod(Constants.NudeOutfitId, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "nudeOutfit",
            X = 0,
            Y = 0,
            MirroredPartId = RelativeId.Default,
            AltPartId = RelativeId.Default,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = false,
                Path = "EmptySprite"
            }
        };

        var pollyNudeOutfitPart = new GirlPartDataMod(Constants.NudeOutfitId, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "nudeOutfitPolly",
            X = 0,
            Y = 0,
            MirroredPartId = RelativeId.Default,
            AltPartId = pollyNudeOutfitPartAltId,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = false,
                Path = "EmptySprite"
            }
        };

        var pollyNudeOutfitPartAlt = new GirlPartDataMod(pollyNudeOutfitPartAltId, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "nudeOutfitPollyAlt",
            X = 604,
            Y = 165,
            MirroredPartId = RelativeId.Default,
            AltPartId = RelativeId.Default,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"RepeatThreesome\images\alt_polly_nude.png")
            }
        };

        // add nude outfits for girls
        foreach (var girlId in ModInterface.Data.GetIds(GameDataType.Girl))
        {
            // polly has an alt
            if (girlId == Girls.PollyId)
            {
                ModInterface.Log.LogInfo($"Adding nude outfit for polly {girlId}");

                ModInterface.AddDataMod(new GirlDataMod(girlId, InsertStyle.append)
                {
                    parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>() { pollyNudeOutfitPart, pollyNudeOutfitPartAlt },
                    outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
                    {
                        new OutfitDataMod(Constants.NudeOutfitId, InsertStyle.replace)
                        {
                            Name = "Nude",
                            OutfitPartId = pollyNudeOutfitPart.Id,
                            IsNSFW = true,
                            HideNipples = false,
                            TightlyPaired = false,
                            PairHairstyleId = null
                        }
                    }
                });
            }
            // all others
            else
            {
                ModInterface.Log.LogInfo($"Adding nude outfit for girl {girlId}");

                ModInterface.AddDataMod(new GirlDataMod(girlId, InsertStyle.append)
                {
                    parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>() { nudeOutfitPart },
                    outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
                    {
                        new OutfitDataMod(Constants.NudeOutfitId, InsertStyle.replace)
                        {
                            Name = "Nude",
                            OutfitPartId = nudeOutfitPart.Id,
                            IsNSFW = true,
                            HideNipples = false,
                            TightlyPaired = false,
                            PairHairstyleId = null
                        }
                    }
                });
            }
        }
    }
}
