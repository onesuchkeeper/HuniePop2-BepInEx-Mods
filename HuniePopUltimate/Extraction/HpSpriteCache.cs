using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AssetStudio;
using AssetStudio.Extractor;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using UE = UnityEngine;

namespace HuniePopUltimate;

/// <summary>
/// Resolves sprite collections and builds <see cref="SpriteInfoTexture"/> /
/// <see cref="TextureInfoRasterized"/> instances from raw HP1 sprite definition
/// dictionaries.
///
/// Texture atlases decoded from <see cref="AssetStudio.Texture2D"/> objects are
/// cached by (file, path) so the same atlas is never decoded twice. Call
/// <see cref="FinalizeTextures"/> once all extraction is complete to upload
/// every cached atlas to the GPU and mark it read-only.
/// </summary>
public class HpSpriteCache
{
    private readonly Extractor _extractor;
    private readonly Dictionary<SerializedFile, Dictionary<UnityAssetPath, TextureInfoRaw>> _textureCache = new();

    public HpSpriteCache(Extractor extractor)
    {
        _extractor = extractor;
    }

    // -------------------------------------------------------------------------
    // Sprite collection helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a name→definition lookup and resolves the backing
    /// <see cref="TextureInfoRaw"/> for a sprite collection.
    /// </summary>
    public bool TryGetSpriteLookup(
        (SerializedFile file, OrderedDictionary def) spriteCollection,
        out Dictionary<string, OrderedDictionary> spriteLookup,
        out TextureInfoRaw textureInfo)
    {
        if (!spriteCollection.def.TryGetValue("spriteDefinitions", out List<object> spriteDefinitions)
            || !spriteCollection.def.TryGetValue("textures", out List<object> textureDefs)
            || textureDefs.Count != 1
            || textureDefs[0] is not OrderedDictionary idData
            || !UnityAssetPath.TryExtract(idData, out var texturePath))
        {
            spriteLookup = null;
            textureInfo = default;
            return false;
        }

        spriteLookup = spriteDefinitions
            .OfType<OrderedDictionary>()
            .Where(x => x != null && x.Contains("name") && x["name"] is string)
            .DistinctBy(x => (string)x["name"])
            .ToDictionary(x => (string)x["name"], x => x);

        textureInfo = GetOrCreateTextureInfo(spriteCollection.file, texturePath);
        return textureInfo != default;
    }

    // -------------------------------------------------------------------------
    // SpriteInfo builders
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a <see cref="SpriteInfoTexture"/> from a standard (non-tiled)
    /// HP1 sprite definition.
    /// </summary>
    public bool TryMakeSpriteInfo(
        OrderedDictionary spriteDef,
        TextureInfoRaw textureInfo,
        out SpriteInfoTexture info)
    {
        info = null;

        if (!spriteDef.TryGetValue("boundsData", out List<object> bounds)
            || bounds.Count != 2
            || !TryGetVector2((OrderedDictionary)bounds[1], out _, out var sizeY)
            || !spriteDef.TryGetValue("uvs", out List<object> uvs)
            || !spriteDef.TryGetValue("positions", out List<object> positions)
            || !spriteDef.TryGetValue("indices", out List<object> indices))
        {
            HpDebugLog.SpriteMessage("Failed to make sprite info");
            return false;
        }

        var uvVecs = uvs.OfType<OrderedDictionary>()
            .Select(o => TryGetVector2(o, out var u, out var v)
                ? new UE.Vector2(u, v)
                : throw new Exception("Malformed UV entry"))
            .ToArray();

        var positionVerts = positions.OfType<OrderedDictionary>()
            .Select(o => TryGetVector2(o, out float x, out float y)
                ? new UE.Vector2(x, sizeY + y)
                : UE.Vector2.zero)
            .ToArray();

        var minPos = positionVerts[0];
        var maxPos = positionVerts[0];
        foreach (var v in positionVerts.Skip(1))
        {
            minPos.x = UE.Mathf.Min(minPos.x, v.x);
            minPos.y = UE.Mathf.Min(minPos.y, v.y);
            maxPos.x = UE.Mathf.Max(maxPos.x, v.x);
            maxPos.y = UE.Mathf.Max(maxPos.y, v.y);
        }

        var triangles = indices.OfType<int>().Select(x => (ushort)x).ToArray();
        var textureSize = textureInfo.GetSize();
        var geomSize = maxPos - minPos;
        var scaleX = textureSize.x / geomSize.x;
        var scaleY = textureSize.y / geomSize.y;
        var scale = UE.Mathf.Min(scaleX, scaleY, 1f);

        // Snap scale down to next power-of-two step so seams align cleanly.
        var correctedScale = 1f;
        while (correctedScale > scale) correctedScale *= 0.5f;
        scale = correctedScale;

        positionVerts = positionVerts.Select(v => (v - minPos) * scale).ToArray();
        var finalSize = geomSize * scale;

        info = new SpriteInfoTexture(
            textureInfo,
            new UE.Rect(0, 0, finalSize.x, finalSize.y),
            positionVerts,
            uvVecs,
            triangles);

        return true;
    }

    /// <summary>
    /// Builds a <see cref="SpriteInfoTexture"/> for a tiled HP1 sprite,
    /// writing / reading through a disk cache at <paramref name="cachePath"/>.
    /// </summary>
    public bool TryMakeSpriteInfoTiled(
        string cachePath,
        OrderedDictionary spriteDef,
        TextureInfoRaw textureInfo,
        out SpriteInfoTexture info)
    {
        if (!TryMakeTextureInfoTiled(spriteDef, textureInfo, out var raster))
        {
            info = null;
            return false;
        }

        info = new SpriteInfoTexture(new TextureInfoCache(cachePath, raster));
        return true;
    }

    /// <summary>
    /// Builds a <see cref="SpriteInfoTexture"/> for a tiled HP1 sprite that is
    /// center-mirrored to fill <paramref name="targetWidth"/> ×
    /// <paramref name="targetHeight"/>, writing / reading through a disk cache
    /// at <paramref name="cachePath"/>.
    /// </summary>
    public bool TryMakeSpriteInfoTiledMirror(
        string cachePath,
        OrderedDictionary spriteDef,
        TextureInfoRaw textureInfo,
        int targetWidth,
        int targetHeight,
        out SpriteInfoTexture info)
    {
        if (!TryMakeTextureInfoTiled(spriteDef, textureInfo, out var raster))
        {
            info = null;
            return false;
        }

        ITextureInfo finalTexture = new TextureInfoCenterMirrored(raster, targetWidth, targetHeight);
        finalTexture = new TextureInfoCache(cachePath, finalTexture);

        info = new SpriteInfoTexture(finalTexture, new UE.Rect(0, 0, targetWidth, targetHeight));
        return true;
    }

    // -------------------------------------------------------------------------
    // Texture finalisation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Should be called once all extraction is complete. Uploads every decoded
    /// texture atlas to the GPU and marks it read-only.
    /// </summary>
    public void FinalizeTextures()
    {
        foreach (var textureInfo in _textureCache.Values.SelectMany(x => x.Values))
            textureInfo.GetTexture().Apply(false, true);
    }

    // -------------------------------------------------------------------------
    // Static geometry helper
    // -------------------------------------------------------------------------

    /// <summary>Extracts x/y floats from a Unity serialised Vector2 dictionary.</summary>
    public static bool TryGetVector2(OrderedDictionary dict, out float x, out float y)
    {
        if (dict.TryGetValue("x", out x) && dict.TryGetValue("y", out y)) return true;
        x = y = default;
        return false;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private TextureInfoRaw GetOrCreateTextureInfo(SerializedFile file, UnityAssetPath texturePath)
    {
        if (!_textureCache.TryGetValue(file, out var perFile))
        {
            perFile = new Dictionary<UnityAssetPath, TextureInfoRaw>();
            _textureCache[file] = perFile;
        }
        else if (perFile.TryGetValue(texturePath, out var cached))
        {
            return cached;
        }

        var asset = _extractor.GetAsset(file, texturePath);
        if (asset is not Texture2D tex2d)
            return default;

        var info = new TextureInfoRaw(
            tex2d.m_Width,
            tex2d.m_Height,
            tex2d.image_data.GetData(),
            (UE.TextureFormat)tex2d.m_TextureFormat,
            UE.FilterMode.Bilinear,
            UE.TextureWrapMode.Mirror,
            false);

        perFile[texturePath] = info;
        return info;
    }

    private bool TryMakeTextureInfoTiled(
        OrderedDictionary spriteDef,
        TextureInfoRaw textureInfo,
        out TextureInfoRasterized info)
    {
        if (!spriteDef.TryGetValue("boundsData", out List<object> bounds)
            || bounds.Count != 2
            || !TryGetVector2((OrderedDictionary)bounds[1], out _, out var sizeY)
            || !spriteDef.TryGetValue("uvs", out List<object> uvs)
            || !spriteDef.TryGetValue("positions", out List<object> positions)
            || !spriteDef.TryGetValue("indices", out List<object> indices))
        {
            HpDebugLog.SpriteMessage("Failed to make tiled sprite info");
            info = null;
            return false;
        }

        var uvVecs = uvs.OfType<OrderedDictionary>()
            .Select(o => TryGetVector2(o, out var u, out var v)
                ? new UE.Vector2(u, v)
                : throw new Exception("Malformed UV entry"))
            .ToArray();

        var positionVerts = positions.OfType<OrderedDictionary>()
            .Select(o => TryGetVector2(o, out float x, out float y)
                ? new UE.Vector2(x, sizeY + y)
                : UE.Vector2.zero)
            .ToArray();

        var minPos = positionVerts[0];
        var maxPos = positionVerts[0];
        foreach (var v in positionVerts)
        {
            minPos = UE.Vector2.Min(minPos, v);
            maxPos = UE.Vector2.Max(maxPos, v);
        }

        // Convert from Unity world units to pixel units using the atlas size
        // as the reference. Without this, RoundToInt on sub-unit floats collapses
        // the geometry to 1x1 pixels for backgrounds whose world-unit extents are < 1.
        var worldSize = maxPos - minPos;
        var texSize = textureInfo.GetSize();
        float scaleX = texSize.x / worldSize.x;
        float scaleY = texSize.y / worldSize.y;
        float scale = UE.Mathf.Min(scaleX, scaleY);

        for (int i = 0; i < positionVerts.Length; i++)
        {
            positionVerts[i] = (positionVerts[i] - minPos) * scale;
        }

        var sourceSize = worldSize * scale;
        int sourceWidth  = UE.Mathf.RoundToInt(sourceSize.x);
        int sourceHeight = UE.Mathf.RoundToInt(sourceSize.y);

        if (sourceWidth <= 0 || sourceHeight <= 0)
        {
            HpDebugLog.SpriteMessage("Invalid sprite dimensions");
            info = null;
            return false;
        }

        info = new TextureInfoRasterized(
            textureInfo,
            sourceWidth,
            sourceHeight,
            positionVerts,
            uvVecs,
            indices.OfType<int>().ToArray(),
            UE.FilterMode.Bilinear,
            UE.TextureWrapMode.Clamp);

        return true;
    }
}