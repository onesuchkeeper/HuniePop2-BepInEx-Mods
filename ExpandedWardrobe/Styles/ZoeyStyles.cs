using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _zoeyBodyX = 522;
    private static readonly int _zoeyBodyY = 900;
    public static void AddZoeyStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.ZoeyId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.BackhairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "extraterrestrialBackhairZoey",
                    X = _zoeyBodyX + 65,
                    Y = _zoeyBodyY + 33,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"zoey_backhair_extraterrestrial.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "extraterrestrialFronthairZoey",
                    X = _zoeyBodyX + 26,
                    Y = _zoeyBodyY + 21,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"zoey_fronthair_extraterrestrial.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "extraterrestrialOutfitZoey",
                    X = _zoeyBodyX - 3,
                    Y = _zoeyBodyY - 215,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"zoey_outfit_extraterrestrial.png")))
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Extraterrestrial",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = Ids.Style1,
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Extraterrestrial",
                    BackHairPartId = Ids.BackhairPart1,
                    FrontHairPartId = Ids.FronthairPart1,
                    IsNSFW = false,
                    HideSpecials = false,
                    TightlyPaired = true,
                    PairOutfitId = Ids.Style1
                }
            }
        });
    }
}