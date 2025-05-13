using System.Collections.Generic;
using System.IO;
using BepInEx;
using Hp2BaseMod;

namespace HiraganaLogo;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
{
    private static string _pluginDir = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    private static string _imagesDir = Path.Combine(_pluginDir, "images");

    private void Awake()
    {
        var tweaksId = ModInterface.GetSourceId("OSK.BepInEx.Hp2BaseModTweaks");

        var configs = ModInterface.GetInterModValue<Dictionary<string, (string ModImagePath, List<(string CreditButtonPath, string CreditButtonOverPath, string RedirectLink)> CreditEntries)>>(tweaksId, "ModCredits");

        configs[MyPluginInfo.PLUGIN_GUID] = (
            Path.Combine(_imagesDir, "CreditsLogo.png"),
            new List<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>(){
                    (
                        Path.Combine(_imagesDir, "silverwoodwork_credits.png"),
                        Path.Combine(_imagesDir, "silverwoodwork_credits_over.png"),
                        "https://twitter.com/silverwoodwork"
                    ),
            }
        );

        var logoPaths = ModInterface.GetInterModValue<List<string>>(tweaksId, "LogoPaths");
        logoPaths.Add(Path.Combine(_imagesDir, "logo.png"));
    }
}