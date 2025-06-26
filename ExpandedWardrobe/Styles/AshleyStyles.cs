using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _ashleyBodyX = 387;
    private static readonly int _ashleyBodyY = 964;
    public static void AddAshleyStyles()
    {
        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modParts, modOutfits, "bikerlaces", "Biker Laves", "ashley", _ashleyBodyX, _ashleyBodyY - 96, false, false, false, true);
        AddPair(modParts, modOutfits, modHairstyles, "junko", "Junko", "ashley",
            _ashleyBodyX - 4, _ashleyBodyY - 99,
            _ashleyBodyX + 205, _ashleyBodyY + 13,
            _ashleyBodyX + 45, _ashleyBodyY + 102,
            false, false, false, true);

        //Chesticles

        ModInterface.AddDataMod(new GirlDataMod(Girls.AshleyId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits,
            hairstyles = modHairstyles
        });
    }
}