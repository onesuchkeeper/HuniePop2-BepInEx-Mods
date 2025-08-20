using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _jewnBodyX = 228;
    private static readonly int _jewnBodyY = 1019;
    public static void AddJewnStyles()
    {
        //On Your Chest

        var modParts = new List<IGirlSubDataMod<GirlPartSubDefinition>>();
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modParts, modOutfits, "occult", "Occult", "jewn", _jewnBodyX + 165, _jewnBodyY - 292, true, false, false, false);

        //Shoulder Boulders

        ModInterface.AddDataMod(new GirlDataMod(Girls.JewnId, InsertStyle.append)
        {
            parts = modParts,
            outfits = modOutfits,
            hairstyles = modHairstyles
        });
    }
}