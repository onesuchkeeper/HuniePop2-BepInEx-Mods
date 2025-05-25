using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

/// <summary>
/// Applies an outline to the texture. Make sure texture has enough padding to accommodate it.<br/>
/// <br/>
/// The outline shader from TextMeshPro doesn't do exactly what I want, and I
/// don't feel like learning ShaderLab and making an asset bundle, so this acts as a "Gpu Shader" to add a cellphone-outline
/// to sprites, which I'm fine with since we only call it the one time on initial install
/// </summary>
public class TextureRsCellphoneOutline : ITextureRenderStep
{
    private static readonly Color _cellphoneBlue = new Color(0.396f, 0.988f, 1f, 1f);
    private static readonly Color _cellphoneBlack = new Color(0f, 0.125f, 0.255f, 1f);

    private float _blueRad;
    private float _blackRad;
    private float _outlineAlpha;

    public TextureRsCellphoneOutline(float blueRad = 3f,
        float blackRad = 5f,
        float outlineAlpha = 0.64f)
    {
        _blueRad = blueRad;
        _blackRad = blackRad;
        _outlineAlpha = outlineAlpha;
    }

    public void Apply(ref Texture2D target)
    {
        //precalculate pixels for outline
        var blueOffsets = new List<Vector2Int>();
        var blackOffsets = new List<Vector2Int>();

        var largestRad = Mathf.Max(_blueRad, _blackRad);

        for (var i = -largestRad; i <= largestRad; i++)
        {
            for (var j = -largestRad; j <= largestRad; j++)
            {
                var dist = Mathf.Sqrt((i * i) + (j * j));

                if (_blueRad >= dist)
                {
                    blueOffsets.Add(new Vector2Int((int)i, (int)j));
                }

                if (_blackRad >= dist)
                {
                    blackOffsets.Add(new Vector2Int((int)i, (int)j));
                }
            }
        }

        //init the final texture
        var texture = new Texture2D(target.width, target.height);
        texture.SetPixels(Enumerable.Repeat(Color.clear, texture.width * texture.height).ToArray());

        //render
        for (var i = 0; i < texture.width; i++)
        {
            for (var j = 0; j < texture.height; j++)
            {
                var sourceColor = target.GetPixel(i, j);

                var blueAlpha = sourceColor.a;
                var blackAlpha = 0f;

                foreach (var offset in blueOffsets)
                {
                    var x = i + offset.x;
                    var y = j + offset.y;

                    if (x > 0
                        && x < target.width
                        && y > 0
                        && y < target.height)
                    {
                        blueAlpha = Mathf.Max(blueAlpha, target.GetPixel(x, y).a);
                    }
                }

                foreach (var offset in blackOffsets)
                {
                    var x = i + offset.x;
                    var y = j + offset.y;

                    if (x > 0
                        && x < target.width
                        && y > 0
                        && y < target.height)
                    {
                        blackAlpha = Mathf.Max(blackAlpha, target.GetPixel(x, y).a);
                    }
                }

                var outlineColor = AlphaMix(new Color(_cellphoneBlack.r, _cellphoneBlack.g, _cellphoneBlack.b, blackAlpha),
                    new Color(_cellphoneBlue.r, _cellphoneBlue.g, _cellphoneBlue.b, blueAlpha));

                outlineColor.a *= _outlineAlpha;

                texture.SetPixel(i, j, AlphaMix(outlineColor, sourceColor));
            }
        }

        UnityEngine.Object.Destroy(target);
        target = texture;
        target.Apply();
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        //noop
    }

    private Color AlphaMix(Color back, Color front)
    {
        var result = new Color();
        result.a = 1 - (1 - front.a) * (1 - back.a);

        if (result.a < 1.0e-6)
        {
            return result;
        }

        result.r = front.r * front.a / result.a + back.r * back.a * (1 - front.a) / result.a;
        result.g = front.g * front.a / result.a + back.g * back.a * (1 - front.a) / result.a;
        result.b = front.b * front.a / result.a + back.b * back.a * (1 - front.a) / result.a;

        return result;
    }
}
