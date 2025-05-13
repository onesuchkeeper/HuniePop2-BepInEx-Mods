using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ScallyCapFanOutfits;

internal static partial class Styles
{
    private static readonly int _abiaBodyX = 423;
    private static readonly int _abiaBodyY = 957;
    public static void AddAbiaStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.AbiaId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "jwowwOutfitAbia",
                    X = _abiaBodyX + 22,
                    Y = _abiaBodyY - 269,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoPath()
                    {
                        IsExternal = true,
                        Path = Path.Combine(Plugin.ImageDir, @"abia_outfit_jwoww.png")
                    }
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Jwoww",
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