using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _sarahBodyX = 414;
    private static readonly int _sarahBodyY = 917;
    public static void AddSarahStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.SarahId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "tsuyomeOutfitSarah",
                    X = _sarahBodyX,
                    Y = _sarahBodyY - 272,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"sarah_outfit_tsuyome.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "sarahYamanbaOutfitPart",
                    X = _sarahBodyX + 18,
                    Y = _sarahBodyY - 82,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"sarah_outfit_yamanba.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "sarahYamanbaFronthairPart",
                    X = _sarahBodyX + 119,
                    Y = _sarahBodyY + 62,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"sarah_fronthair_yamanba.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "sarahYamanbaBackhairPart",
                    X = _sarahBodyX - 1,
                    Y = _sarahBodyY + 83,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"sarah_backhair_yamanba.png")))
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Tsuyome",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                },
                new OutfitDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Yamanba",
                    OutfitPartId = Ids.OutfitPart2,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = Ids.Style2
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Yamanba",
                    BackHairPartId = Ids.BackhairPart1,
                    FrontHairPartId = Ids.FronthairPart1,
                    IsNSFW = false,
                    HideSpecials = false,
                    TightlyPaired = true,
                    PairOutfitId = Ids.Style2
                }
            }
        });
    }
}