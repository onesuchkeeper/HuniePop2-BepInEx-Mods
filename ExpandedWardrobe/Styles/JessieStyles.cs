using System.Collections.Generic;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _jessieBodyX = 457;
    private static readonly int _jessieBodyY = 983;
    public static void AddJessieStyles()
    {
        var modOutfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IBodySubDataMod<GirlHairstyleSubDefinition>>();

        AddOutfit(modOutfits, "businesscasual", "Office Siren", "jessie", _jessieBodyX - 3, _jessieBodyY - 261, true, false, false, false);
        AddOutfit(modOutfits, "milf", "MILF", "Jessie", _jessieBodyX - 3, _jessieBodyY - 227, true, false, false, false);

        AddPair(modOutfits, modHairstyles, "marlena", "Bombshell", "jessie",
            _jessieBodyX + 1, _jessieBodyY - 406,
            _jessieBodyX + 54, _jessieBodyY + 10,
            _jessieBodyX + 58, _jessieBodyY - 102,
            false, false, false, true);

        //Knockers

        ModInterface.AddDataMod(new GirlDataMod(Girls.Jessie, InsertStyle.append)
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