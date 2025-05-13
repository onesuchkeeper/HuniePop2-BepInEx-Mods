using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseModTweaks
{
    /// <summary>
    /// Applies an outline to the final texture. Make sure texture has enough padding to accommodate it.<br/>
    /// <br/>
    /// The outline shader from TextMeshPro doesn't do exactly what I want, and I
    /// don't feel like learning ShaderLab and making an asset bundle, so this acts as a "Gpu Shader" to add a cellphone-outline
    /// to sprites, which I'm fine with since we only call it the one time on initial install
    /// </summary>
    public class SpriteInfoCellphoneOutline : SpriteInfoPath
    {
        public float BlueRad = 3f;
        public float BlackRad = 4.5f;
        public float FinalOutlineAlpha = 0.64f;

        private static readonly Color _cellphoneBlue = new Color(0.396f, 0.988f, 1f, 1f);
        private static readonly Color _cellphoneBlack = new Color(0f, 0.125f, 0.255f, 1f);

        public override Sprite GetSprite()
        {
            string cachePath = null;
            if (!CachePath.IsNullOrWhiteSpace())
            {
                if (File.Exists(CachePath))
                {
                    var texture = TextureUtility.LoadFromPath(CachePath);
                    return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }
                cachePath = base.CachePath;
                base.CachePath = null;
            }

            var def = base.GetSprite();
            var blueOffsets = new List<(int x, int y)>();
            var blackOffsets = new List<(int x, int y)>();

            var largest = Mathf.Max(BlueRad, BlackRad);

            for (var i = -largest; i < largest; i += 0.5f)
            {
                for (var j = -largest; j < largest; j += 0.5f)
                {
                    var dist = Mathf.Sqrt((i * i) + (j * j));

                    if (BlueRad >= dist)
                    {
                        blueOffsets.Add(((int)i, (int)j));
                    }

                    if (BlackRad >= dist)
                    {
                        blackOffsets.Add(((int)i, (int)j));
                    }
                }
            }

            var texture2D = new Texture2D(def.texture.width, def.texture.height);
            for (var i = 0; i < def.texture.width; i++)
            {
                for (var j = 0; j < def.texture.height; j++)
                {
                    texture2D.SetPixel(i, j, Color.clear);
                }
            }

            for (var i = 0; i < def.texture.width; i++)
            {
                for (var j = 0; j < def.texture.height; j++)
                {
                    var color = def.texture.GetPixel(i, j);

                    var blueAlpha = color.a;
                    var blackAlpha = 0f;

                    foreach (var offset in blueOffsets.Select(x => (i + x.x, j + x.y))
                        .Where(x => x.Item1 > 0
                            && x.Item1 < def.texture.width
                            && x.Item2 > 0
                            && x.Item2 < def.texture.height))
                    {
                        blueAlpha = Mathf.Max(blueAlpha, def.texture.GetPixel(offset.Item1, offset.Item2).a);
                    }

                    foreach (var offset in blackOffsets.Select(x => (i + x.x, j + x.y))
                        .Where(x => x.Item1 > 0
                            && x.Item1 < def.texture.width
                            && x.Item2 > 0
                            && x.Item2 < def.texture.height))
                    {
                        blackAlpha = Mathf.Max(blackAlpha, def.texture.GetPixel(offset.Item1, offset.Item2).a);
                    }

                    var outlineColor = AlphaMix(new Color(_cellphoneBlack.r, _cellphoneBlack.g, _cellphoneBlack.b, blackAlpha),
                        new Color(_cellphoneBlue.r, _cellphoneBlue.g, _cellphoneBlue.b, blueAlpha));

                    outlineColor.a *= FinalOutlineAlpha;

                    texture2D.SetPixel(i, j, AlphaMix(outlineColor, color));
                }
            }

            texture2D.Apply();

            if (cachePath != null)
            {
                File.WriteAllBytes(cachePath, texture2D.EncodeToPNG());
            }

            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
        }

        private Color AlphaMix(Color back, Color front)
        {
            var result = new Color();
            result.a = 1 - (1 - front.a) * (1 - back.a);
            if (result.a < 1.0e-6) return result;
            result.r = front.r * front.a / result.a + back.r * back.a * (1 - front.a) / result.a;
            result.g = front.g * front.a / result.a + back.g * back.a * (1 - front.a) / result.a;
            result.b = front.b * front.a / result.a + back.b * back.a * (1 - front.a) / result.a;

            return result;
        }
    }
}
