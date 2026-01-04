using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Hp2BaseMod;

namespace Hp2Randomizer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("OSK.BepInEx.SingleDate", BepInDependency.DependencyFlags.SoftDependency)]
public partial class Plugin : Hp2BaseModPlugin
{
    private const string GENERAL_CONFIG_CAT = "general";
    private static readonly string TWEAKS_GUID = "OSK.BepInEx.Hp2BaseModTweaks";
    public static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    public static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");

    public static ConfigEntry<int> Seed => _seed;
    private static ConfigEntry<int> _seed;

    public static ConfigEntry<bool> RandomizeNames => _randomizeNames;
    private static ConfigEntry<bool> _randomizeNames;

    public static ConfigEntry<bool> RandomizeBaggage => _randomizeBaggage;
    private static ConfigEntry<bool> _randomizeBaggage;

    public static ConfigEntry<bool> RandomizePairs => _randomizePairs;
    private static ConfigEntry<bool> _randomizePairs;

    public static ConfigEntry<bool> RandomizeAffection => _randomizeAffection;
    private static ConfigEntry<bool> _randomizeAffection;

    public static ConfigEntry<bool> IncludeKyu => _includeKyu;
    private static ConfigEntry<bool> _includeKyu;

    public static ConfigEntry<bool> IncludeNymphojinn => _includeNymphojinn;
    private static ConfigEntry<bool> _includeNymphojinn;

    public static ConfigEntry<bool> ForceNormalSpecialSwap => _forceNormalSpecialSwap;
    private static ConfigEntry<bool> _forceNormalSpecialSwap;

    public static ConfigEntry<bool> SwappedSpecialKeepWings => _swappedSpecialKeepWings;
    private static ConfigEntry<bool> _swappedSpecialKeepWings;

    public static ConfigEntry<bool> Disable => _disable;
    private static ConfigEntry<bool> _disable;

    private Dictionary<RelativeId, Action<GirlDefinition, GirlDefinition>> _swapHandlers = new();
    private static Plugin _instance;
    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    protected override void Awake()
    {
        _instance = this;
        base.Awake();

        if (ModInterface.TryGetInterModValue(TWEAKS_GUID, "AddModCredit",
                out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(Path.Combine(IMAGES_DIR, "CreditsLogo.png"), [
                (
                    Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev.png"),
                    Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev_over.png"),
                    "https://linktr.ee/onesuchkeeper"
                )
            ]);
        }

        _seed = Config.Bind(GENERAL_CONFIG_CAT, nameof(Seed), -1, "Randomizer seed. Set to -1 for a new random seed.");
        _randomizeNames = Config.Bind(GENERAL_CONFIG_CAT, nameof(RandomizeNames), true, "If character names will be randomized.");
        _randomizeBaggage = Config.Bind(GENERAL_CONFIG_CAT, nameof(RandomizeBaggage), true, "If character baggages will be randomized.");
        _randomizePairs = Config.Bind(GENERAL_CONFIG_CAT, nameof(RandomizePairs), true, "If character pairings will be randomized.");
        _randomizeAffection = Config.Bind(GENERAL_CONFIG_CAT, nameof(RandomizeAffection), true, "If character favorite and least favorite affection will be randomized.");
        _includeKyu = Config.Bind(GENERAL_CONFIG_CAT, nameof(IncludeKyu), true, "If Kyu should be included in the randomized characters.");
        _includeNymphojinn = Config.Bind(GENERAL_CONFIG_CAT, nameof(IncludeKyu), true, "If the Nymphojinn should be included in the randomized characters.");
        _forceNormalSpecialSwap = Config.Bind(GENERAL_CONFIG_CAT, nameof(ForceNormalSpecialSwap), true, "If special characters should always be swapped with a normal character.");
        _swappedSpecialKeepWings = Config.Bind(GENERAL_CONFIG_CAT, nameof(SwappedSpecialKeepWings), true, "If special characters should keep their wings when swapped.");
        _disable = Config.Bind(GENERAL_CONFIG_CAT, nameof(Disable), false, "Disables the randomizer entirely.");

        ModInterface.AddCommand(new SetSeedCommand());


        if (Plugin.IncludeKyu.Value)
        {
            _swapHandlers.Add(Girls.Kyu, (a, b) => RandomizeUtil.SwapKyu(a, b, SwappedSpecialKeepWings.Value));
        }

        if (Plugin.IncludeNymphojinn.Value)
        {
            _swapHandlers.Add(Girls.Moxie, (a, b) => RandomizeUtil.SwapNymphojinn(a, b, SwappedSpecialKeepWings.Value));
            _swapHandlers.Add(Girls.Jewn, (a, b) => RandomizeUtil.SwapNymphojinn(a, b, SwappedSpecialKeepWings.Value));
        }

        ModInterface.Events.PostDataMods += () => RandomizeUtil.Randomize(_swapHandlers.Select(x => (x.Key, x.Value)));
        ModInterface.Events.PreLoadPlayerFile += RandomizeUtil.CleanGirlStyles;
    }

    /// <summary>
    /// Sets the swap handler for a particular Special Girl.
    /// Handler accepts the the special girl, then the girl she swaps with
    /// and swaps their properties
    /// </summary>
    [InteropMethod]
    public static void SetSpecialCharacterSwapHandler(RelativeId specialGirlId, Action<GirlDefinition, GirlDefinition> swapHandler)
        => _instance._swapHandlers[specialGirlId] = swapHandler;
}
