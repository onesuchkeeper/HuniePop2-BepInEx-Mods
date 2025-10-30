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

        var digitalArtCollectionDir = Plugin.Instance.DigitalArtCollectionDir;

        var texturePath = Path.Combine(digitalArtCollectionDir, "Heads", $"{name}.png");
        if (File.Exists(texturePath))
        {
            //pad to center in square
            var diff = (int)(Math.Abs(headSize.x - headSize.y) / 2f);
            var padding = headSize.x > headSize.y
                ? new Vector2(0, diff + _extraHeadPadding)
                : new Vector2(diff + _extraHeadPadding, 0);

            var cellphoneHeadInfo = new TextureInfoExternal(texturePath, false);

            var scale = KeepRatioInBounds(headSize, _cellphoneHeadSize);
            mod.CellphoneHead = new SpriteInfoTexture(new TextureInfoCache(
                Path.Combine(Plugin.ImagesDir, $"{name}_cellphoneHead.png"),
                new TextureInfoRender(cellphoneHeadInfo, false, [
                    new TextureRsScale(scale),
                    new TextureRsPad((int)(padding.x * scale.x) + 5, (int)(padding.y * scale.y) + 5),
                    new TextureRsCellphoneOutline()
                ])));

            scale = KeepRatioInBounds(headSize, _cellphoneMiniHeadSize);
            mod.CellphoneMiniHead = new SpriteInfoTexture(new TextureInfoCache(
                Path.Combine(Plugin.ImagesDir, $"{name}_cellphoneHeadMini.png"),
                new TextureInfoRender(cellphoneHeadInfo, false, [
                    new TextureRsScale(scale),
                    new TextureRsPad((int)(padding.x * scale.x) + 4, (int)(padding.y * scale.y) + 4),
                    new TextureRsCellphoneOutline(3f, 0f, 1f)
                ])));
        }

        texturePath = Path.Combine(digitalArtCollectionDir, "Sprites", $"{name}.png");
        if (File.Exists(texturePath))
        {
            var trimAmount = portraitSize.y * 0.1f;
            var scale = KeepRatioInBounds(new Vector2(portraitSize.x, portraitSize.y - trimAmount), _cellphonePortraitSize);
            trimAmount *= scale.y;

            mod.CellphonePortrait = new SpriteInfoTexture(new TextureInfoCache(
                Path.Combine(Plugin.ImagesDir, $"{name}_cellphonePortrait.png"),
                new TextureInfoExternal(texturePath, false, FilterMode.Bilinear, [
                    new TextureRsScale(scale),
                    new TextureRsPad(0, 0, 0, (int)-trimAmount),
                    new TextureRsPad(8),
                    new TextureRsCellphoneOutline()
                ])));
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