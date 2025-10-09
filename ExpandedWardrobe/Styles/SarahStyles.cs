using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _sarahBodyX = 414;
    private static readonly int _sarahBodyY = 917;
    public static void AddSarahStyles()
    {
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modOutfits, "tsuyome", "Tsuyome", "sarah", _sarahBodyX, _sarahBodyY - 272, false, false, false, true);

        AddPair(modOutfits, modHairstyles, "yamanba", "Yamanba", "sarah",
            _sarahBodyX + 18, _sarahBodyY - 82,
            _sarahBodyX + 119, _sarahBodyY + 62,
            _sarahBodyX - 1, _sarahBodyY + 83,
            false, false, false, true);

        AddPair(modOutfits, modHairstyles, "momo", "Neko", "sarah",
            _sarahBodyX - 11, _sarahBodyY - 109,
            _sarahBodyX + 88, _sarahBodyY + 90,
            _sarahBodyX + 153, _sarahBodyY - 61,
            false, false, false, true);

        //Hooters

        ModInterface.AddDataMod(new GirlDataMod(Girls.SarahId, InsertStyle.append)
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
