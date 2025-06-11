using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
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
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"kyu_outfit_pamu.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "pamuBackhairKyu",
                    X = _kyuBodyX - 58,
                    Y = _kyuBodyY + 98,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"kyu_backhair_pamu.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "pamuFronthairKyu",
                    X = _kyuBodyX + 121,
                    Y = _kyuBodyY + 9,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"kyu_fronthair_pamu.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "microOutfitKyu",
                    X = _kyuBodyX + 138,
                    Y = _kyuBodyY - 272,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"kyu_outfit_micro.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "microBackhairKyu",
                    X = _kyuBodyX + 146,
                    Y = _kyuBodyY - 152,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"kyu_backhair_micro.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "microFronthairKyu",
                    X = _kyuBodyX + 89,
                    Y = _kyuBodyY + 25,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"kyu_fronthair_micro.png")))
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
                },
                new OutfitDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Micro",
                    OutfitPartId = Ids.OutfitPart2,
                    IsNSFW = true,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = Ids.Style2
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
                    PairOutfitId = Ids.Style1,
                    HideSpecials = true
                },
                new HairstyleDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Micro",
                    FrontHairPartId = Ids.FronthairPart2,
                    BackHairPartId = Ids.BackhairPart2,
                    IsNSFW = false,
                    PairOutfitId = Ids.Style2
                }
            }
        });
    }
}