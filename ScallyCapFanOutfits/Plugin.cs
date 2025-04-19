using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ScallyCapFanOutfits;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private static RelativeId _styleId_1;
    private static int _modId;

    private void Awake()
    {
        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        _styleId_1 = new RelativeId(_modId, 0);

        var outfitPartId_1 = new RelativeId(_modId, 0);
        var frontHairPartId_1 = new RelativeId(_modId, 1);
        var backHairPartId_1 = new RelativeId(_modId, 2);
        var outfitPartId_1_Mirror = new RelativeId(_modId, 3);

        var lailaniKeyWestPart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "keyWestOutfitLailani",
            X = 345 + 76,
            Y = 931 - 249,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\lailani_outfit_keyWest.png")
            }
        };

        var zoeyExtraterrestrialOutfitPart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "extraterrestrialOutfitZoey",
            X = 522 - 3,
            Y = 900 - 215,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\zoey_outfit_extraterrestrial.png")
            }
        };

        var zoeyExtraterrestrialFronthairPart = new GirlPartDataMod(frontHairPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.FRONTHAIR,
            PartName = "extraterrestrialFronthairZoey",
            X = 522 + 26,
            Y = 900 + 21,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\zoey_fronthair_extraterrestrial.png")
            }
        };

        var zoeyExtraterrestrialBackhairPart = new GirlPartDataMod(backHairPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.BACKHAIR,
            PartName = "extraterrestrialBackhairZoey",
            X = 522 + 65,
            Y = 900 + 33,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\zoey_backhair_extraterrestrial.png")
            }
        };

        var jessieBusinessCasualOutfitPart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "businessCasualOutfitJessie",
            X = 457 - 3,
            Y = 983 - 261,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\jessie_outfit_businesscasual.png")
            }
        };

        var candaceHimeOutfitPart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "himeOutfitCandace",
            X = 348 + 107,
            Y = 972 - 184,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\candace_outfit_hime.png")
            }
        };

        var candaceHimeFronthairPart = new GirlPartDataMod(frontHairPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.FRONTHAIR,
            PartName = "himeFronthairCandace",
            X = 348 + 176,
            Y = 972 + 35,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\candace_fronthair_hime.png")
            }
        };

        var candaceHimeBackhairPart = new GirlPartDataMod(backHairPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.BACKHAIR,
            PartName = "himeBackhairCandace",
            X = 348 + 150,
            Y = 972 - 50,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\candace_backhair_hime.png")
            }
        };

        var lillianSceneOutfitPartMirror = new GirlPartDataMod(outfitPartId_1_Mirror, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "sceneQueenOutfitLillianMirror",
            X = 435 - 17,
            Y = 918 - 210,
            MirroredPartId = outfitPartId_1,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\lillian_outfit_sceneQueen_mirror.png")
            }
        };

        var lillianSceneOutfitPart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "sceneQueenOutfitLillian",
            X = 435 - 17,
            Y = 918 - 210,
            MirroredPartId = outfitPartId_1_Mirror,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\lillian_outfit_sceneQueen.png")
            }
        };

        var lillianSceneFronthairPart = new GirlPartDataMod(frontHairPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.FRONTHAIR,
            PartName = "sceneQueenFronthairLillian",
            X = 435 + 111,
            Y = 918 + 31,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\lillian_fronthair_sceneQueen.png")
            }
        };

        var lillianSceneBackhairPart = new GirlPartDataMod(backHairPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.BACKHAIR,
            PartName = "sceneQueenBackhairLillian",
            X = 435 + 71,
            Y = 918 + 73,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\lillian_backhair_sceneQueen.png")
            }
        };

        var ashleyBikerPart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "bikerLacesOutfitAshley",
            X = 387,
            Y = 868,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\ashley_outfit_ashley.png")
            }
        };

        var sarahTsuyomePart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "tsuyomeOutfitSarah",
            X = 414,
            Y = 645,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\sarah_outfit_tsuyome.png")
            }
        };

        ModInterface.AddDataMod(new GirlDataMod(Girls.SarahId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>() { sarahTsuyomePart },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Tsuyome",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                }
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.AshleyId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>() { ashleyBikerPart },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Biker Laces",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                }
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.LillianId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>(){
                 lillianSceneBackhairPart,
                 lillianSceneFronthairPart,
                 lillianSceneOutfitPart,
                 lillianSceneOutfitPartMirror
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Scene Queen",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = _styleId_1
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Scene Queen",
                    BackHairPartId = backHairPartId_1,
                    FrontHairPartId = frontHairPartId_1,
                    IsNSFW = false,
                    HideSpecials = false,
                    TightlyPaired = true,
                    PairOutfitId = _styleId_1
                }
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.CandaceId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>(){
                 candaceHimeOutfitPart,
                 candaceHimeBackhairPart,
                 candaceHimeFronthairPart
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Hime",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = _styleId_1,
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Hime",
                    BackHairPartId = backHairPartId_1,
                    FrontHairPartId = frontHairPartId_1,
                    IsNSFW = false,
                    HideSpecials = false,
                    TightlyPaired = true,
                    PairOutfitId = _styleId_1
                }
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.JessieId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>() { jessieBusinessCasualOutfitPart },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Office Siren",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                }
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.ZoeyId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>(){
                 zoeyExtraterrestrialBackhairPart,
                 zoeyExtraterrestrialFronthairPart,
                 zoeyExtraterrestrialOutfitPart
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Extraterrestrial",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = _styleId_1,
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Extraterrestrial",
                    BackHairPartId = backHairPartId_1,
                    FrontHairPartId = frontHairPartId_1,
                    IsNSFW = false,
                    HideSpecials = false,
                    TightlyPaired = true,
                    PairOutfitId = _styleId_1
                }
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.LailaniId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>() { lailaniKeyWestPart },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Key West",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                }
            }
        });

        ModInterface.Events.PreLoadPlayerFile += On_PrePersistenceReset;
    }

    private void On_PrePersistenceReset(PlayerFile file)
    {
        ModInterface.Log.LogInfo("Unlocking ScallyCapFan Outfits");
        using (ModInterface.Log.MakeIndent())
        {
            foreach (var fileGirl in file.girls)
            {
                var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id);

                foreach (var outfitId in ModInterface.Data.GetAllOutfitIds(girlId).Where(x => x.SourceId == _modId))
                {
                    ModInterface.Log.LogInfo($"Unlocking outfit {outfitId} for girl {girlId}");
                    fileGirl.UnlockOutfit(ModInterface.Data.GetOutfitIndex(girlId, outfitId));
                }

                foreach (var hairstyleId in ModInterface.Data.GetAllHairstyleIds(girlId).Where(x => x.SourceId == _modId))
                {
                    ModInterface.Log.LogInfo($"Unlocking hairstyle {hairstyleId} for girl {girlId}");
                    fileGirl.UnlockHairstyle(ModInterface.Data.GetHairstyleIndex(girlId, hairstyleId));
                }
            }
        }
    }
}
