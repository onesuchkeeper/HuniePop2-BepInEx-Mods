using System.Collections.Generic;
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
        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddPair(modParts, modOutfits, modHairstyles, "extraterrestrial", "Extraterrestrial", "zoey",
            _zoeyBodyX - 3, _zoeyBodyY - 215,
            _zoeyBodyX + 26, _zoeyBodyY + 21,
            _zoeyBodyX + 65, _zoeyBodyY + 33,
            false, false, false, true);

        AddPair(modParts, modOutfits, modHairstyles, "freakshit", "Freak Shit", "zoey",
            _zoeyBodyX - 11, _zoeyBodyY - 247,
            _zoeyBodyX + 23, _zoeyBodyY + 19,
            _zoeyBodyX + 5, _zoeyBodyY + 46,
            false, false, false, true);

        AddPair(modParts, modOutfits, modHairstyles, "tropical", "Tropical", "zoey",
            _zoeyBodyX - 8, _zoeyBodyY - 209,
            _zoeyBodyX + 13, _zoeyBodyY + 71,
            _zoeyBodyX + 121, _zoeyBodyY + 71,
            false, false, false, true);

        //Fun Bags

        ModInterface.AddDataMod(new GirlDataMod(Girls.ZoeyId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits,
            hairstyles = modHairstyles
        });
    }
}
