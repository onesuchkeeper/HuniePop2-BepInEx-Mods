using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseModTweaks;

public static class UiPrefabs
{
    public static Sprite CreditsBG => _creditsBG;
    private static Sprite _creditsBG;

    public static Sprite PairBG => _pairBG;
    private static Sprite _pairBG;

    public static Sprite CensoredThumb => _censoredThumb;
    private static Sprite _censoredThumb;

    public static Sprite CensoredBig => _censoredBig;
    private static Sprite _censoredBig;

    public static Material BgBlur => _bgBlur;
    private static Material _bgBlur;

    internal static void Init()
    {
        _creditsBG = TextureUtility.SpriteFromPng(Path.Combine(Plugin.ImagesDir, "ui_app_credits_modded_background.png"));
        _pairBG = TextureUtility.SpriteFromPng(Path.Combine(Plugin.ImagesDir, "ui_app_pair_background.png"));
        _censoredThumb = TextureUtility.SpriteFromPng(Path.Combine(Plugin.ImagesDir, "ui_photo_album_slot_censored.png"));
        _censoredBig = TextureUtility.SpriteFromPng(Path.Combine(Plugin.ImagesDir, "cg_censored.png"));

        var bundlePath = Path.Combine(Plugin.RootDir, "hp2basemodtweaks_assetbundle");
        if (File.Exists(bundlePath))
        {
            AssetBundle bundle = null;

            using (var bundleFile = File.OpenRead(bundlePath))
            {
                bundle = AssetBundle.LoadFromStream(bundleFile);
            }

            if (bundle == null)
            {
                ModInterface.Log.LogWarning("Failed to load Hp2BaseModTweaks asset bundle");
            }
            else
            {
                _bgBlur = bundle.LoadAsset<Material>("BgBlur");
            }
        }
    }
}