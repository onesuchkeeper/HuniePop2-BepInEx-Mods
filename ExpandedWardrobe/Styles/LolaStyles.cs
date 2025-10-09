using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _lolaBodyX = 414;
    private static readonly int _lolaBodyY = 985;
    public static void AddLolaStyles()
    {
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddPair(modOutfits, modHairstyles, "nostalgia", "Nostalgia", "lola",
            _lolaBodyX + 24, _lolaBodyY - 284,
            _lolaBodyX + 164, _lolaBodyY + 7,
            _lolaBodyX + 66, _lolaBodyY + 50,
            false, false, false, true);

        AddOutfit(modOutfits, "topless", "Tits", "lola", _lolaBodyX - 4, _lolaBodyY - 675, true, false, false, false);

        ModInterface.AddDataMod(new GirlDataMod(Girls.LolaId, InsertStyle.append)
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