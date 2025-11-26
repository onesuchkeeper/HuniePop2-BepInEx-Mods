using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Hp2BaseMod;

namespace ExpandedWardrobe;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private static readonly string CONFIG_CAT_STYLES = "styles";
    private static readonly string CONFIG_PROP_STYLES_ABIA = "abia styles";
    private static readonly string CONFIG_PROP_STYLES_ASHLEY = "ashley styles";
    private static readonly string CONFIG_PROP_STYLES_BROOKE = "brooke styles";
    private static readonly string CONFIG_PROP_STYLES_CANDACE = "candace styles";
    private static readonly string CONFIG_PROP_STYLES_JESSIE = "jessie styles";
    private static readonly string CONFIG_PROP_STYLES_JEWN = "abia styles";
    private static readonly string CONFIG_PROP_STYLES_KYU = "kyu styles";
    private static readonly string CONFIG_PROP_STYLES_LAILANI = "lailani styles";
    private static readonly string CONFIG_PROP_STYLES_LILLIAN = "lillian styles";
    private static readonly string CONFIG_PROP_STYLES_LOLA = "lola styles";
    private static readonly string CONFIG_PROP_STYLES_MOXIE = "moxie styles";
    private static readonly string CONFIG_PROP_STYLES_NORA = "nora styles";
    private static readonly string CONFIG_PROP_STYLES_POLLY = "polly styles";
    private static readonly string CONFIG_PROP_STYLES_SARAH = "sarah styles";
    private static readonly string CONFIG_PROP_STYLES_ZOEY = "zoey styles";

    private static readonly string CONFIG_PROP_UNLOCK_ALL = "unlock all";

    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");

    private void Awake()
    {
        Ids.Init();

        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_ABIA, true, "If all Abia's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_ASHLEY, true, "If all Ashley's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_BROOKE, true, "If all Brooke's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_CANDACE, true, "If all Candace's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_JESSIE, true, "If all Jessie's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_JEWN, true, "If all Jewn's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_KYU, true, "If all Kyu's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_LAILANI, true, "If all Lailani's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_LOLA, true, "If all Lola's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_MOXIE, true, "If all Moxie's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_NORA, true, "If all Nora's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_POLLY, true, "If all Polly's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_SARAH, true, "If all Sarah's styles should be added.");
        this.Config.Bind(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_ZOEY, true, "If all Zoey's styles should be added.");

        this.Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, CONFIG_PROP_UNLOCK_ALL, true, "If all expanded wardrobe outfits and hairstyles should be automatically unlocked.");

        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_ABIA, Styles.AddAbiaStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_ASHLEY, Styles.AddAshleyStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_BROOKE, Styles.AddBrookeStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_CANDACE, Styles.AddCandaceStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_JESSIE, Styles.AddJessieStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_JEWN, Styles.AddJewnStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_KYU, Styles.AddKyuStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_LAILANI, Styles.AddLailaniStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_LILLIAN, Styles.AddLillianStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_LOLA, Styles.AddLolaStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_MOXIE, Styles.AddMoxieStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_NORA, Styles.AddNoraStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_POLLY, Styles.AddPollyStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_SARAH, Styles.AddSarahStyles);
        IfBoolConfig(CONFIG_CAT_STYLES, CONFIG_PROP_STYLES_ZOEY, Styles.AddZoeyStyles);

        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
                out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(Path.Combine(IMAGES_DIR, "CreditsLogo.png"), [
                (
                        Path.Combine(IMAGES_DIR, "ScallyCapFan_credits.png"),
                        Path.Combine(IMAGES_DIR, "ScallyCapFan_credits_over.png"),
                        "https://www.reddit.com/user/scallycapfan/"
                    ),
                    (
                        Path.Combine(IMAGES_DIR, "onesuchkeeper_credits.png"),
                        Path.Combine(IMAGES_DIR, "onesuchkeeper_credits_over.png"),
                        "https://www.youtube.com/@onesuchkeeper8389"
                    )
            ]);
        }

        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;
    }

    private void IfBoolConfig(string configSection, string configKey, Action action)
    {
        if (this.Config.TryGetEntry(configSection, configKey, out ConfigEntry<bool> configEntry)
            && configEntry.Value)
        {
            action();
        }
    }

    //unlock all expanded wardrobe styles
    private void On_PreLoadPlayerFile(PlayerFile file)
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
