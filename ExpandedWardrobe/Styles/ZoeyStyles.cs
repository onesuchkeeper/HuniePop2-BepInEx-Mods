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
                },
                new GirlPartDataMod(Ids.BackhairPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "freakshitBackhairZoey",
                    X = _zoeyBodyX + 5,
                    Y = _zoeyBodyY + 46,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"zoey_backhair_freakshit.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "freakshitFronthairZoey",
                    X = _zoeyBodyX + 23,
                    Y = _zoeyBodyY + 19,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"zoey_fronthair_freakshit.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "freakshitOutfitZoey",
                    X = _zoeyBodyX - 11,
                    Y = _zoeyBodyY - 247,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"zoey_outfit_freakshit.png")))
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
                },
                new OutfitDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Freak Shit",
                    OutfitPartId = Ids.OutfitPart2,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = Ids.Style2,
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
                },
                new HairstyleDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Freak Shit",
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
