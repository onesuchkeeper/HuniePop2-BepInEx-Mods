using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace ScallyCapFanOutfits;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private static RelativeId _styleId_1;
    private static int _modId;

    private void Awake()
    {
        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        _styleId_1 = new RelativeId(_modId, 0);

        var outfitPartId_1 = new RelativeId(_modId, 0);
        var hairstyleFrontPartId_1 = new RelativeId(_modId, 1);
        var hairstyleBackPartId_1 = new RelativeId(_modId, 2);

        var ashleyBikerPart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "ashleyOutfitAshley",
            X = 387,
            Y = 868,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\ashley_outfit_ashley.png")
            }
        };

        var sarahTsuyomePart = new GirlPartDataMod(outfitPartId_1, InsertStyle.replace)
        {
            PartType = GirlPartType.OUTFIT,
            PartName = "tsuyomeOutfitSarah",
            X = 414,
            Y = 645,
            MirroredPartId = RelativeId.Default,
            AltPartId = null,
            SpriteInfo = new SpriteInfoPath()
            {
                IsExternal = true,
                Path = Path.Combine(Paths.PluginPath, @"ScallyCapFanOutfits\images\sarah_outfit_tsuyome.png")
            }
        };

        ModInterface.AddDataMod(new GirlDataMod(Girls.SarahId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>() { sarahTsuyomePart },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Tsuyome",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                }
            }
        });

        ModInterface.AddDataMod(new GirlDataMod(Girls.AshleyId, InsertStyle.append)
        {
            parts = new List<IGirlSubDataMod<GirlPartSubDefinition>>() { ashleyBikerPart },
            outfits = new List<IGirlSubDataMod<ExpandedOutfitDefinition>>()
            {
                new OutfitDataMod(_styleId_1, InsertStyle.replace)
                {
                    Name = "Biker Laces",
                    OutfitPartId = outfitPartId_1,
                    IsNSFW = false,
                    HideNipples = true,
                    TightlyPaired = false,
                    PairHairstyleId = null
                }
            }
        });

        ModInterface.Events.PreLoadPlayerFile += On_PrePersistenceReset;
    }

    private void On_PrePersistenceReset(PlayerFile file)
    {
        ModInterface.Log.LogInfo("Unlocking ScallyCapFan Outfits");
        using (ModInterface.Log.MakeIndent())
        {
            foreach (var fileGirl in file.girls)
            {
                var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id);

                foreach (var outfitId in ModInterface.Data.GetAllOutfitIds(girlId).Where(x => x.SourceId == _modId))
                {
                    ModInterface.Log.LogInfo($"Unlocking outfit {outfitId} for girl {girlId}");
                    fileGirl.UnlockOutfit(ModInterface.Data.GetOutfitIndex(girlId, outfitId));
                }

                foreach (var hairstyleId in ModInterface.Data.GetAllHairstyleIds(girlId).Where(x => x.SourceId == _modId))
                {
                    ModInterface.Log.LogInfo($"Unlocking hairstyle {hairstyleId} for girl {girlId}");
                    fileGirl.UnlockHairstyle(ModInterface.Data.GetHairstyleIndex(girlId, hairstyleId));
                }
            }
        }
    }
}