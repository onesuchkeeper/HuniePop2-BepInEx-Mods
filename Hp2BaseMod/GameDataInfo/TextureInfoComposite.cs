using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using UnityEngine;

namespace Hp2BaseMod;

public class TextureInfoComposite : ITextureInfo
{
    private IEnumerable<(ITextureInfo texture, Vector2Int offset)> _textures;
    private Vector2Int _size;
    private Texture2D _texture;

    public TextureInfoComposite(Vector2Int size, IEnumerable<(ITextureInfo texture, Vector2Int offset)> textures)
    {
        _size = size;
        _textures = textures ?? throw new ArgumentNullException(nameof(textures));
    }

    public Texture2D GetTexture()
    {
        if (_texture != null)
        {
            return _texture;
        }

        _texture = new Texture2D(_size.x, _size.y);
        var clear = new Color(1f, 1f, 1f, 0f);
        Color[] outputPixels = new Color[_size.x * _size.y];
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

                for (int x = 0; x < texWidth; x++)
                {
                    int outputX = offset.x + x;
                    if (outputX < 0 || outputX >= _size.x) { continue; }

                    int outputIndex = outputY * _size.x + outputX;
                    Color dstColor = outputPixels[outputIndex];
                    Color srcColor = sourcePixels[y * texWidth + x];

                    if (dstColor.a == 0f)
                    {
                        outputPixels[outputIndex] = srcColor;
                    }
                    else
                    {
                        // Alpha blend srcColor over dstColor:
                        float srcAlpha = srcColor.a;
                        Color blended = srcColor * srcAlpha + dstColor * (1f - srcAlpha);
                        blended.a = srcAlpha + dstColor.a * (1f - srcAlpha);

                        outputPixels[outputIndex] = blended;
                    }
                }
            }
        }

        _texture.SetPixels(outputPixels);
        _texture.Apply();
        return _texture;
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        _textures.ForEach(x => x.texture.RequestInternals(assetProvider));
    }
}