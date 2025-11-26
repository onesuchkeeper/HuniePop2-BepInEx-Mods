using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod;

public class TextureInfoComposite : ITextureInfo
{
    private readonly IEnumerable<(ITextureInfo texture, Vector2Int offset)> _textures;
    private readonly Vector2Int _size;
    private readonly bool _readOnly;

    private Texture2D _texture;

    public TextureInfoComposite(Vector2Int size, bool readOnly, IEnumerable<(ITextureInfo texture, Vector2Int offset)> textures)
    {
        _size = size;
        _textures = textures ?? throw new ArgumentNullException(nameof(textures));
        _readOnly = readOnly;
    }

    public Texture2D GetTexture()
    {
        if (_texture != null)
        {
            return _texture;
        }

        _texture = new Texture2D(_size.x, _size.y);
        var clear = new Color(1f, 1f, 1f, 0f);
        var outputPixels = new Color[_size.x * _size.y];
        for (int i = 0; i < outputPixels.Length; i++)
        {
            outputPixels[i] = clear;
        }

        foreach (var (textureInfo, offset) in _textures)
        {
            var texture = textureInfo.GetTexture();

            Color[] sourcePixels = texture.GetPixels();
            int texWidth = texture.width;
            int texHeight = texture.height;

            for (int y = 0; y < texHeight; y++)
            {
                int outputY = offset.y + y;
                if (outputY < 0 || outputY >= _size.y) { continue; }

                var rowOffset = outputY * _size.x;
                var yShift = y * texWidth;
                for (int x = 0; x < texWidth; x++)
                {
                    int outputX = offset.x + x;
                    if (outputX < 0 || outputX >= _size.x) { continue; }

                    int outputIndex = rowOffset + outputX;
                    Color dstColor = outputPixels[outputIndex];
                    Color srcColor = sourcePixels[yShift + x];

                    if (dstColor.a == 0f)
                    {
                        outputPixels[outputIndex] = srcColor;
                    }
                    else
                    {
                        // Alpha blend srcColor over dstColor:
                        float srcAlpha = srcColor.a;
                        if (srcAlpha == 1f)
                        {
                            outputPixels[outputIndex] = srcColor;
                        }
                        else
                        {
                            outputPixels[outputIndex] = new Color(
                                srcColor.r * srcAlpha + dstColor.r * (1f - srcAlpha),
                                srcColor.g * srcAlpha + dstColor.g * (1f - srcAlpha),
                                srcColor.b * srcAlpha + dstColor.b * (1f - srcAlpha),
                                srcAlpha + dstColor.a * (1f - srcAlpha)
                            ); ;
                        }
                    }
                }
            }
        }

        _texture.SetPixels(outputPixels);
        _texture.Apply(updateMipmaps: false, makeNoLongerReadable: _readOnly);
        return _texture;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        _textures.ForEach(x => x.texture.RequestInternals(assetProvider));
    }
}