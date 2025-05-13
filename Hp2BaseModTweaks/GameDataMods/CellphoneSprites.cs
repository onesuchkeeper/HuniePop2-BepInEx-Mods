using System;
using System.IO;
using Hp2BaseMod;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseModTweaks;

public static class CellphoneSprites
{
    private static readonly int _extraHeadPadding = 10;

    private static readonly Vector2 _cellphoneHeadSize = new Vector2(138, 126);
    private static readonly Vector2 _cellphoneMiniHeadSize = new Vector2(80, 80);
    private static readonly Vector2 _cellphonePortraitSize = new Vector2(214, 270);

    public static void AddUiCellphoneSprites(string name, RelativeId girlId, Vector2 headSize, Vector2 portraitSize)
    {
        var mod = new GirlDataMod(girlId, InsertStyle.replace);

        var texturePath = Path.Combine(Plugin.DacDir, "Heads", $"{name}.png");
        if (File.Exists(texturePath))
        {
            //offset to center in square
            var diff = Math.Abs(headSize.x - headSize.y);
            var padding = headSize.x > headSize.y
                ? new RectInt(0, (int)(diff / 2), 0, (int)(diff))
                : new RectInt((int)(diff / 2), 0, (int)(diff), 0);

            var scale = KeepRatioInBounds(headSize, _cellphoneHeadSize);
            mod.CellphoneHead = new SpriteInfoCellphoneOutline()
            {
                CachePath = Path.Combine(Plugin.ImagesDir, $"{name}_cellphoneHead.png"),
                IsExternal = true,
                Path = texturePath,
                TextureScale = scale,
                FinalTexturePadding = new RectInt((int)(padding.x * scale.x) + _extraHeadPadding,
                    (int)(padding.y * scale.y) + _extraHeadPadding,
                    (int)(padding.width * scale.x) + (2 * _extraHeadPadding),
                    (int)(padding.height * scale.y) + (2 * _extraHeadPadding)),
                BlueRad = 3f,
                BlackRad = 4.5f
            };

            scale = KeepRatioInBounds(headSize, _cellphoneMiniHeadSize);
            mod.CellphoneMiniHead = new SpriteInfoCellphoneOutline()
            {
                CachePath = Path.Combine(Plugin.ImagesDir, $"{name}_cellphoneHeadMini.png"),
                IsExternal = true,
                Path = texturePath,
                TextureScale = scale,
                FinalTexturePadding = new RectInt((int)(padding.x * scale.x) + _extraHeadPadding,
                    (int)(padding.y * scale.y) + _extraHeadPadding,
                    (int)(padding.width * scale.x) + (2 * _extraHeadPadding),
                    (int)(padding.height * scale.y) + (2 * _extraHeadPadding)),
                BlueRad = 3f,
                BlackRad = 0f,
                FinalOutlineAlpha = 1f
            };
        }

        texturePath = Path.Combine(Plugin.DacDir, "Sprites", $"{name}.png");
        if (File.Exists(texturePath))
        {
            var trimAmount = (int)(portraitSize.y * 0.1f);
            var finalHeight = (int)portraitSize.y - trimAmount;

            var scale = KeepRatioInBounds(new Vector2(portraitSize.x, finalHeight), _cellphonePortraitSize);

            mod.CellphonePortrait = new SpriteInfoCellphoneOutline()
            {
                IsExternal = true,
                CachePath = Path.Combine(Plugin.ImagesDir, $"{name}_cellphonePortrait.png"),
                Path = texturePath,
                TextureScale = scale,
                TrimRect = new RectInt(0, trimAmount, (int)portraitSize.x, finalHeight),
                FinalTexturePadding = new RectInt(8, 8, 16, 16),
                BlueRad = 3f,
                BlackRad = 4.5f
            };
        }

        ModInterface.AddDataMod(mod);
    }

    private static Vector2 KeepRatioInBounds(Vector2 size, Vector2 bounds)
    {
        var ratio = bounds / size;
        var min = Mathf.Min(ratio.x, ratio.y);
        return new Vector2(min, min);
    }
}