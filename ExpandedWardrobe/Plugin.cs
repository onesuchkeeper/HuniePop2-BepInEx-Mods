using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Hp2BaseMod;

namespace ExpandedWardrobe;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private const string CONFIG_CAT_STYLES = "styles";

    public static ConfigEntry<bool> AbiaStyles => _abiaStyles;
    private static ConfigEntry<bool> _abiaStyles;

    public static ConfigEntry<bool> AshleyStyles => _ashleyStyles;
    private static ConfigEntry<bool> _ashleyStyles;

    public static ConfigEntry<bool> BrookeStyles => _brookeStyles;
    private static ConfigEntry<bool> _brookeStyles;

    public static ConfigEntry<bool> CandaceStyles => _candaceStyles;
    private static ConfigEntry<bool> _candaceStyles;

    public static ConfigEntry<bool> JessieStyles => _jessieStyles;
    private static ConfigEntry<bool> _jessieStyles;

    public static ConfigEntry<bool> JewnStyles => _jewnStyles;
    private static ConfigEntry<bool> _jewnStyles;

    public static ConfigEntry<bool> KyuStyles => _kyuStyles;
    private static ConfigEntry<bool> _kyuStyles;

    public static ConfigEntry<bool> LailaniStyles => _lailaniStyles;
    private static ConfigEntry<bool> _lailaniStyles;

    public static ConfigEntry<bool> LillianStyles => _lillianStyles;
    private static ConfigEntry<bool> _lillianStyles;

    public static ConfigEntry<bool> LolaStyles => _lolaStyles;
    private static ConfigEntry<bool> _lolaStyles;

    public static ConfigEntry<bool> MoxieStyles => _moxieStyles;
    private static ConfigEntry<bool> _moxieStyles;

    public static ConfigEntry<bool> NoraStyles => _noraStyles;
    private static ConfigEntry<bool> _noraStyles;

    public static ConfigEntry<bool> PollyStyles => _pollyStyles;
    private static ConfigEntry<bool> _pollyStyles;

    public static ConfigEntry<bool> SarahStyles => _sarahStyles;
    private static ConfigEntry<bool> _sarahStyles;

    public static ConfigEntry<bool> ZoeyStyles => _zoeyStyles;
    private static ConfigEntry<bool> _zoeyStyles;

    public static ConfigEntry<bool> UnlockAll => _unlockAll;
    private static ConfigEntry<bool> _unlockAll;

    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");

    private void Awake()
    {
        Ids.Init();

        _abiaStyles = Config.Bind(CONFIG_CAT_STYLES, "Abia Styles", true, "If all Abia's styles should be added.");
        _ashleyStyles = Config.Bind(CONFIG_CAT_STYLES, "Ashley Styles", true, "If all Ashley's styles should be added.");
        _brookeStyles = Config.Bind(CONFIG_CAT_STYLES, "Brooke Styles", true, "If all Brooke's styles should be added.");
        _candaceStyles = Config.Bind(CONFIG_CAT_STYLES, "Candace Styles", true, "If all Candace's styles should be added.");
        _jessieStyles = Config.Bind(CONFIG_CAT_STYLES, "Jessie Styles", true, "If all Jessie's styles should be added.");
        _jewnStyles = Config.Bind(CONFIG_CAT_STYLES, "Jewn Styles", true, "If all Jewn's styles should be added.");
        _kyuStyles = Config.Bind(CONFIG_CAT_STYLES, "Kyu Styles", true, "If all Kyu's styles should be added.");
        _lailaniStyles = Config.Bind(CONFIG_CAT_STYLES, "Lailani Styles", true, "If all Lailani's styles should be added.");
        _lillianStyles = Config.Bind(CONFIG_CAT_STYLES, "Lillian Styles", true, "If all Lillian's styles should be added.");
        _lolaStyles = Config.Bind(CONFIG_CAT_STYLES, "Lola Styles", true, "If all Lola's styles should be added.");
        _moxieStyles = Config.Bind(CONFIG_CAT_STYLES, "Moxie Styles", true, "If all Moxie's styles should be added.");
        _noraStyles = Config.Bind(CONFIG_CAT_STYLES, "Nora Styles", true, "If all Nora's styles should be added.");
        _pollyStyles = Config.Bind(CONFIG_CAT_STYLES, "Polly Styles", true, "If all Polly's styles should be added.");
        _sarahStyles = Config.Bind(CONFIG_CAT_STYLES, "Sarah Styles", true, "If all Sarah's styles should be added.");
        _zoeyStyles = Config.Bind(CONFIG_CAT_STYLES, "Zoey Styles", true, "If all Zoey's styles should be added.");

        _unlockAll = Config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, "Unlock All Styles", true, "If all expanded wardrobe outfits and hairstyles should be automatically unlocked.");

        if (_abiaStyles.Value) Styles.AddAbiaStyles();
        if (_ashleyStyles.Value) Styles.AddAshleyStyles();
        if (_brookeStyles.Value) Styles.AddBrookeStyles();
        if (_candaceStyles.Value) Styles.AddCandaceStyles();
        if (_jessieStyles.Value) Styles.AddJessieStyles();
        if (_jewnStyles.Value) Styles.AddJewnStyles();
        if (_kyuStyles.Value) Styles.AddKyuStyles();
        if (_lailaniStyles.Value) Styles.AddLailaniStyles();
        if (_lillianStyles.Value) Styles.AddLillianStyles();
        if (_lolaStyles.Value) Styles.AddLolaStyles();
        if (_moxieStyles.Value) Styles.AddMoxieStyles();
        if (_noraStyles.Value) Styles.AddNoraStyles();
        if (_pollyStyles.Value) Styles.AddPollyStyles();
        if (_sarahStyles.Value) Styles.AddSarahStyles();
        if (_zoeyStyles.Value) Styles.AddZoeyStyles();

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
                        Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_art.png"),
                        Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_art_over.png"),
                        "https://linktr.ee/onesuchkeeper"
                    )
            ]);
        }

        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;
    }

    // unlock all expanded wardrobe styles
    private void On_PreLoadPlayerFile(PlayerFile file)
    {
        if (_unlockAll.Value)
        {
            foreach (var fileGirl in file.girls)
            {
                var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id);
                var expansion = ExpandedGirlDefinition.Get(girlId);

                foreach (var outfitId in expansion.OutfitLookup.Ids.Where(x => x.SourceId == Ids.ModId))
                {
                    fileGirl.UnlockOutfit(expansion.OutfitLookup[outfitId]);
                }

                foreach (var hairstyleId in expansion.HairstyleLookup.Ids.Where(x => x.SourceId == Ids.ModId))
                {
                    fileGirl.UnlockHairstyle(expansion.HairstyleLookup[hairstyleId]);
                }
            }
        }
    }
}
