using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;
internal static partial class Styles
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
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"candace_outfit_hime.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "himeFronthairCandace",
                    X = _candaceBodyX + 176,
                    Y = _candaceBodyY + 35,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"candace_fronthair_hime.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "himeBackhairCandace",
                    X = _candaceBodyX + 150,
                    Y = _candaceBodyY - 50,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"candace_backhair_hime.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "backwoodsOutfitCandace",
                    X = _candaceBodyX + 107,
                    Y = _candaceBodyY - 277,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"candace_outfit_backwoods.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "backwoodsFronthairCandace",
                    X = _candaceBodyX + 192,
                    Y = _candaceBodyY + 58,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"candace_fronthair_backwoods.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "backwoodsBackhairCandace",
                    X = _candaceBodyX + 251,
                    Y = _candaceBodyY - 113,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"candace_backhair_backwoods.png")))
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Hime",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = Ids.Style1
                },
                new OutfitDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Backwoods",
                    OutfitPartId = Ids.OutfitPart2,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = Ids.Style2
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Hime",
                    FrontHairPartId = Ids.FronthairPart1,
                    BackHairPartId = Ids.BackhairPart1,
                    IsNSFW = false,
                    TightlyPaired = true,
                    PairOutfitId = Ids.Style1
                },
                new HairstyleDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Backwoods",
                    FrontHairPartId = Ids.FronthairPart2,
                    BackHairPartId = Ids.BackhairPart2,
                    IsNSFW = false,
                    TightlyPaired = true,
                    PairOutfitId = Ids.Style2
                }
            }
        });
    }
}