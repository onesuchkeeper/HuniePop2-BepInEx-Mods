using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _kyuBodyX = 420;
    private static readonly int _kyuBodyY = 968;
    public static void AddKyuStyles()
    {
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddPair(modOutfits, modHairstyles, "pamu", "Crushed", "kyu",
            _kyuBodyX - 38, _kyuBodyY - 383,
            _kyuBodyX + 121, _kyuBodyY + 9,
            _kyuBodyX - 58, _kyuBodyY + 98,
            false, false, false, true);

        ((OutfitDataMod)modOutfits.Last()).HideSpecial = true;

        AddPair(modOutfits, modHairstyles, "micro", "Valentine", "kyu",
            _kyuBodyX + 138, _kyuBodyY - 272,
            _kyuBodyX + 89, _kyuBodyY + 25,
            _kyuBodyX + 146, _kyuBodyY - 152,
            true, false, false, true);

        AddOutfit(modOutfits, "topless", "Big Breasts", "kyu", _kyuBodyX + 96, _kyuBodyY - 624, true, false, false, false);

        ModInterface.AddDataMod(new GirlDataMod(Girls.KyuId, InsertStyle.append)
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
