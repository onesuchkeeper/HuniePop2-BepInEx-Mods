using System.Collections.Generic;
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
        var modOutfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IBodySubDataMod<GirlHairstyleSubDefinition>>();

        AddPair(modOutfits, modHairstyles, "hime", "Hime", "candace",
            _candaceBodyX + 107, _candaceBodyY - 184,
            _candaceBodyX + 176, _candaceBodyY + 35,
            _candaceBodyX + 150, _candaceBodyY - 50,
            false, false, false, true);

        AddPair(modOutfits, modHairstyles, "backwoods", "Backwoods Bottom", "candace",
            _candaceBodyX + 107, _candaceBodyY - 277,
            _candaceBodyX + 192, _candaceBodyY + 58,
            _candaceBodyX + 251, _candaceBodyY - 113,
            false, false, false, true);

        AddPair(modOutfits, modHairstyles, "clown", "Clussy", "candace",
            _candaceBodyX - 2, _candaceBodyY - 168,
            _candaceBodyX + 210, _candaceBodyY + 78,
            _candaceBodyX + 455, _candaceBodyY - 203,
            false, false, false, true);

        AddOutfit(modOutfits, "topless", "Boobs", "candace", _candaceBodyX + 207, _candaceBodyY - 248, true, false, false, false);

        ModInterface.AddDataMod(new GirlDataMod(Girls.Candace, InsertStyle.append)
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