using System.Collections.Generic;
using System.IO;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

public static partial class Styles
{
    private static readonly int _candaceBodyX = 348;
    private static readonly int _candaceBodyY = 972;
    public static void AddCandaceStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.CandaceId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "himeOutfitCandace",
                    X = _candaceBodyX + 107,
                    Y = _candaceBodyY - 184,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoPath()
                    {
                        IsExternal = true,
                        Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\candace_outfit_hime.png")
                    }
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "himeFronthairCandace",
                    X = _candaceBodyX + 176,
                    Y = _candaceBodyY + 35,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoPath()
                    {
                        IsExternal = true,
                        Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\candace_fronthair_hime.png")
                    }
                },
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "himeBackhairCandace",
                    X = _candaceBodyX + 150,
                    Y = _candaceBodyY - 50,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoPath()
                    {
                        IsExternal = true,
                        Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\candace_backhair_hime.png")
                    }
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Key West",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                }
            }
        });
    }
}