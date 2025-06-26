using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _lailaniBodyX = 345;
    private static readonly int _lailaniBodyY = 931;
    public static void AddLailaniStyles()
    {
        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modParts, modOutfits, "keyWest", "Key West", "lailani", _lailaniBodyX + 76, _lailaniBodyY - 249, false, false, false, true);
        AddOutfit(modParts, modOutfits, "topless", "Chest Puppies", "lailani", _lailaniBodyX + 101, _lailaniBodyY - 284, true, false, false, false);

        ModInterface.AddDataMod(new GirlDataMod(Girls.LailaniId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits,
            hairstyles = modHairstyles
        });
    }
}