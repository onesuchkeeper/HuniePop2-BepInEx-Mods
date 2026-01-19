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
        var modOutfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IBodySubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modOutfits, "keyWest", "Key West", "lailani", _lailaniBodyX + 76, _lailaniBodyY - 249, false, false, false, true);
        AddOutfit(modOutfits, "topless", "Chest Puppies", "lailani", _lailaniBodyX + 101, _lailaniBodyY - 284, true, false, false, false);

        ModInterface.AddDataMod(new GirlDataMod(Girls.Lailani, InsertStyle.append)
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