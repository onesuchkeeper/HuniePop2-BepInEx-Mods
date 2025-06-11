using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _brookeBodyX = 378;
    private static readonly int _brookeBodyY = 960;
    public static void AddBrookeStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.BrookeId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "warcrimesOutfitBrooke",
                    X = _brookeBodyX + 44,
                    Y = _brookeBodyY - 269,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"brooke_outfit_warcrimes.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "warcrimesFronthairBrooke",
                    X = _brookeBodyX + 163,
                    Y = _brookeBodyY + 38,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"brooke_fronthair_warcrimes.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "warcrimesBackhairBrooke",
                    X = _brookeBodyX + 258,
                    Y = _brookeBodyY - 115,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"brooke_backhair_warcrimes.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "plasticOutfitBrooke",
                    X = _brookeBodyX + 2,
                    Y = _brookeBodyY - 234,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"brooke_outfit_plastic.png")))
                },
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "M. R. E.",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = Ids.Style1
                },
                new OutfitDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Plastic",
                    OutfitPartId = Ids.OutfitPart2,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "M. R. E.",
                    FrontHairPartId = Ids.FronthairPart1,
                    BackHairPartId = Ids.BackhairPart1,
                    IsNSFW = false,
                    TightlyPaired = true,
                    PairOutfitId = Ids.Style1
                }
            }
        });
    }
}