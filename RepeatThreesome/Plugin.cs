using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace RepeatThreesome;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : Hp2BaseModPlugin
{
    private static string PLUGIN_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    private static string IMAGES_DIR = Path.Combine(PLUGIN_DIR, "images");

    [ConfigProperty(true, "If threesomes can only take place at the location their photo occurs at.")]
    public static bool LoversLocationRequirement
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If characters will change to nude outfits during bonus rounds.")]
    public bool IsBonusRoundNude
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    public static new int ModId { get { return ((Hp2BaseModPlugin)_instance).ModId; } }
    private static Plugin _instance;

    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    private void Awake()
    {
        _instance = this;

        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
                out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModCredit))
        {
            m_addModCredit(Path.Combine(IMAGES_DIR, "CreditsLogo.png"),
            [
                (
                    Path.Combine(IMAGES_DIR, "onesuchkeeper_credits.png"),
                    Path.Combine(IMAGES_DIR, "onesuchkeeper_credits_over.png"),
                    "https://www.youtube.com/@onesuchkeeper8389"
                )
            ]);
        }

        Constants.Init(ModId);

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
        ModInterface.Events.PuzzleRoundOver += ThreesomeHandler.OnPuzzleRoundOver;
        ModInterface.Events.PostCodeSubmitted += On_PostCodeSubmitted;
        ModInterface.Events.PrePersistenceReset += On_PostPersistenceReset;
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PostPersistenceReset(SaveData data)
    {
        CodeUtility.ValidateCode(data, Constants.LocalCodeId, LoversLocationRequirement);
        CodeUtility.ValidateCode(data, Constants.NudeCodeId, IsBonusRoundNude);
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

        var nudeOutfitPart = new GirlPartDataMod(Constants.NudeOutfitId, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "nudeOutfit",
            X = 0,
            Y = 0,
            SpriteInfo = emptySpriteInfo
        };

        var pollyNudeOutfitPartAlt = new GirlPartDataMod(new RelativeId(ModId, _partCount++), InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "nudeOutfitPollyAlt",
            X = 604,
            Y = 165,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Paths.PluginPath, @"RepeatThreesome\images\alt_polly_nude.png"), transform))
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
