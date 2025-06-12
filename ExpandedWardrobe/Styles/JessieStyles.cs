using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _jessieBodyX = 457;
    private static readonly int _jessieBodyY = 983;
    public static void AddJessieStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.JessieId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "businessCasualOutfitJessie",
                    X = _jessieBodyX - 3,
                    Y = _jessieBodyY - 261,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"jessie_outfit_businesscasual.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart2, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "milfOutfitJessie",
                    X = _jessieBodyX - 3,
                    Y = _jessieBodyY - 227,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"jessie_outfit_milf.png")))
                },
                new GirlPartDataMod(Ids.OutfitPart3, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "marlenaOutfitJessie",
                    X = _jessieBodyX + 1,
                    Y = _jessieBodyY - 406,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"jessie_outfit_marlena.png")))
                },
                new GirlPartDataMod(Ids.FronthairPart3, InsertStyle.replace)
                {
                    PartType = GirlPartType.FRONTHAIR,
                    PartName = "marlenaFronthairJessie",
                    X = _jessieBodyX + 54,
                    Y = _jessieBodyY + 10,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"jessie_fronthair_marlena.png")))
                },
                new GirlPartDataMod(Ids.BackhairPart3, InsertStyle.replace)
                {
                    PartType = GirlPartType.BACKHAIR,
                    PartName = "marlenaBackhairJessie",
                    X = _jessieBodyX + 58,
                    Y = _jessieBodyY - 102,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, @"jessie_backhair_marlena.png")))
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Office Siren",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                },
                new OutfitDataMod(Ids.Style2, InsertStyle.replace)
                {
                    Name = "Milf",
                    OutfitPartId = Ids.OutfitPart2,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                },
                new OutfitDataMod(Ids.Style3, InsertStyle.replace)
                {
                    Name = "Bombshell",
                    OutfitPartId = Ids.OutfitPart3,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = true,
                    PairHairstyleId = Ids.Style3
                }
            },
            hairstyles = new List<IGirlSubDataMod<ExpandedHairstyleDefinition>>()
            {
                new HairstyleDataMod(Ids.Style3, InsertStyle.replace)
                {
                    Name = "Bombshell",
                    FrontHairPartId = Ids.FronthairPart3,
                    BackHairPartId = Ids.BackhairPart3,
                    IsNSFW = false,
                    TightlyPaired = true,
                    PairOutfitId = Ids.Style3
                }
            }
        });
    }
}