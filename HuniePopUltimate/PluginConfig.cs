using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Hp2BaseMod;

public class PluginConfig
{
    public ConfigEntry<bool> UnlockStyles => _unlockStyles;
    private ConfigEntry<bool> _unlockStyles;

    public ConfigEntry<bool> UnlockPhotos => _unlockPhotos;
    private ConfigEntry<bool> _unlockPhotos;

    public ConfigEntry<string> HuniePopDir => _huniePopDir;
    private ConfigEntry<string> _huniePopDir;

    public ConfigEntry<bool> AddDateLocations => _addDateLocations;
    private ConfigEntry<bool> _addDateLocations;

    public ConfigEntry<bool> AddSimLocations => _addSimLocations;
    private ConfigEntry<bool> _addSimLocations;

    public ConfigEntry<bool> AddCharacters => _addCharacters;
    private ConfigEntry<bool> _addCharacters;

    public ConfigEntry<bool> UseHp1LolaStats => _useHp1LolaStats;
    private ConfigEntry<bool> _useHp1LolaStats;

    public ConfigEntry<bool> UseHp1JessieStats => _useHp1JessieStats;
    private ConfigEntry<bool> _useHp1JessieStats;

    public ConfigEntry<bool> UseHp1LolaLines => _useHp1LolaLines;
    private ConfigEntry<bool> _useHp1LolaLines;

    public ConfigEntry<bool> UseHp1JessieLines => _useHp1JessieLines;
    private ConfigEntry<bool> _useHp1JessieLines;

    public PluginConfig(ConfigFile config)
    {
        _huniePopDir = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(HuniePopDir), Path.Combine(Paths.PluginPath, "..", "..", "..", "HuniePop"), "Path to the HuniePop install directory.");

        _addDateLocations = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(AddDateLocations), true, "If HuniePop date locations should be added.");
        _addSimLocations = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(AddSimLocations), true, "If HuniePop sim locations should be added.");
        _addCharacters = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(AddCharacters), true, "If HuniePop characters should be added.");

        _useHp1LolaStats = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseHp1LolaStats), true, "If HuniePop stats should be used for Lola.");
        _useHp1LolaLines = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseHp1LolaLines), true, "If HuniePop lines should be used for Lola.");

        _useHp1JessieStats = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseHp1JessieStats), true, "If HuniePop stats should be used for Jessie.");
        _useHp1JessieLines = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UseHp1JessieLines), true, "If HuniePop lines should be used for Jessie.");

        _unlockStyles = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UnlockStyles), false, "If all HuniePop outfit and hairstyles should auto-unlock.");
        _unlockPhotos = config.Bind(Hp2BaseModPlugin.CONFIG_GENERAL, nameof(UnlockPhotos), false, "If all HuniePop photos should auto-unlock.");
    }
}