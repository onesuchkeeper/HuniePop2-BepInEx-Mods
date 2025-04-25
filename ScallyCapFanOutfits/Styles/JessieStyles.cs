using System.Collections.Generic;
using System.IO;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

public static partial class Styles
{
    private static readonly int _jessieBodyX = 457;
    private static readonly int _jessieBodyY = 983;
    public static void AddJessieStyles()
    {
        ModInterface.AddDataMod(new GirlDataMod(Girls.JessieId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>()
            {
                new GirlPartDataMod(Ids.OutfitPart1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "businessCasualOutfitJessie",
            X = _jessieBodyX - 3,
            Y = _jessieBodyY - 261,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\jessie_outfit_businesscasual.png")
            }
        }
            },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(Ids.Style1, InsertStyle.replace)
                {
                    Name = "Office Siren",
                    OutfitPartId = Ids.OutfitPart1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                }
            }
        });
    }
}