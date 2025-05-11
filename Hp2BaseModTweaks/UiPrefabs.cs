using System.IO;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseModTweaks;

public static class UiPrefabs
{
    public static Sprite CreditsBG => _creditsBG;
    private static Sprite _creditsBG;

    public static Sprite PairBG => _pairBG;
    private static Sprite _pairBG;

    internal static void Init()
    {
        _creditsBG = TextureUtility.SpriteFromPath(Path.Combine(Plugin.ImagesDir, "ui_app_credits_modded_background.png"));
        _pairBG = TextureUtility.SpriteFromPath(Path.Combine(Plugin.ImagesDir, "ui_app_pair_background.png"));
    }
}