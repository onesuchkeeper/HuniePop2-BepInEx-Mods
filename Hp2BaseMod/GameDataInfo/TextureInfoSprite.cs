using System;
using System.Collections.Generic;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

/// <summary>
/// Texture info taken from an internal sprite asset.
/// </summary>
public class TextureInfoSprite : ITextureInfo
{
    private readonly IGameDefinitionInfo<Sprite> _spriteData;
    private readonly IEnumerable<ITextureRenderStep> _renderSteps;
    private readonly bool _renderSprite;
    private readonly bool _forceTight;
    private readonly bool _readOnly;

    private Texture2D _texture;

    public TextureInfoSprite(IGameDefinitionInfo<Sprite> spriteData, bool readOnly, bool forceTight = false, bool renderSprite = true, IEnumerable<ITextureRenderStep> renderSteps = null)
    {
        _spriteData = spriteData ?? throw new ArgumentNullException(nameof(spriteData));
        _renderSprite = renderSprite;
        _renderSteps = renderSteps;
        _forceTight = forceTight;
        _readOnly = readOnly;
    }

    public Texture2D GetTexture()
    {
        if (_texture == null)
        {
            Sprite sprite = null;
            _spriteData.SetData(ref sprite, ModInterface.GameData, ModInterface.Assets, InsertStyle.replace);

            if (_renderSprite)
            {
                _texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

                if (_forceTight)
                {
                    RenderTight(_texture, sprite);
                }
                else
                {
                    switch (sprite.packingMode)
                    {
                        case SpritePackingMode.Tight:
                            RenderTight(_texture, sprite);
                            break;
                        case SpritePackingMode.Rectangle:
                            RenderRect(_texture, sprite);
                            break;
                        default:
                            ModInterface.Log.Message($"Unsupported {nameof(Sprite)} {nameof(sprite.packingMode)} {sprite.packingMode}");
                            break;
                    }
                }

                // if there are render steps, don't make readOnly until after
                _texture.Apply(false, _renderSteps == null && _readOnly);
            }
            else
            {
                _texture = sprite.texture;
            }

            if (_renderSteps != null)
            {
                _renderSteps?.ForEach(x => x.Apply(ref _texture));
                _texture.Apply(false, _readOnly);
            }
        }

        return _texture;
    }

    private void RenderRect(Texture2D texture, Sprite sprite)
    {
        var textureSize = new Vector2(sprite.texture.width, sprite.texture.height);
        var scale = sprite.textureRect.size / textureSize;
        var offset = sprite.textureRect.position / textureSize;
        var diff = (sprite.rect.size - sprite.textureRect.size) / 2;

        var renderTexture = RenderTexture.GetTemporary((int)sprite.textureRect.width, (int)sprite.textureRect.height);

        Graphics.Blit(sprite.texture, renderTexture, scale, offset);
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), (int)diff.x, (int)diff.y);

        RenderTexture.ReleaseTemporary(renderTexture);
    }

    private void RenderTight(Texture2D texture, Sprite sprite)
    {
        if (!sprite.texture.isReadable)
        {
            ModInterface.Log.Warning("Unable to render a tight-mesh sprite with a non-readable texture");
            return;
        }

        int texWidth = texture.width;
        int texHeight = texture.height;
        var outputPixels = new Color[texWidth * texHeight];
        for (int i = 0; i < outputPixels.Length; i++)
        {
            outputPixels[i] = Color.clear;
        }

        var sourceTex = sprite.texture;
        var sourcePixels = sourceTex.GetPixels();
        int srcWidth = sourceTex.width;
        int srcHeight = sourceTex.height;
        var srcSize = new Vector2(srcWidth, srcHeight);

        var verts = sprite.vertices;
        var uvs = sprite.uv;
        var tris = sprite.triangles;
        float ppu = sprite.pixelsPerUnit;

        for (int i = 0; i < tris.Length; i += 3)
        {
            int a = tris[i];
            int b = tris[i + 1];
            int c = tris[i + 2];

            Vector2 vertA = verts[a] * ppu;
            Vector2 vertB = verts[b] * ppu;
            Vector2 vertC = verts[c] * ppu;

            Vector2 uvA = uvs[a] * srcSize;
            Vector2 uvB = uvs[b] * srcSize;
            Vector2 uvC = uvs[c] * srcSize;

            int minX = Mathf.FloorToInt(Mathf.Min(vertA.x, vertB.x, vertC.x));
            int maxX = Mathf.CeilToInt(Mathf.Max(vertA.x, vertB.x, vertC.x));
            int minY = Mathf.FloorToInt(Mathf.Min(vertA.y, vertB.y, vertC.y));
            int maxY = Mathf.CeilToInt(Mathf.Max(vertA.y, vertB.y, vertC.y));

            if (maxX < 0 || maxY < 0 || minX >= texWidth || minY >= texHeight)
            {
                continue;
            }

            // Clamp to valid region
            minX = Mathf.Max(minX, 0);
            minY = Mathf.Max(minY, 0);
            maxX = Mathf.Min(maxX, texWidth - 1);
            maxY = Mathf.Min(maxY, texHeight - 1);

            // Precompute barycentric basis
            Vector2 v0 = vertB - vertA;
            Vector2 v1 = vertC - vertA;
            float d00 = Vector2.Dot(v0, v0);
            float d01 = Vector2.Dot(v0, v1);
            float d11 = Vector2.Dot(v1, v1);
            float denom = d00 * d11 - d01 * d01;
            if (Mathf.Abs(denom) < 1e-5f)
            {
                continue; // degenerate triangle
            }

            for (int y = minY; y <= maxY; y++)
            {
                int rowOffset = y * texWidth;
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 v2 = new Vector2(x, y) - vertA;
                    float d20 = Vector2.Dot(v2, v0);
                    float d21 = Vector2.Dot(v2, v1);

                    float weightB = (d11 * d20 - d01 * d21) / denom;
                    float weightC = (d00 * d21 - d01 * d20) / denom;
                    float weightA = 1f - weightB - weightC;

                    if (weightA < 0f || weightB < 0f || weightC < 0f)
                    {
                        continue; // outside triangle
                    }

                    // Interpolate UVs
                    Vector2 finalUV = uvA * weightA + uvB * weightB + uvC * weightC;

                    int srcX = Mathf.Clamp((int)finalUV.x, 0, srcWidth - 1);
                    int srcY = Mathf.Clamp((int)finalUV.y, 0, srcHeight - 1);
                    Color srcColor = sourcePixels[srcY * srcWidth + srcX];

                    outputPixels[rowOffset + x] = srcColor;
                }
            }
        }

        texture.SetPixels(outputPixels);
    }


    public void RequestInternals(AssetProvider assetProvider)
    {
        _spriteData.RequestInternals(assetProvider);
    }
}
