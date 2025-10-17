using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ExpandedWardrobe;

internal static partial class Styles
{
    private static readonly int _lillianBodyX = 435;
    private static readonly int _lillianBodyY = 918;
    public static void AddLillianStyles()
    {
        var modOutfits = new List<IBodySubDataMod<GirlOutfitSubDefinition>>();
        var modHairstyles = new List<IBodySubDataMod<GirlHairstyleSubDefinition>>();

        AddPair(modOutfits, modHairstyles, "sceneQueen", "Scene Queen", "lillian",
            _lillianBodyX - 17, _lillianBodyY - 208,
            _lillianBodyX + 111, _lillianBodyY + 31,
            _lillianBodyX + 71, _lillianBodyY + 73,
            false, false, false, true);

        ((GirlPartDataMod)modOutfits.Last()).MirroredPart = new GirlPartDataMod(new RelativeId(Ids.ModId, _partCount++), InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "sceneQueenOutfitLillianMirror",
            X = _lillianBodyX - 17,
            Y = _lillianBodyY - 208,
            SpriteInfo = new SpriteInfoTexture(new TextureInfoExternal(Path.Combine(Plugin.ImageDir, "lillian_outfit_sceneQueen_mirror.png")))
        };

        AddPair(modOutfits, modHairstyles, "lolita", "Lolita", "lillian",
            _lillianBodyX - 149, _lillianBodyY - 227,
            _lillianBodyX - 7, _lillianBodyY + 67,
            _lillianBodyX - 48, _lillianBodyY - 185,
            false, false, false, true);

        AddOutfit(modOutfits, "topless", "Double D's", "lillian", _lillianBodyX - 2, _lillianBodyY - 225, true, false, false, false);
        AddOutfit(modOutfits, "batMicro", "Anarchy", "lillian", _lillianBodyX + 3, _lillianBodyY - 203, true, false, false, true);

        ModInterface.AddDataMod(new GirlDataMod(Girls.LillianId, InsertStyle.append)
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
