using System.Collections.Generic;
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
        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modParts, modOutfits, "plastic", "Plastic", "brooke", _brookeBodyX + 2, _brookeBodyY - 234, false, false, false, true);

        AddPair(modParts, modOutfits, modHairstyles, "warcrimes", "M. R. E.", "brooke",
            _brookeBodyX + 44, _brookeBodyY - 269,
            _brookeBodyX + 163, _brookeBodyY + 38,
            _brookeBodyX + 258, _brookeBodyY - 115,
            false, false, false, true);

        AddOutfit(modParts, modOutfits, "topless", "Jubblies", "brooke", _brookeBodyX + 105, _brookeBodyY - 643, true, false, false, false);

        ModInterface.AddDataMod(new GirlDataMod(Girls.BrookeId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits,
            hairstyles = modHairstyles
        });
    }
}