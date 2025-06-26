using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _noraBodyX = 478;
    private static readonly int _noraBodyY = 966;
    public static void AddNoraStyles()
    {
        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modParts, modOutfits, "hotline", "Hotline", "nora", _noraBodyX - 11, _noraBodyY - 250, false, false, false, true);
        AddOutfit(modParts, modOutfits, "topless", "Melons", "nora", _noraBodyX - 24, _noraBodyY - 810, true, false, false, false);

        ModInterface.AddDataMod(new GirlDataMod(Girls.NoraId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits,
            hairstyles = modHairstyles
        });
    }
}