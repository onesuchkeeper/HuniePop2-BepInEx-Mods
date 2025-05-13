// Hp2BaseMod 2021, By OneSuchKeeper

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
        public RectInt? FinalTexturePadding;

        /// <summary>
        /// Mat to use when rendering the texture
        /// </summary>
        public Material TextureRenderMat;

        /// <summary>
        /// Rect to trim the initial texture to before
        /// applying other modifications
        /// </summary>
        public RectInt? TrimRect;

        /// <summary>
        /// Position this sprite takes in its atlas texture
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
        public virtual void SetData(ref Sprite def, GameDefinitionProvider _, AssetProvider assetProvider, InsertStyle insertStyle)
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
                def = GetSprite();
            }
        }

        public virtual Sprite GetSprite()
        {
            Sprite def = null;
            Texture2D texture2D = null;
            var modifyTexture = TextureScale.HasValue || FinalTexturePadding.HasValue || TextureRenderMat != null || TrimRect.HasValue;
            var cacheExists = !CachePath.IsNullOrWhiteSpace() && File.Exists(CachePath);

            if (cacheExists)
            {
                if (!ModInterface.Assets.ExternalTextures.TryGetValue(Path, out texture2D))
                {
                    texture2D = TextureUtility.LoadFromPath(CachePath);
                    ModInterface.Assets.AddExternalTexture(Path, texture2D);
                }
                modifyTexture = false;
            }
            else if (IsExternal)
            {
                if (File.Exists(Path))
                {
                    if (!ModInterface.Assets.ExternalTextures.TryGetValue(Path, out texture2D))
                    {
                        texture2D = TextureUtility.LoadFromPath(Path);
                        texture2D.filterMode = FilterMode.Bilinear;
                        ModInterface.Assets.AddExternalTexture(Path, texture2D);
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
                def = ModInterface.Assets.GetInternalAsset<Sprite>(Path);
                texture2D = def.texture;
            }

            if (modifyTexture)
            {
                int initWidth;
                int initHeight;

                if (TrimRect.HasValue)
                {
                    initHeight = TrimRect.Value.height;
                    initWidth = TrimRect.Value.width;
                }
                else
                {
                    initWidth = texture2D.width;
                    initHeight = texture2D.height;
                }

                TextureScale = TextureScale ?? new Vector2(1, 1);
                FinalTexturePadding = FinalTexturePadding ?? new RectInt();
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

                    var x = Mathf.RoundToInt(def.textureRect.x);
                    var y = Mathf.RoundToInt(def.textureRect.y);

                    //and trim the texture at the same time while we're at it
                    if (TrimRect.HasValue)
                    {
                        texture2D = new Texture2D(TrimRect.Value.width, TrimRect.Value.height);
                        Graphics.CopyTexture(result, 0, 0,
                            x + TrimRect.Value.x, y + TrimRect.Value.y,
                            TrimRect.Value.width, TrimRect.Value.height,
                            texture2D, 0, 0, 0, 0);
                    }
                    else
                    {
                        var width = Mathf.RoundToInt(def.textureRect.width);
                        var height = Mathf.RoundToInt(def.textureRect.height);

                        texture2D = new Texture2D(width, height);
                        Graphics.CopyTexture(result, 0, 0, x, y, width, height, texture2D, 0, 0, 0, 0);
                    }

                    texture2D.Apply();
                }
                else if (TrimRect.HasValue)
                {
                    var newTexture = new Texture2D(TrimRect.Value.width, TrimRect.Value.height);

                    Graphics.CopyTexture(texture2D, 0, 0,
                        TrimRect.Value.x, TrimRect.Value.y,
                        TrimRect.Value.width, TrimRect.Value.height,
                        newTexture, 0, 0, 0, 0);
                    newTexture.Apply();

                    texture2D = newTexture;
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

                result = new Texture2D(targetX + FinalTexturePadding.Value.width, targetY + FinalTexturePadding.Value.height);

                for (var i = 0; i < result.width; i++)
                {
                    for (var j = 0; j < result.height; j++)
                    {
                        result.SetPixel(i, j, Color.clear);
                    }
                }

                result.ReadPixels(new Rect(0, 0, targetX, targetY), FinalTexturePadding.Value.x, FinalTexturePadding.Value.y);
                result.Apply();
                texture2D = result;
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            if (!cacheExists && !CachePath.IsNullOrWhiteSpace())
            {
                File.WriteAllBytes(CachePath, texture2D.EncodeToPNG());
            }

            //if we modified the texture or have an atlasRect, make a new sprite
            if (modifyTexture || IsExternal || AtlasRect != null)
            {
                def = Sprite.Create(texture2D, AtlasRect.HasValue
                    ? AtlasRect.Value
                    : new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
            }

            return def;
        }

        /// <inheritdoc/>
        public virtual void RequestInternals(AssetProvider assetProvider)
        {
            if (!IsExternal)
            {
                assetProvider.RequestInternal(typeof(Sprite), Path);
            }
        }
    }
}
