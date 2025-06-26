using System.Collections.Generic;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _moxieBodyX = 262;
    private static readonly int _moxieBodyY = 956;
    public static void AddMoxieStyles()
    {
        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modParts, modOutfits, "roost", "Roost", "moxie", _moxieBodyX + 105, _moxieBodyY - 247, true, false, false, false);

        //Shoulder Boulders

        ModInterface.AddDataMod(new GirlDataMod(Girls.MoxieId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits,
            hairstyles = modHairstyles
        });
    }
}