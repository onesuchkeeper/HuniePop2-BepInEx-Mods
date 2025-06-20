using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _lillianBodyX = 435;
    private static readonly int _lillianBodyY = 918;
    public static void AddLillianStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.LillianId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1Mirror, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "sceneQueenOutfitLillianMirror",
                    X = _lillianBodyX - 17,
                    Y = _lillianBodyY - 208,
                    MirroredPartId = Ids.OutfitPart1,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, "lillian_outfit_sceneQueen_mirror.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "sceneQueenOutfitLillian",
                    X = _lillianBodyX - 17,
                    Y = _lillianBodyY - 208,
                    MirroredPartId = Ids.OutfitPart1Mirror,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, "lillian_outfit_sceneQueen.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "sceneQueenFronthairLillian",
                    X = _lillianBodyX + 111,
                    Y = _lillianBodyY + 31,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, "lillian_fronthair_sceneQueen.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "sceneQueenBackhairLillian",
                    X = _lillianBodyX + 71,
                    Y = _lillianBodyY + 73,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, "lillian_backhair_sceneQueen.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "lolitaOutfitLillian",
                    X = _lillianBodyX - 149,
                    Y = _lillianBodyY - 227,
                    MirroredPartId = Ids.OutfitPart1Mirror,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, "lillian_outfit_lolita.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "lolitaFronthairLillian",
                    X = _lillianBodyX - 7,
                    Y = _lillianBodyY + 67,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, "lillian_fronthair_lolita.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "lolitaBackhairLillian",
                    X = _lillianBodyX - 48,
                    Y = _lillianBodyY - 185,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, "lillian_backhair_lolita.png")))
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Scene Queen",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = Ids.Style1
                },
                new OutfitDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Lolita",
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
                    Name = "Scene Queen",
                    BackHairPartId = Ids.BackhairPart1,
                    FrontHairPartId = Ids.FronthairPart1,
                    IsNSFW = false,
                    HideSpecials = false,
                    TightlyPaired = true,
                    PairOutfitId = Ids.Style1
                },
                new HairstyleDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Lolita",
                    BackHairPartId = Ids.BackhairPart2,
                    FrontHairPartId = Ids.FronthairPart2,
                    IsNSFW = false,
                    HideSpecials = false,
                    TightlyPaired = true,
                    PairOutfitId = Ids.Style2
                }
            }
        });
    }
}