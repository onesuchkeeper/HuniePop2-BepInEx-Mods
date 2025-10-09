using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _pollyBodyX = 384;
    private static readonly int _pollyBodyY = 949;
    public static void AddPollyStyles()
    {
        var modOutfits = new List<IGirlSubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IGirlSubDataMod<GirlHairstyleSubDefinition>>();

        AddPair(modOutfits, modHairstyles, "finalGirl", "Final Girl", "polly",
            _pollyBodyX + 53, _pollyBodyY - 245,
            _pollyBodyX + 156, _pollyBodyY + 35,
            _pollyBodyX + 187, _pollyBodyY - 142,
            false, false, false, true);

        //Milk Jugs

        ModInterface.AddDataMod(new GirlDataMod(Girls.PollyId, InsertStyle.append)
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