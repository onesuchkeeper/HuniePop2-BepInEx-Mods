using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace Hp2BaseModTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static readonly string RootDir = Path.Combine(Paths.PluginPath, "Hp2BaseModTweaks");
    internal static readonly string ImagesDir = Path.Combine(RootDir, "images");
    internal static readonly string DacDir = Path.Combine(Paths.PluginPath, "..", "..", "Digital Art Collection");

    internal static TweaksSaveData Save => _save;
    private static TweaksSaveData _save;

    internal static Dictionary<string, (string ModImagePath, List<(string CreditButtonPath, string CreditButtonOverPath, string RedirectLink)> CreditEntries)> GetModCredits()
        => ModInterface.GetInterModValue<Dictionary<string, (string ModImagePath, List<(string CreditButtonPath, string CreditButtonOverPath, string RedirectLink)> CreditEntries)>>(ModId, "ModCredits");

    internal static IEnumerable<string> GetLogoPaths() => ModInterface.GetInterModValue<IEnumerable<string>>(ModId, "LogoPaths");

    public static readonly int ModId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        ModInterface.RegisterInterModValue(ModId, "ModCredits", new Dictionary<string, (string ModImagePath, List<(string CreditButtonPath, string CreditButtonOverPath, string RedirectLink)> CreditEntries)>() {
            {MyPluginInfo.PLUGIN_GUID, (
                Path.Combine(ImagesDir, "CreditsLogo.png"),
                new List<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>(){
                    (
                        Path.Combine(ImagesDir, "onesuchkeeper_credits.png"),
                        Path.Combine(ImagesDir, "onesuchkeeper_credits_over.png"),
                        "https://www.youtube.com/@onesuchkeeper8389"
                    )
                }
            )}
        });

        ModInterface.RegisterInterModValue(ModId, "LogoPaths",
            new List<string> {
                Path.Combine(ImagesDir, "logo.png")
            });

        UiPrefabs.Init();
        ToggleCodeMods.AddMods(ModId);

        ModInterface.AddDataMod(new ItemDataMod(new RelativeId(ModId, 0), Hp2BaseMod.Utility.InsertStyle.replace)
        {
            ItemName = "More Girls In The Wardrobe!",
            ItemDescription = "Kyu and the Nymphojinn are now in the wardrobe! Each time the Nymphojinn are defeated, each will have a style unlocked!",
            CategoryDescription = "Otherworldly Attire",
            ItemType = ItemType.MISC,
            ItemSpriteInfo = new SpriteInfoPath()
            {
                IsExternal = false,
                Path = "item_shell_auger"
            },
        });

        ModInterface.AddDataMod(new PostGameCutsceneMod());
        ModInterface.AddCommand(new SetIconCommand());

        ModInterface.Events.RequestStyleChange += RandomizeStyles.On_RequestStyleChange;
        ModInterface.Events.PostCodeSubmitted += On_PostCodeSubmitted;

        ModInterface.Events.PostPersistenceReset += () => Application.runInBackground = ModInterface.GameData.IsCodeUnlocked(ToggleCodeMods.RunInBackgroundCodeId);

        ModInterface.Assets.RequestInternal(typeof(Sprite), Common.AllSprites());
        ModInterface.Assets.RequestInternal(typeof(AudioClip), Common.Sfx_PhoneAppButtonPressed);

        ModInterface.Events.PreDataMods += On_PreDataMods;
        ModInterface.Events.PostDataMods += On_PostDataMods;
        ModInterface.Events.PreGameSave += On_PreGameSave;
        ModInterface.Events.PostPersistenceReset += On_PostPersistenceReset;

        ModInterface.AddCommand(new SetIconCommand());

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }

    private void On_PreGameSave()
    {
        ModInterface.SetSourceSave(ModId, JsonConvert.SerializeObject(_save));
    }

    private void On_PostPersistenceReset()
    {
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

            ModInterface.Log.LogInfo("Applying wings");
            if (kyu == null)
            {
                ModInterface.Log.LogWarning("Unable to find Kyu, \"PINK BITCH!\" wings not applied D:");
                return;
            }

            foreach (var girl in Game.Data.Girls.GetAll())
            {
                girl.specialEffectPrefab = kyu.specialEffectPrefab;
                girl.specialEffectOffset = kyu.specialEffectOffset;
            }
        }
    }

    /// <summary>
    /// Adds head/back position and glow eyes where missing
    /// </summary>
    private void On_PreDataMods()
    {
        ModInterface.Events.PreDataMods -= On_PreDataMods;

        ModInterface.AddDataMod(new GirlDataMod(Girls.AbiaId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 423 - 420 + 250,
                Ypos = 957 - 24 - 460
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.AshleyId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 387 - 420 + 260,
                Ypos = 964 - 24 - 400
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.BrookeId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 378 - 420 + 305,
                Ypos = 960 - 24 - 440
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.CandaceId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 348 - 420 + 300,
                Ypos = 972 - 24 - 430
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.JessieId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 457 - 420 + 190,
                Ypos = 983 - 24 - 445
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.JewnId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 228 - 420 + 450,
                Ypos = 1019 - 24 - 500
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.LailaniId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 345 - 420 + 350,
                Ypos = 931 - 24 - 420
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.LillianId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 435 - 420 + 248,
                Ypos = 918 - 24 - 400
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.LolaId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 414 - 420 + 270,
                Ypos = 985 - 24 - 450
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.MoxieId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 262 - 420 + 390,
                Ypos = 956 - 24 - 450
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.NoraId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 478 - 420 + 240,
                Ypos = 966 - 24 - 420
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.PollyId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 384 - 420 + 300,
                Ypos = 949 - 24 - 420
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.SarahId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 414 - 420 + 270,
                Ypos = 917 - 24 - 420
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.ZoeyId, InsertStyle.replace)
        {
            BackPosition = new VectorInfo()
            {
                Xpos = 522 - 420 + 170,
                Ypos = 900 - 24 - 410
            }
        });

        var kyuEyesGlowNeutralPartId = new RelativeId(ModId, 0);
        var kyuEyesGlowNeutralPart = new GirlPartDataMod(kyuEyesGlowNeutralPartId, InsertStyle.replace)
        {
            X = 590,
            Y = 854,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(ImagesDir, "kyu_eyesglow_neutral.png")
            }
        };

        var kyuEyesGlowAnnoyedPartId = new RelativeId(ModId, 1);
        var kyuEyesGlowAnnoyedPart = new GirlPartDataMod(kyuEyesGlowAnnoyedPartId, InsertStyle.replace)
        {
            X = 592,
            Y = 852,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(ImagesDir, "kyu_eyesglow_annoyed.png")
            }
        };

        var kyuEyesGlowHornyPartId = new RelativeId(ModId, 2);
        var kyuEyesGlowHornyPart = new GirlPartDataMod(kyuEyesGlowHornyPartId, InsertStyle.replace)
        {
            X = 590,
            Y = 851,
            PartType = GirlPartType.EYESGLOW,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(ImagesDir, "kyu_eyesglow_horny.png")
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
                new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.HORNY), InsertStyle.replace){
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
            },
            HeadPosition = new VectorInfo()
            {
                Xpos = 420 + 250 - 420,
                Ypos = 968 - 140 - 24,
            },
            CellphoneMiniHead = new SpriteInfoPath()
            {
                IsExternal = false,
                Path = "ui_title_icon_kyu",
            }
        });

        if (Directory.Exists(DacDir))
        {
            CellphoneSprites.AddUiCellphoneSprites("Jewn", Girls.JewnId, new Vector2(2636, 1990), new Vector2(3911, 4711));
            CellphoneSprites.AddUiCellphoneSprites("Moxie", Girls.MoxieId, new Vector2(2411, 2287), new Vector2(3900, 5068));
        }
    }

    private void On_PostDataMods()
    {
        ModInterface.Events.PostDataMods -= On_PostDataMods;

        Game.Data.Girls.GetAllBySpecial(true)
            .SelectMany(x => x.expressions)
            .Where(x => x.partIndexEyesGlow == -1)
            .ForEach(x => x.partIndexEyesGlow = x.partIndexEyes);
    }

    private void On_PostCodeSubmitted()
    {
        Application.runInBackground = ModInterface.GameData.IsCodeUnlocked(ToggleCodeMods.RunInBackgroundCodeId);
    }
}
