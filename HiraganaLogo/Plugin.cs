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
    private static readonly string PLUGIN_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    private static readonly string IMAGES_DIR = Path.Combine(PLUGIN_DIR, "images");
    private static readonly string TWEAKS_GUID = "OSK.BepInEx.Hp2BaseModTweaks";

    private void Awake()
    {
        if (ModInterface.TryGetInterModValue(TWEAKS_GUID, "AddModCredit",
                out Action<string, IEnumerable<(string creditButtonPath, string creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(Path.Combine(IMAGES_DIR, "CreditsLogo.png"), [
                (
                        Path.Combine(IMAGES_DIR, "silverwoodwork_credits_art.png"),
                        Path.Combine(IMAGES_DIR, "silverwoodwork_credits_art_over.png"),
                        "https://twitter.com/silverwoodwork"
                ),
                (
                    Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev.png"),
                    Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev_over.png"),
                    "https://linktr.ee/onesuchkeeper"
                )
            ]);
        }

        if (ModInterface.TryGetInterModValue(TWEAKS_GUID, "AddLogoPath",
            out Action<string> m_addLogoPath))
        {
            m_addLogoPath(Path.Combine(IMAGES_DIR, "logo.png"));
        }
    }
}
