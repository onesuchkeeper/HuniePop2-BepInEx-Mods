// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.IO;
using BepInEx;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a sprite
    /// </summary>
    public class SpriteInfoPath : IGameDefinitionInfo<Sprite>
    {
        /// <summary>
        /// Path to the sprite
        /// </summary>
        public string Path;

        /// <summary>
        /// Path to save modified texture to as a png
        /// </summary>
        public string CachePath;

        /// <summary>
        /// If the path is to an file on disk. Is an internal resource otherwise
        /// </summary>
        public bool IsExternal;

        /// <summary>
        /// The scale to scale the texture to
        /// </summary>
        public Vector2? TextureScale;

        /// <summary>
        /// Padding of transparent pixels around the texture
        /// </summary>
        public RectInt? TexturePadding;

        /// <summary>
        /// Mat to use when rendering the texture
        /// </summary>
        public Material TextureRenderMat;

        /// <summary>
        /// Position this sprite takes in it's atlas texture
        /// </summary>
        public Rect? AtlasRect;

        /// <summary>
        /// If the sprite is flipped vertically on the atlas texture
        /// </summary>
        public bool AtlasFlipV;

        /// <summary>
        /// If the sprite is flipped horizontally on the atlas texture
        /// </summary>
        public bool AtlasFlipH;

        /// <summary>
        /// If the sprite is transposed on the atlas texture
        /// </summary>
        public bool AtlasTranspose;

        /// <summary>
        /// Constructor
        /// </summary>
        public SpriteInfoPath() { }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
        public SpriteInfoPath(Sprite def, AssetProvider assetProvider)
        {
            if (def == null) { return; }

            assetProvider.NameAndAddAsset(ref Path, def);
            IsExternal = false;
        }

        /// <inheritdoc/>
        public void SetData(ref Sprite def, GameDefinitionProvider _, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (Path == null)
            {
                if (insertStyle == InsertStyle.assignNull)
                {
                    def = null;
                }
            }
            else
            {
                Texture2D texture2D = null;
                var modifyTexture = TextureScale.HasValue || TexturePadding.HasValue || TextureRenderMat != null;
                var cacheExists = !CachePath.IsNullOrWhiteSpace() && File.Exists(CachePath);

                if (cacheExists)
                {
                    if (!assetProvider.ExternalTextures.TryGetValue(Path, out texture2D))
                    {
                        texture2D = TextureUtility.LoadFromPath(CachePath);
                        assetProvider.ExternalTextures[Path] = texture2D;
                    }
                    modifyTexture = false;
                }
                else if (IsExternal)
                {
                    if (File.Exists(Path))
                    {
                        if (!assetProvider.ExternalTextures.TryGetValue(Path, out texture2D))
                        {
                            texture2D = TextureUtility.LoadFromPath(Path);
                            texture2D.filterMode = FilterMode.Bilinear;
                        }
                    }
                    else
                    {
                        ModInterface.Log.LogInfo($"Could not find {Path} to load {nameof(Sprite)} from");
                        texture2D = new Texture2D(32, 32);
                    }
                }
                else
                {
                    def = (Sprite)assetProvider.GetAsset(Path);
                    texture2D = def.texture;
                }

                if (modifyTexture)
                {
                    var initWidth = texture2D.width;
                    var initHeight = texture2D.height;
                    TextureScale = TextureScale ?? new Vector2(1, 1);
                    TexturePadding = TexturePadding ?? new RectInt();
                    int targetX = 0;
                    int targetY = 0;
                    RenderTexture renderTexture = null;
                    Texture2D result = null;

                    //if working with an internal sprite, use a copy of the texture and preserve the original
                    if (!IsExternal)
                    {
                        renderTexture = RenderTexture.GetTemporary(def.texture.width, def.texture.height);

                        Graphics.Blit(texture2D, renderTexture);

                        result = new Texture2D(def.texture.width, def.texture.height);
                        result.ReadPixels(new Rect(0, 0, def.texture.width, def.texture.height), 0, 0);
                        result.Apply();

                        RenderTexture.ReleaseTemporary(renderTexture);

                        ModInterface.Log.LogInfo(def.rect.ToString());

                        int x = Mathf.RoundToInt(def.textureRect.x);
                        int y = Mathf.RoundToInt(def.textureRect.y);
                        int width = Mathf.RoundToInt(def.textureRect.width);
                        int height = Mathf.RoundToInt(def.textureRect.height);

                        texture2D = new Texture2D(width, height);
                        Graphics.CopyTexture(result, 0, 0, x, y, width, height, texture2D, 0, 0, 0, 0);
                        texture2D.Apply();
                    }

                    int preScaleRounds = 0;
                    while (TextureScale.Value.x < 0.5f || TextureScale.Value.y < 0.5f)
                    {
                        TextureScale = TextureScale * 2f;
                        preScaleRounds++;
                    }

                    for (var i = 0; i < preScaleRounds; i++)
                    {
                        targetX = (int)(initWidth * TextureScale.Value.x);
                        targetY = (int)(initHeight * TextureScale.Value.y);

                        renderTexture = RenderTexture.GetTemporary(targetX, targetY);

                        Graphics.Blit(texture2D, renderTexture);

                        result = new Texture2D(targetX, targetY);
                        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
                        result.Apply();
                        texture2D = result;
                        RenderTexture.ReleaseTemporary(renderTexture);

                        TextureScale = TextureScale * 0.5f;
                    }

                    targetX = (int)(initWidth * TextureScale.Value.x);
                    targetY = (int)(initHeight * TextureScale.Value.y);

                    renderTexture = RenderTexture.GetTemporary(targetX, targetY);

                    if (TextureRenderMat == null)
                    {
                        Graphics.Blit(texture2D, renderTexture);
                    }
                    else
                    {
                        Graphics.Blit(texture2D, renderTexture, TextureRenderMat);
                    }

                    result = new Texture2D(targetX + TexturePadding.Value.width, targetY + TexturePadding.Value.height);

                    for (var i = 0; i < result.width; i++)
                    {
                        for (var j = 0; j < result.height; j++)
                        {
                            result.SetPixel(i, j, Color.clear);
                        }
                    }

                    result.ReadPixels(new Rect(0, 0, targetX, targetY), TexturePadding.Value.x, TexturePadding.Value.y);
                    result.Apply();
                    texture2D = result;
                    RenderTexture.ReleaseTemporary(renderTexture);
                }
                else if (IsExternal)//keep ref to unmodified external textures in case they're used multiple times 
                {
                    assetProvider.ExternalTextures[Path] = texture2D;
                }

                if (!cacheExists && !CachePath.IsNullOrWhiteSpace())
                {
                    File.WriteAllBytes(CachePath, texture2D.EncodeToPNG());
                }

                //if we modified the texture or have an atlasRect, make a new sprite
                if (modifyTexture || IsExternal || AtlasRect != null)
                {
                    ModInterface.Log.LogInfo($"Making New Sprite");
                    def = Sprite.Create(texture2D, AtlasRect.HasValue
                        ? AtlasRect.Value
                        : new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalAudioRequests() => null;

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalSpriteRequests()
        {
            if (!IsExternal)
            {
                yield return Path;
            }
        }
    }
}