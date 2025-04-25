using System.Linq;
using BepInEx;
using Hp2BaseMod;

namespace ScallyCapFanOutfits;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Ids.Init();

        Styles.AddSarahStyles();
        Styles.AddJessieStyles();
        Styles.AddAshleyStyles();
        Styles.AddZoeyStyles();
        Styles.AddLailaniStyles();
        Styles.AddCandaceStyles();
        Styles.AddLillianStyles();
        Styles.AddKyuStyles();

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

                foreach (var outfitId in ModInterface.Data.GetAllOutfitIds(girlId).Where(x => x.SourceId == Ids.ModId))
                {
                    ModInterface.Log.LogInfo($"Unlocking outfit {outfitId} for girl {girlId}");
                    fileGirl.UnlockOutfit(ModInterface.Data.GetOutfitIndex(girlId, outfitId));
                }

                foreach (var hairstyleId in ModInterface.Data.GetAllHairstyleIds(girlId).Where(x => x.SourceId == Ids.ModId))
                {
                    ModInterface.Log.LogInfo($"Unlocking hairstyle {hairstyleId} for girl {girlId}");
                    fileGirl.UnlockHairstyle(ModInterface.Data.GetHairstyleIndex(girlId, hairstyleId));
                }
            }
        }
    }
}
