using System.Collections.Generic;
using System.IO;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

public static partial class Styles
{
    private static readonly int _kyuBodyX = 420;
    private static readonly int _kyuBodyY = 968;
    public static void AddKyuStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.KyuId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "pamuOutfitKyu",
                    X = _kyuBodyX - 38,
                    Y = _kyuBodyY - 383,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoPath()
                    {
                        IsExternal = true,
                        Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\kyu_outfit_pamu.png")
                    }
                },
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "pamuBackhairKyu",
                    X = _kyuBodyX - 57,
                    Y = _kyuBodyY + 79,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoPath()
                    {
                        IsExternal = true,
                        Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\kyu_backhair_pamu.png")
                    }
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "pamuFronthairKyu",
                    X = _kyuBodyX + 121,
                    Y = _kyuBodyY + 9,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoPath()
                    {
                        IsExternal = true,
                        Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\kyu_fronthair_pamu.png")
                    }
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Crushed",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = Ids.Style1
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Crushed",
                    FrontHairPartId = Ids.FronthairPart1,
                    BackHairPartId = Ids.BackhairPart1,
                    IsNSFW = false,
                    PairOutfitId = Ids.Style1
                }
            }
        });
    }
}