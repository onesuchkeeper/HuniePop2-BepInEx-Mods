using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;

namespace HuniePopUltimate;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private static readonly string _dataDir = "HuniePop_Data";
    private static readonly string _assemblyDir = Path.Combine(_dataDir, "Managed");

    public static string RootDir => _dir;
    private static string _dir;

    public static int ModId => _modId;
    private static int _modId;

    public static int PhotoModCount = 0;

    private void Awake()
    {
        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);
        _dir = Path.GetDirectoryName(this.Info.Location);

        this.Config.Bind("General", "HuniePopDir", Path.Combine(Paths.PluginPath, "..", "..", "..", "HuniePop"), "Path to the Huniepop install directory.");

        if (!(this.Config.TryGetEntry<string>("General", "HuniePopDir", out var hpDirConfig)
                && !string.IsNullOrWhiteSpace(hpDirConfig.Value)
                && Directory.Exists(hpDirConfig.Value)))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate configuration does not contain an existing HuniePop directory. Please check \"HuniePop 2 - Double Date\\BepInEx\\config\\{MyPluginInfo.PLUGIN_GUID}.cfg\" and make sure the directory is correct.");
            return;
        }

        var dataDir = Path.Combine(hpDirConfig.Value, _dataDir);
        if (!Directory.Exists(dataDir))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate failed to find HuniePop data directory");
            return;
        }

        var assemblyDir = Path.Combine(hpDirConfig.Value, _assemblyDir);
        if (!Directory.Exists(assemblyDir))
        {
            ModInterface.Log.LogWarning("HuniePop Ultimate failed to find HuniePop assembly directory");
            return;
        }

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();

        ModInterface.Log.LogInfo("Loading HuniePop assembly (this may take a bit)");

        using (var hpExtraction = new HpExtraction(hpDirConfig.Value))
        {
            ModInterface.Log.LogInfo("HuniePop assembly loaded successfully, beginning import:");
            ModInterface.Log.IncreaseIndent();
            hpExtraction.Extract();
            ModInterface.Log.DecreaseIndent();
        }

        ModInterface.Events.RequestUnlockedPhotos += On_RequestUnlockedPhotos;
    }

    private void On_RequestUnlockedPhotos(RequestUnlockedPhotosEventArgs args)
    {
        args.UnlockedPhotos ??= new List<PhotoDefinition>();

        for (int i = 0; i < PhotoModCount; i++)
        {
            args.UnlockedPhotos.Add(ModInterface.GameData.GetPhoto(new RelativeId(ModId, i)));
        }
    }

    private bool TryLocalizeGirlId(int nativeId, out int localizedId)
    {
        localizedId = nativeId;
        return true;
        // switch (nativeId)
        // {
        //     case 1:
        //         localizedId = _catalog.Models.Character.Tiffany;
        //         return true;
        //     case 2:
        //         localizedId = _catalog.Models.Character.Aiko;
        //         return true;
        //     case 3:
        //         localizedId = _catalog.Models.Character.Kyanna;
        //         return true;
        //     case 4:
        //         localizedId = _catalog.Models.Character.Audrey;
        //         return true;
        //     case 5:
        //         localizedId = _catalog.Models.Character.Lola;
        //         return true;
        //     case 6:
        //         localizedId = _catalog.Models.Character.Nikki;
        //         return true;
        //     case 7:
        //         localizedId = _catalog.Models.Character.Jessie;
        //         return true;
        //     case 8:
        //         localizedId = _catalog.Models.Character.Beli;
        //         return true;
        //     case 9:
        //         localizedId = _catalog.Models.Character.Kyu;
        //         return true;
        //     case 10:
        //         localizedId = _catalog.Models.Character.Momo;
        //         return true;
        //     case 11:
        //         localizedId = _catalog.Models.Character.Celeste;
        //         return true;
        //     case 12:
        //         localizedId = _catalog.Models.Character.Venus;
        //         return true;
        // }
        // localizedId = 0;
        // return false;
    }
}