using System.IO;
using System.Linq;
using BepInEx;
using Hp2BaseMod;

namespace ScallyCapFanOutfits;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
internal class Plugin : BaseUnityPlugin
{
    public static readonly string ModDir = Path.Combine(Paths.PluginPath, "ScallyCapFanOutfits");
    public static readonly string ImageDir = Path.Combine(ModDir, "images");

    private void Awake()
    {
        Ids.Init();

        Styles.AddAbiaStyles();
        Styles.AddAshleyStyles();
        Styles.AddBrookeStyles();
        Styles.AddCandaceStyles();
        Styles.AddJessieStyles();
        Styles.AddJewnStyles();
        Styles.AddKyuStyles();
        Styles.AddLailaniStyles();
        Styles.AddLillianStyles();
        Styles.AddLolaStyles();
        Styles.AddMoxieStyles();
        Styles.AddJewnStyles();
        Styles.AddSarahStyles();
        Styles.AddZoeyStyles();

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
                var expansion = ExpandedGirlDefinition.Get(girlId);

                foreach (var outfitId_Index in expansion.OutfitIdToIndex.Where(x => x.Key.SourceId == Ids.ModId))
                {
                    ModInterface.Log.LogInfo($"Unlocking outfit {outfitId_Index} for girl {girlId}");
                    fileGirl.UnlockOutfit(outfitId_Index.Value);
                }

                foreach (var hairstyleId_index in expansion.HairstyleIdToIndex.Where(x => x.Key.SourceId == Ids.ModId))
                {
                    ModInterface.Log.LogInfo($"Unlocking hairstyle {hairstyleId_index} for girl {girlId}");
                    fileGirl.UnlockHairstyle(hairstyleId_index.Value);
                }
            }
        }
    }
}
