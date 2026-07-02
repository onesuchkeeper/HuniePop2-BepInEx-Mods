using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.Utility;
using UnityEngine;

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
        if (ModInterface.TryGetInterModValue("OSK.BepInEx.Hp2BaseModTweaks", "AddModCredit",
            out Action<Sprite, IEnumerable<(Sprite creditButtonPath, Sprite creditButtonOverPath, string redirectLink)>> m_addModConfig))
        {
            m_addModConfig(TextureUtility.SpriteFromPng(Path.Combine(IMAGES_DIR, "CreditsLogo.png"), true), [
                (
                    TextureUtility.SpriteFromPng(Path.Combine(IMAGES_DIR, "silverwoodwork_credits_art.png"), true),
                    TextureUtility.SpriteFromPng(Path.Combine(IMAGES_DIR, "silverwoodwork_credits_art_over.png"), true),
                    "https://twitter.com/silverwoodwork"
                ),
                (
                    TextureUtility.SpriteFromPng(Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev.png"), true),
                    TextureUtility.SpriteFromPng(Path.Combine(IMAGES_DIR, "onesuchKeeper_credits_dev_over.png"), true),
                    "https://linktr.ee/onesuchkeeper"
                ),
            ]);
        }

        if (ModInterface.TryGetInterModValue(TWEAKS_GUID, "AddLogoSprite",
            out Action<Sprite> m_addLogoSprite))
        {
            m_addLogoSprite(Hp2BaseMod.Utility.TextureUtility.SpriteFromPng(Path.Combine(IMAGES_DIR, "logo.png"), true));
        }
    }
}
