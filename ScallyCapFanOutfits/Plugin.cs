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

        ModInterface.Events.PreLoadSaveFile += On_PrePersistenceReset;
    }

    private void On_PrePersistenceReset(SaveFile file)
    {
        ModInterface.Log.LogInfo("Unlocking ScallyCapFan Outfits");

        foreach (var saveFileGirl in file.girls)
        {
            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, saveFileGirl.girlId);

            saveFileGirl.unlockedOutfits = saveFileGirl.unlockedOutfits
                .Concat(ModInterface.Data.GetAllOutfitIds(girlId).Where(x => x.SourceId == _modId)
                    .Select(x => ModInterface.Data.GetOutfitIndex(girlId, x)))
                .Distinct()
                .ToList();

            saveFileGirl.unlockedHairstyles = saveFileGirl.unlockedHairstyles
                .Concat(ModInterface.Data.GetAllHairstyleIds(girlId).Where(x => x.SourceId == _modId)
                    .Select(x => ModInterface.Data.GetHairstyleIndex(girlId, x)))
                .Distinct()
                .ToList();
        }
    }
}