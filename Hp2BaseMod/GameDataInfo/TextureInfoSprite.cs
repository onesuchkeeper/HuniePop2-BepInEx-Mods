using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo;

/// <summary>
/// Texture info taken from an internal sprite asset.
/// </summary>
public class TextureInfoSprite : ITextureInfo
{
    private IGameDefinitionInfo<Sprite> _spriteData;
    private IEnumerable<ITextureRenderStep> _renderSteps;
    private bool _renderSprite;
    private Texture2D _texture;
    private bool _forceTight;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="spriteName">The name of the internal sprite asset.</param>
    /// <param name="renderSprite">If the sprite should be used to render a new texture using its mesh. Otherwise the sprite's texture is used.</param>
    /// <param name="renderSteps">Steps to apply to the texture.</param>
    public TextureInfoSprite(IGameDefinitionInfo<Sprite> spriteData, bool forceTight = false, bool renderSprite = true, IEnumerable<ITextureRenderStep> renderSteps = null)
    {
        _spriteData = spriteData ?? throw new ArgumentNullException(nameof(spriteData));
        _renderSprite = renderSprite;
        _renderSteps = renderSteps;
        _forceTight = forceTight;
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
                            ModInterface.Log.LogInfo($"Unsupported {nameof(Sprite)} {nameof(sprite.packingMode)} {sprite.packingMode}");
                            break;
                    }
                }

                _texture.Apply();
            }
            else
            {
                _texture = sprite.texture;
            }

            _renderSteps?.ForEach(x => x.Apply(ref _texture));
        }

        return _texture;
    }

    private void RenderRect(Texture2D texture, Sprite sprite)
    {
        texture.SetPixels(Enumerable.Repeat(Color.clear, texture.width * texture.height).ToArray());

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
            ModInterface.Log.LogWarning("Unable to render a tight-mesh sprite with a non-readable texture");
            return;
        }

        for (int i = 0; i < sprite.triangles.Length; i += 3)
        {
            var a = sprite.triangles[i];
            var b = sprite.triangles[i + 1];
            var c = sprite.triangles[i + 2];

            var vertA = sprite.vertices[a] * sprite.pixelsPerUnit;
            var vertB = sprite.vertices[b] * sprite.pixelsPerUnit;
            var vertC = sprite.vertices[c] * sprite.pixelsPerUnit;

            var textureSize = new Vector2(sprite.texture.width, sprite.texture.height);
            var uvA = sprite.uv[a] * textureSize;
            var uvB = sprite.uv[b] * textureSize;
            var uvC = sprite.uv[c] * textureSize;

            var minX = Mathf.FloorToInt(Mathf.Min(vertA.x, vertB.x, vertC.x));
            var maxX = Mathf.CeilToInt(Mathf.Max(vertA.x, vertB.x, vertC.x));
            var minY = Mathf.FloorToInt(Mathf.Min(vertA.y, vertB.y, vertC.y));
            var maxY = Mathf.CeilToInt(Mathf.Max(vertA.y, vertB.y, vertC.y));

            //Barycentric calc
            Vector2 v0 = vertB - vertA;
            Vector2 v1 = vertC - vertA;

            float d00 = Vector2.Dot(v0, v0);
            float d01 = Vector2.Dot(v0, v1);
            float d11 = Vector2.Dot(v1, v1);

            float denom = d00 * d11 - d01 * d01;
            if (Mathf.Abs(denom) < 1e-5f)
            {
                //bad triangle
                continue;
            }

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2 v2 = new Vector2(x, y) - vertA;
                    float d20 = Vector2.Dot(v2, v0);
                    float d21 = Vector2.Dot(v2, v1);

                    float weightB = (d11 * d20 - d01 * d21) / denom;
                    if (weightB < 0) { continue; }

                    float weightC = (d00 * d21 - d01 * d20) / denom;
                    if (weightC < 0) { continue; }

                    float weightA = 1.0f - weightB - weightC;
                    if (weightA < 0) { continue; }

                    var finalUv = (uvA * weightA) + (uvB * weightB) + (uvC * weightC);
                    texture.SetPixel(x, y, sprite.texture.GetPixel((int)finalUv.x, (int)finalUv.y));
                }
            }
        }
    }

    public void RequestInternals(AssetProvider assetProvider)
    {
        _spriteData.RequestInternals(assetProvider);
    }
}
