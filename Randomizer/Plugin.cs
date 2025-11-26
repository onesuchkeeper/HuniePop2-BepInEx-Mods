using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Hp2BaseMod;

namespace Hp2Randomizer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.SingleDate", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : Hp2BaseModPlugin
{
    [ConfigProperty(-1, "Randomizer seed. Set to -1 for a new random seed.")]
    public static int Seed
    {
        get => _instance.GetConfigProperty<int>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If character names will be randomized.")]
    public static bool RandomizeNames
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If character baggages will be randomized.")]
    public static bool RandomizeBaggage
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If character pairings will be randomized.")]
    public static bool RandomizePairs
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If character favorite and least favorite affection will be randomized.")]
    public static bool RandomizeAffection
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If Kyu should be included in the randomized characters.")]
    public static bool IncludeKyu
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If the Nymphojinn should be included in the randomized characters.")]
    public static bool IncludeNymphojinn
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If special characters should always be swapped with a normal character.")]
    public static bool ForceSpecialNormalSwap
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(true, "If special characters should keep their wings when swapped.")]
    public static bool SwappedSpecialKeepWings
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    [ConfigProperty(false, "Disables the randomizer entirely.")]
    public static bool Disable
    {
        get => _instance.GetConfigProperty<bool>();
        set => _instance.SetConfigProperty(value);
    }

    private Dictionary<RelativeId, Action<GirlDefinition, GirlDefinition>> _swapHandlers = new();
    private static Plugin _instance;
    public Plugin() : base(MyPluginInfo.PLUGIN_GUID) { }

    private void Awake()
    {
        _instance = this;

        ModInterface.AddCommand(new SetSeedCommand());

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
