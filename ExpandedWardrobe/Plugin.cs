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
    private static readonly string ConfigStylesName = "styles";
    private static readonly string ConfigAbiaStylesName = "abia styles";
    private static readonly string ConfigAshleyStylesName = "ashley styles";
    private static readonly string ConfigBrookeStylesName = "brooke styles";
    private static readonly string ConfigCandaceStylesName = "candace styles";
    private static readonly string ConfigJessieStylesName = "jessie styles";
    private static readonly string ConfigJewnStylesName = "abia styles";
    private static readonly string ConfigKyuStylesName = "kyu styles";
    private static readonly string ConfigLailaniStylesName = "lailani styles";
    private static readonly string ConfigLillianStylesName = "lillian styles";
    private static readonly string ConfigLolaStylesName = "lola styles";
    private static readonly string ConfigMoxieStylesName = "moxie styles";
    private static readonly string ConfigNoraStylesName = "nora styles";
    private static readonly string ConfigPollyStylesName = "polly styles";
    private static readonly string ConfigSarahStylesName = "sarah styles";
    private static readonly string ConfigZoeyStylesName = "zoey styles";

    private static readonly string ConfigSettingsName = "settings";
    private static readonly string ConfigUnlockAllName = "unlock all";


    public static readonly string ModDir = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string ImageDir = Path.Combine(ModDir, "images");

    private void IfBoolConfig(string configSection, string configKey, Action action)
    {
        if (this.Config.TryGetEntry(configSection, configKey, out ConfigEntry<bool> configEntry)
            && configEntry.Value)
        {
            action();
        }
    }

    private void Awake()
    {
        Ids.Init();

        this.Config.Bind(ConfigStylesName, ConfigAbiaStylesName, true, "If all Abia's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigAshleyStylesName, true, "If all Ashley's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigBrookeStylesName, true, "If all Brooke's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigCandaceStylesName, true, "If all Candace's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigJessieStylesName, true, "If all Jessie's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigJewnStylesName, true, "If all Jewn's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigKyuStylesName, true, "If all Kyu's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigLailaniStylesName, true, "If all Lailani's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigLolaStylesName, true, "If all Lola's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigMoxieStylesName, true, "If all Moxie's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigNoraStylesName, true, "If all Nora's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigPollyStylesName, true, "If all Polly's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigSarahStylesName, true, "If all Sarah's styles should be added.");
        this.Config.Bind(ConfigStylesName, ConfigZoeyStylesName, true, "If all Zoey's styles should be added.");

        this.Config.Bind(ConfigSettingsName, ConfigUnlockAllName, true, "If all expanded wardrobe outfits and hairstyles should be automatically unlocked.");

        //HuniePopUltimate Compatibility
        if (Chainloader.PluginInfos.TryGetValue("OSK.BepInEx.HuniePopUltimate", out var hpUltimateInfo)
            && hpUltimateInfo.Instance.Config.TryGetEntry("general", "hp1Kyu", out ConfigEntry<bool> hp1KyuConfig))
        {
            ModInterface.Log.LogInfo("Kyu is hp1, don't add her outfits");
        }

        IfBoolConfig(ConfigStylesName, ConfigAbiaStylesName, Styles.AddAbiaStyles);
        IfBoolConfig(ConfigStylesName, ConfigAshleyStylesName, Styles.AddAshleyStyles);
        IfBoolConfig(ConfigStylesName, ConfigBrookeStylesName, Styles.AddBrookeStyles);
        IfBoolConfig(ConfigStylesName, ConfigCandaceStylesName, Styles.AddCandaceStyles);
        IfBoolConfig(ConfigStylesName, ConfigJessieStylesName, Styles.AddJessieStyles);
        IfBoolConfig(ConfigStylesName, ConfigJewnStylesName, Styles.AddJewnStyles);
        IfBoolConfig(ConfigStylesName, ConfigKyuStylesName, Styles.AddKyuStyles);
        IfBoolConfig(ConfigStylesName, ConfigLailaniStylesName, Styles.AddLailaniStyles);
        IfBoolConfig(ConfigStylesName, ConfigLillianStylesName, Styles.AddLillianStyles);
        IfBoolConfig(ConfigStylesName, ConfigLolaStylesName, Styles.AddLolaStyles);
        IfBoolConfig(ConfigStylesName, ConfigMoxieStylesName, Styles.AddMoxieStyles);
        IfBoolConfig(ConfigStylesName, ConfigNoraStylesName, Styles.AddNoraStyles);
        IfBoolConfig(ConfigStylesName, ConfigPollyStylesName, Styles.AddPollyStyles);
        IfBoolConfig(ConfigStylesName, ConfigSarahStylesName, Styles.AddSarahStyles);
        IfBoolConfig(ConfigStylesName, ConfigZoeyStylesName, Styles.AddZoeyStyles);

        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
                out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(Path.Combine(ImageDir, "CreditsLogo.png"), [
                (
                        Path.Combine(ImageDir, "ScallyCapFan_credits.png"),
                        Path.Combine(ImageDir, "ScallyCapFan_credits_over.png"),
                        "https://www.reddit.com/user/scallycapfan/"
                    ),
                    (
                        Path.Combine(ImageDir, "onesuchkeeper_credits.png"),
                        Path.Combine(ImageDir, "onesuchkeeper_credits_over.png"),
                        "https://www.youtube.com/@onesuchkeeper8389"
                    )
            ]);
        }

        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;
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
