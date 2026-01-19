using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod;

namespace AnniversaryTitle;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
[BepInDependency("OSK.BepInEx.Hp2BaseModTweaks", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    internal static readonly string ROOT_DIR = Path.Combine(Paths.PluginPath, MyPluginInfo.PLUGIN_NAME);
    internal static readonly string IMAGES_DIR = Path.Combine(ROOT_DIR, "images");
    internal static readonly string AUDIO_DIR = Path.Combine(ROOT_DIR, "audio");
    internal static bool InitialTitleAnimation = true;
    private void Awake()
    {
        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
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

        new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll();
    }
}
