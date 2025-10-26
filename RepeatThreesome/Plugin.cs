using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
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
    public static Plugin Instance => _instance;
    private static Plugin _instance;

    private static string _pluginDir = Path.Combine(Paths.PluginPath, "RepeatThreesome");
    private static string _imagesDir = Path.Combine(_pluginDir, "images");
    private static string ConfigGeneral = "General";

    public bool LoversLocationRequirement
    {
        get => this.Config.TryGetEntry<bool>(ConfigGeneral, nameof(LoversLocationRequirement), out var config)
            ? config.Value
            : true;
        set
        {
            if (this.Config.TryGetEntry<bool>(ConfigGeneral, nameof(LoversLocationRequirement), out var config))
            {
                config.Value = value;
            }
            else
            {
                Logger.LogWarning($"Failed to find binding for config value {nameof(LoversLocationRequirement)}");
            }
        }
    }

    public bool IsBonusRoundNude
    {
        get => this.Config.TryGetEntry<bool>(ConfigGeneral, nameof(IsBonusRoundNude), out var config)
            ? config.Value
            : true;
        set
        {
            if (this.Config.TryGetEntry<bool>(ConfigGeneral, nameof(IsBonusRoundNude), out var config))
            {
                config.Value = value;
            }
            else
            {
                Logger.LogWarning($"Failed to find binding for config value {nameof(IsBonusRoundNude)}");
            }
        }
    }

    private int _modId;

    private void Awake()
    {
        _instance = this;

        this.Config.Bind(ConfigGeneral, nameof(LoversLocationRequirement), true, "If threesomes can only take place at the location their photo occurs at.");
        this.Config.Bind(ConfigGeneral, nameof(IsBonusRoundNude), true, "If characters will change to nude outfits during bonus rounds.");

        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
                out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModCredit))
        {
            m_addModCredit(Path.Combine(_imagesDir, "CreditsLogo.png"),
            [
                (
                    Path.Combine(_imagesDir, "onesuchkeeper_credits.png"),
                    Path.Combine(_imagesDir, "onesuchkeeper_credits_over.png"),
                    "https://www.youtube.com/@onesuchkeeper8389"
                )
            ]);
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
        ModInterface.Events.PostCodeSubmitted += On_PostCodeSubmitted;
        ModInterface.Events.PrePersistenceReset += On_PostPersistenceReset;
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PostPersistenceReset(SaveData data)
    {
        ValidateCode(data, Constants.LocalCodeId, LoversLocationRequirement);
        ValidateCode(data, Constants.NudeCodeId, IsBonusRoundNude);
    }

    private void ValidateCode(SaveData data, RelativeId codeId, bool isUnlocked)
    {
        var runtimeId = ModInterface.Data.GetRuntimeDataId(GameDataType.Code, codeId);
        if (isUnlocked)
        {
            if (!data.unlockedCodes.Contains(runtimeId))
            {
                data.unlockedCodes.Add(runtimeId);
            }
        }
        else
        {
            data.unlockedCodes.Remove(runtimeId);
        }
    }

    private void On_PostCodeSubmitted(CodeDefinition codeDefinition)
    {
        if (codeDefinition == null)
        {
            return;
        }

        LoversLocationRequirement = ModInterface.GameData.IsCodeUnlocked(Constants.LocalCodeId);
        IsBonusRoundNude = ModInterface.GameData.IsCodeUnlocked(Constants.NudeCodeId);
    }

    private void On_PreDataMods()
    {
        var _partCount = 0;

        var emptySpriteInfo = new SpriteInfoInternal("EmptySprite");

        var emptyPart = new GirlPartDataMod(new RelativeId(_modId, _partCount++), InsertStyle.replace)
        {
            X = 0,
            Y = 0,
            PartType = GirlPartType.BODY,
            PartName = "Body",
            SpriteInfo = emptySpriteInfo
        };

        var nudeOutfitPart = new GirlPartDataMod(Constants.NudeOutfitId, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "nudeOutfit",
            X = 0,
            Y = 0,
            SpriteInfo = emptySpriteInfo
        };

        var pollyNudeOutfitPartAlt = new GirlPartDataMod(new RelativeId(_modId, _partCount++), InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "nudeOutfitPollyAlt",
            X = 604,
            Y = 165,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Paths.PluginPath, @"RepeatThreesome\images\alt_polly_nude.png")))
        };

        var pollyNudeOutfitPart = new GirlPartDataMod(Constants.NudeOutfitId, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "nudeOutfitPolly",
            X = 0,
            Y = 0,
            AltPart = pollyNudeOutfitPartAlt,
            SpriteInfo = emptySpriteInfo
        };

        var nudeOutfit = new OutfitDataMod(Constants.NudeOutfitId, InsertStyle.replace)
        {
            Name = "Nude",
            OutfitPart = nudeOutfitPart,
            IsNSFW = true,
            HideNipples = false,
            TightlyPaired = false,
            PairHairstyleId = null
        };

        var nudeOutfitPolly = new OutfitDataMod(Constants.NudeOutfitId, InsertStyle.replace)
        {
            Name = "Nude",
            OutfitPart = pollyNudeOutfitPart,
            IsNSFW = true,
            HideNipples = false,
            TightlyPaired = false,
            PairHairstyleId = null
        };

        // add nude outfits for girls
        foreach (var girlId in ModInterface.Data.GetIds(GameDataType.Girl).Where(x => x.SourceId == -1))
        {
            // polly has an alt
            if (girlId == Girls.PollyId)
            {
                ModInterface.Log.LogInfo($"Adding nude outfit for polly {girlId}");

                ModInterface.AddDataMod(new GirlDataMod(girlId, InsertStyle.append)
                {
                    bodies = new(){
                        new GirlBodyDataMod(new RelativeId(-1,0), InsertStyle.append){
                            outfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>()
                            {
                                nudeOutfitPolly
                            }
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
                    bodies = new(){
                        new GirlBodyDataMod(new RelativeId(-1,0), InsertStyle.append){
                            outfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>()
                            {
                                nudeOutfit
                            }
                        }
                    }
                });
            }
        }
    }
}
