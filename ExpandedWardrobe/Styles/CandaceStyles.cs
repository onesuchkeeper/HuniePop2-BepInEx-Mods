using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;
internal static partial class Styles
{
    private static readonly int _candaceBodyX = 348;
    private static readonly int _candaceBodyY = 972;
    public static void AddCandaceStyles()
    {
        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddPair(modParts, modOutfits, modHairstyles, "hime", "Hime", "candace",
            _candaceBodyX + 107, _candaceBodyY - 184,
            _candaceBodyX + 176, _candaceBodyY + 35,
            _candaceBodyX + 150, _candaceBodyY - 50,
            false, false, false, true);

        AddPair(modParts, modOutfits, modHairstyles, "backwoods", "Backwoods", "candace",
            _candaceBodyX + 107, _candaceBodyY - 277,
            _candaceBodyX + 192, _candaceBodyY + 58,
            _candaceBodyX + 251, _candaceBodyY - 113,
            false, false, false, true);

        AddPair(modParts, modOutfits, modHairstyles, "clown", "Clussy", "candace",
            _candaceBodyX - 2, _candaceBodyY - 168,
            _candaceBodyX + 210, _candaceBodyY + 78,
            _candaceBodyX + 455, _candaceBodyY - 203,
            false, false, false, true);

        AddOutfit(modParts, modOutfits, "topless", "Boobs", "candace", _candaceBodyX + 207, _candaceBodyY - 248, true, false, false, false);

        ModInterface.AddDataMod(new GirlDataMod(Girls.CandaceId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits,
            hairstyles = modHairstyles
        });
    }
}