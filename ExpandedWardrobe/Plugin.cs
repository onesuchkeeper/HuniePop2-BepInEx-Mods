using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using Hp2BaseMod;

namespace ExpandedWardrobe;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    public static readonly string ModDir = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
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

        if (Chainloader.PluginInfos.ContainsKey("OSK.BepInEx.Hp2BaseModTweaks"))
        {
            var configs = ModInterface.GetInterModValue<Dictionary<string, (string ModImagePath, List<(string CreditButtonPath, string CreditButtonOverPath, string RedirectLink)> CreditEntries)>>("OSK.BepInEx.Hp2BaseModTweaks", "ModCredits");

            configs[MyPluginInfo.PLUGIN_GUID] = (
                Path.Combine(ImageDir, "CreditsLogo.png"),
                new List<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>(){
                    (
                        Path.Combine(ImageDir, "ScallyCapFan_credits.png"),
                        Path.Combine(ImageDir, "ScallyCapFan_credits_over.png"),
                        "https://www.reddit.com/user/scallycapfan/"
                    ),
                    (
                        Path.Combine(ImageDir, "onesuchkeeper_credits.png"),
                        Path.Combine(ImageDir, "onesuchkeeper_credits_over.png"),
                        "https://www.youtube.com/@onesuchkeeper8389"
                    ),
                }
            );
        }

        ModInterface.Events.PreLoadPlayerFile += On_PrePersistenceReset;
    }

    private void On_PrePersistenceReset(PlayerFile file)
    {
        using (ModInterface.Log.MakeIndent())
        {
            foreach (var fileGirl in file.girls)
            {
                var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id);
                var expansion = ExpandedGirlDefinition.Get(girlId);

                foreach (var outfitId_Index in expansion.OutfitIdToIndex.Where(x => x.Key.SourceId == Ids.ModId))
                {
                    fileGirl.UnlockOutfit(outfitId_Index.Value);
                }

                foreach (var hairstyleId_index in expansion.HairstyleIdToIndex.Where(x => x.Key.SourceId == Ids.ModId))
                {
                    fileGirl.UnlockHairstyle(hairstyleId_index.Value);
                }
            }
        }
    }
}
