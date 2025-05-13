using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ScallyCapFanOutfits;

internal static partial class Styles
{
    private static readonly int _ashleyBodyX = 387;
    private static readonly int _ashleyBodyY = 964;
    public static void AddAshleyStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.AshleyId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
                {
                    PartType = GirlPartType.OUTFIT,
                    PartName = "bikerLacesOutfitAshley",
                    X = _ashleyBodyX,
                    Y = _ashleyBodyY - 96,
                    MirroredPartId = RelativeId.Default,
                    AltPartId = null,
                    SpriteInfo = new SpriteInfoPath()
                    {
                        IsExternal = true,
                        Path = Path.Combine(Plugin.ImageDir, @"ashley_outfit_ashley.png")
                    }
                }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Biker Laces",
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