using System.IO;
using BepInEx;
using HarmonyLib;

namespace AnniversaryTitle;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static readonly string RootDir = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    internal static readonly string ImagesDir = Path.Combine(RootDir, "images");
    internal static readonly string AudioDir = Path.Combine(RootDir, "audio");
    internal static bool InitialTitleAnimation = true;
    private void Awake()
    {
        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }
}
