using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _pollyBodyX = 384;
    private static readonly int _pollyBodyY = 949;
    public static void AddPollyStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.PollyId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "finalGirlOutfitPolly",
                    X = _pollyBodyX + 53,
                    Y = _pollyBodyY - 245,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"polly_outfit_finalGirl.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "finalGirlFronthairPolly",
                    X = _pollyBodyX + 156,
                    Y = _pollyBodyY + 35,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"polly_fronthair_finalGirl.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "finalGirlBackhairPolly",
                    X = _pollyBodyX + 187,
                    Y = _pollyBodyY - 142,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"polly_backhair_finalGirl.png")))
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Final Girl",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = Ids.Style1
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Final Girl",
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