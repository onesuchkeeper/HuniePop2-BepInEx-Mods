using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _abiaBodyX = 423;
    private static readonly int _abiaBodyY = 957;
    public static void AddAbiaStyles()
    {
        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();

        AddOutfit(modParts, modOutfits, "jwoww", "Jwoww", "abia", _abiaBodyX + 22, _abiaBodyY - 269, false, false, false, true);

        //Mammaries

        ModInterface.AddDataMod(new GirlDataMod(Girls.AbiaId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits
        });
    }
}