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
        var modOutfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IBodySubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modOutfits, "occult", "Occult", "jewn", _jewnBodyX + 165, _jewnBodyY - 292, true, false, false, false);

        //Shoulder Boulders

        ModInterface.AddDataMod(new GirlDataMod(Girls.JewnId, InsertStyle.append)
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