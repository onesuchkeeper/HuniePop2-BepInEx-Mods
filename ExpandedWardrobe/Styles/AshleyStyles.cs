using System.Collections.Generic;
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
        var modOutfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IBodySubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modOutfits, "bikerlaces", "Biker Laces", "ashley", _ashleyBodyX, _ashleyBodyY - 96, false, false, false, true);
        AddPair(modOutfits, modHairstyles, "junko", "Junko", "ashley",
            _ashleyBodyX - 4, _ashleyBodyY - 99,
            _ashleyBodyX + 205, _ashleyBodyY + 13,
            _ashleyBodyX + 45, _ashleyBodyY + 102,
            false, false, false, true);

        //Chesticles

        ModInterface.AddDataMod(new GirlDataMod(Girls.Ashley, InsertStyle.append)
        {
            bodies = new List<IGirlBodyDataMod>()
            {
                new GirlBodyDataMod(new RelativeId(-1,0), InsertStyle.append)
                {
                    outfits = modOutfits,
                    hairstyles = modHairstyles
                }
            }
        });
    }
}