using System;
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
    private static string ImageDir = Path.Combine(_pluginDir, "images");
    private static readonly string TweaksGuid = "OSK.BepInEx.Hp2BaseModTweaks";

    private void Awake()
    {
        if (ModInterface.TryGetInterModValue(TweaksGuid, "AddModCredit",
                out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(Path.Combine(ImageDir, "CreditsLogo.png"), [
                (
                        Path.Combine(ImageDir, "silverwoodwork_credits.png"),
                        Path.Combine(ImageDir, "silverwoodwork_credits_over.png"),
                        "https://twitter.com/silverwoodwork"
                )
            ]);
        }

        if (ModInterface.TryGetInterModValue(TweaksGuid, "AddLogoPath",
                out Action<string> m_addLogoPath))
        {
            m_addLogoPath(Path.Combine(ImageDir, "logo.png"));
        }
    }
}