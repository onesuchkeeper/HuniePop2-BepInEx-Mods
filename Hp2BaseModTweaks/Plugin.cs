using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace Hp2BaseModTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public partial class Plugin : Hp2BaseModPlugin
{
    internal static readonly string RootDir = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    internal static readonly string ImagesDir = Path.Combine(RootDir, "images");

    public static ConfigEntry<string> DigitalArtCollectionDir => _digitalArtCollectionDir;
    private static ConfigEntry<string> _digitalArtCollectionDir;

    public static ConfigEntry<bool> UseModLogo => _useModLogo;
    private static ConfigEntry<bool> _useModLogo;

    internal static TweaksSaveData Save => _instance._save;
    private TweaksSaveData _save;

    internal static List<CreditEntry> ModCredits => _instance._modCredits;
    private List<CreditEntry> _modCredits;

    internal static List<string> LogoPaths => _instance._logoPaths;
    private List<string> _logoPaths;

    public static new int ModId => ((Hp2BaseModPlugin)_instance).ModId;

    private static Plugin _instance;

    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    protected override void Awake()
    {
        _instance = this;

        base.Awake();

        Config.SaveOnConfigSet = false;

        _useModLogo = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseModLogo), true, "If the \"HuneiePop 2: Modded\" logo should be included in logo rotation. You may want to disable this if you've installed a mod with another custom logo.");
        _digitalArtCollectionDir = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(DigitalArtCollectionDir), Path.Combine(Paths.PluginPath, "..", "..", "Digital Art Collection"), "Directory containing the HuniePop 2 Digital Art Collection Dlc");

        _modCredits = new List<CreditEntry>()
        {
            new CreditEntry(Path.Combine(ImagesDir, "CreditsLogo.png"),
            [
                new CreditMember(Path.Combine(ImagesDir, "onesuchkeeper_credits.png"),
                    Path.Combine(ImagesDir, "onesuchkeeper_credits_over.png"),
                    "https://www.youtube.com/@onesuchkeeper8389")
            ])
        };

        _logoPaths = _useModLogo.Value
            ? new List<string> { Path.Combine(ImagesDir, "logo.png") }
            : new();

        UiPrefabs.Init();
        ToggleCodeMods.AddMods(ModId);

        ModInterface.AddDataMod(new ItemDataMod(new RelativeId(ModId, 0), Hp2BaseMod.Utility.InsertStyle.replace)
        {
            ItemName = "More Girls In The Wardrobe!",
            ItemDescription = "Kyu and the Nymphojinn are now in the wardrobe! Each time the Nymphojinn are defeated, each will have a style unlocked!",
            CategoryDescription = "Otherworldly Attire",
            ItemType = ItemType.MISC,
            ItemSpriteInfo = new SpriteInfoInternal("item_shell_auger")
        });

        ModInterface.AddDataMod(new PostGameCutsceneMod());
        ModInterface.AddCommand(new SetIconCommand());

        ModInterface.Events.RequestStyleChange += RandomizeStyles.On_RequestStyleChange;
        ModInterface.Events.PostCodeSubmitted += On_PostCodeSubmitted;

        ModInterface.Assets.RequestInternal(typeof(Sprite), Common.AllSprites());
        ModInterface.Assets.RequestInternal(typeof(AudioClip), Common.Sfx_PhoneAppButtonPressed);

        ModInterface.Events.PreDataMods += On_PreDataMods;
        ModInterface.Events.PostDataMods += On_PostDataMods;
        ModInterface.Events.PreGameSave += On_PreGameSave;
        ModInterface.Events.PrePersistenceReset += On_PrePersistenceReset;
        ModInterface.Events.PostPersistenceReset += On_PostPersistenceReset;
        ModInterface.Events.RequestUnlockedPhotos += On_RequestUnlockedPhotos;

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    /// <summary>
    /// Adds a logo to the title logo pool.
    /// </summary>
    /// <param name="value"></param>
    [InteropMethod]
    public void AddLogoPath(string value) => LogoPaths.Add(value);

    /// <summary>
    /// Adds a credit entry.
    /// </summary>
    /// <param name="logoPath"></param>
    /// <param name="creditEntries"></param>
    [InteropMethod]
    public void AddModCredit(string logoPath, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)> creditEntries)
        => ModCredits.Add(new CreditEntry(logoPath, creditEntries.Select(x => new CreditMember(x.creditButtonPath, x.creditButtonOverPath, x.redirectLink))));

    private void On_PrePersistenceReset(SaveData data)
    {
        if (File.Exists(this.Config.ConfigFilePath))
        {
            foreach (var codeId in ModInterface.Data.GetIds(GameDataType.Code).Where(x => x.SourceId == -1 || x.SourceId == ModId))
            {
                var code = ModInterface.GameData.GetCode(codeId);
                var key = $"{codeId}{code.name}";

                var config = this.Config.Bind("Codes", key, false, code.codeType == CodeType.TOGGLE ? $"False:{code.offMessage}, True:{code.onMessage}" : code.onMessage);

                var runtimeId = ModInterface.Data.GetRuntimeDataId(GameDataType.Code, codeId);
                if (config.Value)
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
        }
    }

    private void On_PostPersistenceReset(SaveData data)
    {
        var handledCodes = ModInterface.Data.GetIds(GameDataType.Code).Where(x => x.SourceId == -1 || x.SourceId == ModId);

        if (!File.Exists(this.Config.ConfigFilePath))
        {
            foreach (var codeId in handledCodes)
            {
                var code = ModInterface.GameData.GetCode(codeId);
                var key = $"{codeId}{code.name}";

                var config = this.Config.Bind("Codes", key, false, code.codeType == CodeType.TOGGLE ? $"False:{code.offMessage}, True:{code.onMessage}" : code.onMessage);
                config.Value = ModInterface.GameData.IsCodeUnlocked(codeId);
            }
        }

        Config.SaveOnConfigSet = true;
        Config.Save();

        Application.runInBackground = data.unlockedCodes.Contains(ModInterface.Data.GetRuntimeDataId(GameDataType.Code, ToggleCodeMods.RunInBackgroundCodeId));

        var saveStr = ModInterface.GetSourceSave(ModId);

        if (!string.IsNullOrWhiteSpace(saveStr))
        {
            _save = JsonConvert.DeserializeObject<TweaksSaveData>(saveStr);
        }

        _save ??= new TweaksSaveData();
        _save.Clean();

        if (ModInterface.GameData.IsCodeUnlocked(ToggleCodeMods.FairyWingsCodeId))
        {
            var kyu = ModInterface.GameData.GetGirl(Girls.KyuId);

            ModInterface.Log.Message("Applying wings");
            if (kyu == null)
            {
                ModInterface.Log.Warning("Unable to find Kyu, \"PINK BITCH!\" wings not applied D:");
                return;
            }

            foreach (var girl in Game.Data.Girls.GetAllBySpecial(false))
            {
                girl.specialEffectPrefab = kyu.specialEffectPrefab;
                girl.specialEffectOffset = kyu.specialEffectOffset;
            }
        }
    }

    private void On_PreGameSave()
    {
        ModInterface.SetSourceSave(ModId, JsonConvert.SerializeObject(_save));
    }

    /// <summary>
    /// Adds head/back position and glow eyes where missing
    /// </summary>
    private void On_PreDataMods()
    {
        ModInterface.Events.PreDataMods -= On_PreDataMods;

        void MakeBackPosMod(RelativeId girlId, int x, int y)
        {
            ModInterface.AddDataMod(new GirlDataMod(girlId, InsertStyle.replace)
            {
                bodies = new()
                {
                    new GirlBodyDataMod(new RelativeId(-1, 0), InsertStyle.append)
                    {
                        BackPosition = new VectorInfo()
                        {
                            Xpos = x,
                            Ypos = y
                        }
                    }
                }
            });
        }

        MakeBackPosMod(Girls.AbiaId, 253, 473);
        MakeBackPosMod(Girls.AshleyId, 227, 540);
        MakeBackPosMod(Girls.BrookeId, 263, 496);
        MakeBackPosMod(Girls.CandaceId, 228, 518);
        MakeBackPosMod(Girls.JessieId, 227, 514);
        MakeBackPosMod(Girls.JewnId, 258, 495);
        MakeBackPosMod(Girls.LailaniId, 275, 487);
        MakeBackPosMod(Girls.LillianId, 263, 494);
        MakeBackPosMod(Girls.LolaId, 264, 511);
        MakeBackPosMod(Girls.MoxieId, 232, 482);
        MakeBackPosMod(Girls.NoraId, 298, 522);
        MakeBackPosMod(Girls.PollyId, 264, 505);
        MakeBackPosMod(Girls.SarahId, 264, 473);
        MakeBackPosMod(Girls.ZoeyId, 272, 466);

        var kyuEyesGlowNeutralPartId = new RelativeId(ModId, 0);
        var kyuEyesGlowNeutralPart = new GirlPartDataMod(kyuEyesGlowNeutralPartId, InsertStyle.replace)
        {
            X = 590,
            Y = 854,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(ImagesDir, "kyu_eyesglow_neutral.png"), true))
        };

        var kyuEyesGlowAnnoyedPartId = new RelativeId(ModId, 1);
        var kyuEyesGlowAnnoyedPart = new GirlPartDataMod(kyuEyesGlowAnnoyedPartId, InsertStyle.replace)
        {
            X = 592,
            Y = 852,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(ImagesDir, "kyu_eyesglow_annoyed.png"), true))
        };

        var kyuEyesGlowHornyPartId = new RelativeId(ModId, 2);
        var kyuEyesGlowHornyPart = new GirlPartDataMod(kyuEyesGlowHornyPartId, InsertStyle.replace)
        {
            X = 590,
            Y = 851,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(ImagesDir, "kyu_eyesglow_horny.png"), true))
        };

        ModInterface.AddDataMod(new GirlDataMod(Girls.KyuId, InsertStyle.append)
        {
            bodies = new()
            {
                new GirlBodyDataMod(new RelativeId(-1, 0), InsertStyle.append)
                {
                    expressions = new List<IBodySubDataMod<GirlExpressionSubDefinition>>() {
                        new GirlExpressionDataMod(GirlExpressions.Neutral, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowNeutralPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Annoyed, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowAnnoyedPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Horny, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowHornyPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Disappointed, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowNeutralPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Excited, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowNeutralPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Confused, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowNeutralPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Inquisitive, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowNeutralPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Sarcastic, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowNeutralPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Shy, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowNeutralPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Exhausted, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowAnnoyedPart
                        },
                        new GirlExpressionDataMod(GirlExpressions.Upset, InsertStyle.append){
                            PartEyesGlow = kyuEyesGlowAnnoyedPart
                        },
                    },
                    HeadPosition = new VectorInfo()
                    {
                        Xpos = 250,
                        Ypos = 804,
                    }
                },
            },
            CellphoneMiniHead = new SpriteInfoInternal("ui_title_icon_kyu")
        });

        if (Directory.Exists(_digitalArtCollectionDir.Value))
        {
            CellphoneSprites.AddUiCellphoneSprites("Jewn", Girls.JewnId, new Vector2(2636, 1990), new Vector2(3911, 4711));
            CellphoneSprites.AddUiCellphoneSprites("Moxie", Girls.MoxieId, new Vector2(2411, 2287), new Vector2(3900, 5068));
        }
    }

    private void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        if (Game.Persistence.playerFile.storyProgress >= 13
            && ModInterface.GameData.IsCodeUnlocked(ToggleCodeMods.KyuHoleCodeId))
        {
            args.UnlockedPhotos ??= new List<PhotoDefinition>();
            args.UnlockedPhotos.AddRange(Game.Session.Hub.kyuPhotoDefs);
        }
    }

    private void On_PostDataMods()
    {
        ModInterface.Events.PostDataMods -= On_PostDataMods;

        Game.Data.Girls.GetAllBySpecial(true)
            .SelectMany(x => x.expressions)
            .Where(x => x != null && x.partIndexEyesGlow == -1)
            .ForEach(x => x.partIndexEyesGlow = x.partIndexEyes);

        var kyu = ModInterface.GameData.GetGirl(Girls.KyuId).Expansion();
    }

    private void On_PostCodeSubmitted(CodeDefinition codeDefinition)
    {
        if (codeDefinition == null)
        {
            ModInterface.Log.Message("null code");
            return;
        }

        var id = ModInterface.Data.GetDataId(GameDataType.Code, codeDefinition.id);

        if (id.SourceId == -1 || id.SourceId == ModId)
        {
            if (this.Config.TryGetEntry<bool>("Codes", $"{id}{codeDefinition.name}", out var config))
            {
                config.Value = ModInterface.GameData.IsCodeUnlocked(id);
            }
        }

        if (id == ToggleCodeMods.RunInBackgroundCodeId)
        {
            Application.runInBackground = ModInterface.GameData.IsCodeUnlocked(ToggleCodeMods.RunInBackgroundCodeId);
        }
    }
}
