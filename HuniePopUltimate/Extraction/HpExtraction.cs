using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AssetStudio;
using AssetStudio.Extractor;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace HuniePopUltimate;

public partial class HpExtraction : BaseExtraction
{
    public static readonly string _dataDir = "HuniePop_Data";
    public static readonly string _assemblyDir = Path.Combine(_dataDir, "Managed");

    public IReadOnlyDictionary<RelativeId, SingleDatePairData> SingleDatePairData => _singleDatePairData;
    private Dictionary<RelativeId, SingleDatePairData> _singleDatePairData = new();

    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, TextureInfoRaw>> _textureInfo = new();

    // Cached info replaces the old per-file vorbis dictionaries.
    // Keyed the same way as before so nothing else needs to change.
    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, AudioClipInfoCached>> _audioInfo = new();
    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, AudioClipInfoPCMStreamed>> _audioStreamedInfo = new();

    // Shared reader over the baked .pcmx file, initialised in InitAudioCache.
    private AudioPcmCacheReader _audioCacheReader;

    private Dictionary<int, GirlDataMod> _hpGirlIdToMod = new();

    private Dictionary<UnityAssetPath, PuzzleAffectionType> _affectionTypes = new(){
        {new UnityAssetPath(){ FileId = 0, PathId = 9371}, PuzzleAffectionType.TALENT},
        {new UnityAssetPath(){ FileId = 0, PathId = 9370}, PuzzleAffectionType.SEXUALITY},
        {new UnityAssetPath(){ FileId = 0, PathId = 9368}, PuzzleAffectionType.ROMANCE},
        {new UnityAssetPath(){ FileId = 0, PathId = 9366}, PuzzleAffectionType.FLIRTATION},
    };

    private GirlDataMod GetGirlMod(int nativeId)
    {
        if (!_hpGirlIdToMod.TryGetValue(nativeId, out var girlMod))
        {
            var id = Girls.FromHp1Id(nativeId);
            if (id == RelativeId.Default) return null;

            girlMod = new GirlDataMod(id, InsertStyle.append);
            _hpGirlIdToMod[nativeId] = girlMod;
        }
        return girlMod;
    }

    private (SerializedFile, OrderedDictionary) _thumbnailCollection;
    private int _partCount = 0;

    private Action<RelativeId, IEnumerable<(RelativeId, float)>> m_AddGirlDatePhotos;
    private Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> m_AddGirlSexPhotos;
    private Action<RelativeId, UnityEngine.Sprite> m_SetCharmSprite;
    private IBodySubDataMod<GirlPartSubDefinition> _nudeOutfitPart;
    private UnityEngine.AssetBundle _assetBundle;

    public HpExtraction(
        string dir,
        Action<RelativeId, IEnumerable<(RelativeId, float)>> addGirlDatePhotos,
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, UnityEngine.Sprite> setCharmSprite,
        IBodySubDataMod<GirlPartSubDefinition> nudeOutfitPart,
        UnityEngine.AssetBundle assetBundle)
        : base(Path.Combine(dir, _dataDir), Path.Combine(dir, _assemblyDir))
    {
        m_AddGirlDatePhotos = addGirlDatePhotos;
        m_AddGirlSexPhotos = addGirlSexPhotos;
        m_SetCharmSprite = setCharmSprite;
        _nudeOutfitPart = nudeOutfitPart;
        _assetBundle = assetBundle;
    }

    /// <summary>
    /// Must be called before <see cref="Extract"/>.
    /// If <paramref name="cacheFilePath"/> exists on disk the cache is opened
    /// directly (fast path).  Otherwise every Ogg clip in all serialized files
    /// is decoded, the cache is written, and the reader is opened over it.
    /// </summary>
    public void InitAudioCache(string cacheFilePath)
    {
        if (File.Exists(cacheFilePath))
        {
            ModInterface.Log.Message($"[AudioCache] Loading existing cache: {cacheFilePath}");
            _audioCacheReader = new AudioPcmCacheReader(cacheFilePath);
            ModInterface.Log.Message($"[AudioCache] {_audioCacheReader.Count} entries loaded.");
            return;
        }

        ModInterface.Log.Message($"[AudioCache] Cache not found — decoding all Ogg clips…");

        var requests = new List<AudioPcmCache.CacheBuildRequest>();

        foreach (SerializedFile file in _extractor.SerializedFiles)
        {
            // ObjectsDic is Dictionary<long, Object> — pathId is the key
            foreach (var kvp in file.ObjectsDic)
            {
                if (kvp.Value is not AssetStudio.AudioClip ac) continue;
                if (ac.m_Type != FMODSoundType.OGGVORBIS) continue;

                string key = AudioPcmCache.MakeKey(file, kvp.Key);
                requests.Add(new AudioPcmCache.CacheBuildRequest
                {
                    Key = key,
                    AudioData = ac.m_AudioData,
                });
            }
        }

        if (requests.Count == 0)
        {
            ModInterface.Log.Warning("[AudioCache] No Ogg clips found — cache not written.");
            return;
        }

        ModInterface.Log.Message($"[AudioCache] Decoding {requests.Count} clips…");
        _audioCacheReader = AudioPcmCache.Build(requests, cacheFilePath, SampleEncoding.PCM16);
        ModInterface.Log.Message($"[AudioCache] Cache written to: {cacheFilePath}");
    }

    public void Extract()
    {
        var collectionData = new Dictionary<string, (SerializedFile, OrderedDictionary)>();

        foreach (var file_behaviorList in _extractor.ExtractMonoBehaviors("tk2dSpriteCollectionData"))
        {
            foreach (var behavior in file_behaviorList.Value)
            {
                if (behavior.TryGetValue("spriteCollectionName", out string name))
                {
                    collectionData[name] = (file_behaviorList.Key, behavior);
                } 
            }
        }

        _thumbnailCollection = collectionData["AllPhotoIconsSpriteCollection"];

        foreach (var file_dialogTriggers in _extractor.ExtractMonoBehaviors("DialogTriggerDefinition"))
        {
            foreach (var dtDef in file_dialogTriggers.Value) ExtractDialogTrigger(dtDef, file_dialogTriggers.Key);
        }

        if (Plugin.AddCharacters.Value)
        {
            foreach (var file_girlDefs in _extractor.ExtractMonoBehaviors("GirlDefinition"))
            {
                foreach (var girlDef in file_girlDefs.Value) ExtractGirl(file_girlDefs.Key, girlDef, collectionData);
            }
        }

        foreach (var file_locationDef in _extractor.ExtractMonoBehaviors("LocationDefinition"))
        {
            foreach (var locationDef in file_locationDef.Value) ExtractLocation(file_locationDef.Key, locationDef, collectionData);
        }

        AddHp2GirlMods();

        foreach (var mod in _hpGirlIdToMod.Values) ModInterface.AddDataMod(mod);

        foreach (var textureInfo in _textureInfo.Values.SelectMany(x => x.Values)) textureInfo.GetTexture().Apply(false, true);

        var itemFile = _extractor.SerializedFiles.FirstOrDefault(x => x.fileName == "sharedassets0.assets");
        if (itemFile != null
            && collectionData.TryGetValue("ItemIconsSpriteCollection", out var itemIconSpriteCollection)
            && TryGetSpriteLookup(itemIconSpriteCollection, out var itemIconSpriteLookup, out var textureInfoRaw))
        {
            int i = 0;
            foreach (var item in _extractor.ExtractMonoBehaviors(itemFile, "ItemDefinition", [9184, 9167]))
            {
                switch (i++)
                {
                    case 0: ExtractGoldfish(item, itemIconSpriteLookup, textureInfoRaw); break;
                    case 1: ExtractWeirdThing(item, itemIconSpriteLookup, textureInfoRaw); break;
                }
            }
        }
        else
        {
            ModInterface.Log.Warning("Failed to find sharedassets0.assets");
        }
    }

    private bool TryExtractAudioDef(
        OrderedDictionary audioDef,
        SerializedFile file,
        out AudioClipInfoCached clipInfo)
    {
        if (audioDef.TryGetValue("clip", out OrderedDictionary clip)
            && UnityAssetPath.TryExtract(clip, out var unityPath)
            && TryGetAudioClipInfo(file, unityPath, out clipInfo))
        {
            return true;
        }
        clipInfo = null;
        return false;
    }

    private bool TryExtractAudioDefStreamed(
        OrderedDictionary audioDef,
        SerializedFile file,
        string directory,
        out AudioClipInfoPCMStreamed clipInfo)
    {
        if (audioDef.TryGetValue("clip", out OrderedDictionary clip)
            && UnityAssetPath.TryExtract(clip, out var unityPath)
            && TryGetAudioClipInfoStreamed(file, directory, unityPath, out clipInfo))
        {
            return true;
        }
        clipInfo = null;
        return false;
    }

    /// <summary>
    /// Returns an <see cref="AudioClipInfoCached"/> for the clip at
    /// <paramref name="path"/> inside <paramref name="file"/>.
    /// Looks up the per-file dictionary first; on miss, opens a slice from the
    /// shared cache reader and inserts it.
    /// </summary>
    private bool TryGetAudioClipInfo(
        SerializedFile file,
        UnityAssetPath path,
        out AudioClipInfoCached clipInfo)
    {
        var pathToInfo = _audioInfo.GetOrNew(file);

        if (pathToInfo.TryGetValue(path, out clipInfo))
            return true;

        if (_audioCacheReader == null)
        {
            ModInterface.Log.Warning("[AudioCache] TryGetAudioClipInfo called before InitAudioCache.");
            clipInfo = null;
            return false;
        }

        // Resolve the target SerializedFile from the UnityAssetPath.
        // FileId == 0 means the asset lives in the same file.
        // FileId > 0 is a 1-based index into m_Externals; match by fileName
        // against the assetsManager's loaded file list.
        SerializedFile targetFile;
        if (path.FileId == 0)
        {
            targetFile = file;
        }
        else
        {
            int extIndex = path.FileId - 1;
            if (extIndex < 0 || extIndex >= file.m_Externals.Count)
            {
                ModInterface.Log.Warning($"[AudioCache] External index {extIndex} out of range for {file.fileName}");
                clipInfo = null;
                return false;
            }

            string extFileName = file.m_Externals[extIndex].fileName;
            targetFile = file.assetsManager.assetsFileList
                .FirstOrDefault(f => string.Equals(f.fileName, extFileName, StringComparison.OrdinalIgnoreCase));

            if (targetFile == null)
            {
                ModInterface.Log.Warning($"[AudioCache] Could not resolve external file: {extFileName}");
                clipInfo = null;
                return false;
            }
        }

        PcmSliceHandle handle = _audioCacheReader.OpenSlice(targetFile, path.PathId);

        if (handle == null)
        {
            // Clip was not found in cache (e.g. non-Ogg type skipped during build)
            clipInfo = null;
            return false;
        }

        clipInfo = new AudioClipInfoCached(handle, path.ToString());
        pathToInfo[path] = clipInfo;
        return true;
    }

    private bool TryGetAudioClipInfoStreamed(
        SerializedFile file,
        string directory,
        UnityAssetPath path,
        out AudioClipInfoPCMStreamed clipInfo)
    {
        var pathToInfo = _audioStreamedInfo.GetOrNew(file);

        if (pathToInfo.TryGetValue(path, out clipInfo))
            return true;

        var audioClip = (AssetStudio.AudioClip)_extractor.GetAsset(file, path);

        if (audioClip.m_Type == FMODSoundType.OGGVORBIS)
        {
            clipInfo = new AudioClipInfoPCMStreamed(
                audioClip.m_AudioData,
                Path.Combine(directory, path.ToString() + ".wav"));
            return true;
        }

        clipInfo = null;
        return false;
    }

    private static void LogAll(OrderedDictionary dict)
    {
        var keys = dict.Keys.GetEnumerator();
        var vals = dict.Values.GetEnumerator();
        while (keys.MoveNext() && vals.MoveNext())
        {
            if (vals.Current is OrderedDictionary subDict)
            {
                using (ModInterface.Log.MakeIndent(keys.Current.ToString()))
                    LogAll(subDict);
            }
            else if (vals.Current is List<object> list)
            {
                using (ModInterface.Log.MakeIndent(keys.Current.ToString()))
                {
                    int i = 0;
                    foreach (var entry in list)
                    {
                        using (ModInterface.Log.MakeIndent($"{i++}"))
                        {
                            if (entry is OrderedDictionary od) LogAll(od);
                            else ModInterface.Log.Message($"{vals.Current?.ToString() ?? "null"}");
                        }
                    }
                }
            }
            else
            {
                ModInterface.Log.Message($"{keys.Current} : {vals.Current?.ToString() ?? "null"}");
            }
        }
    }

    private bool TryMakeSpriteInfo(
        OrderedDictionary spriteDef,
        TextureInfoRaw textureInfo,
        out SpriteInfoTexture info)
    {
        info = null;

        if (!spriteDef.TryGetValue("boundsData", out List<object> bounds)
            || bounds.Count != 2
            || !TryGetVector2((OrderedDictionary)bounds[0], out var centerX, out var centerY)
            || !TryGetVector2((OrderedDictionary)bounds[1], out var sizeX, out var sizeY)
            || !spriteDef.TryGetValue("uvs", out List<object> uvs)
            || !spriteDef.TryGetValue("positions", out List<object> positions)
            || !spriteDef.TryGetValue("indices", out List<object> indices))
        {
            ModInterface.Log.Message("Failed to make sprite info");
            return false;
        }

        var uvVecs = uvs.OfType<OrderedDictionary>()
            .Select(o => TryGetVector2(o, out var u, out var v)
                ? new UnityEngine.Vector2(u, v)
                : throw new Exception())
            .ToArray();

        var positionVerts = positions.OfType<OrderedDictionary>()
            .Select(o => TryGetVector2(o, out float x, out float y)
                ? new UnityEngine.Vector2(x, sizeY + y)
                : UnityEngine.Vector2.zero)
            .ToArray();

        var minPos = positionVerts[0];
        var maxPos = positionVerts[0];
        foreach (var v in positionVerts.Skip(1))
        {
            minPos.x = Mathf.Min(minPos.x, v.x);
            minPos.y = Mathf.Min(minPos.y, v.y);
            maxPos.x = Mathf.Max(maxPos.x, v.x);
            maxPos.y = Mathf.Max(maxPos.y, v.y);
        }

        var triangles = indices.OfType<int>().Select(x => (ushort)x).ToArray();
        var textureSize = textureInfo.GetSize();
        var geomSize = maxPos - minPos;
        var scaleX = textureSize.x / geomSize.x;
        var scaleY = textureSize.y / geomSize.y;
        var scale = Mathf.Min(scaleX, scaleY, 1f);

        var correctedScale = 1f;
        while (correctedScale > scale) correctedScale *= 0.5f;
        scale = correctedScale;

        positionVerts = positionVerts.Select(v => (v - minPos) * scale).ToArray();
        var finalSize = geomSize * scale;

        info = new SpriteInfoTexture(
            textureInfo,
            new Rect(0, 0, finalSize.x, finalSize.y),
            positionVerts,
            uvVecs,
            triangles);

        return true;
    }

    private bool TryMakeTextureInfoTiled(OrderedDictionary spriteDef,
        TextureInfoRaw textureInfo,
        out TextureInfoRasterized info)
    {
        if (!spriteDef.TryGetValue("boundsData", out List<object> bounds)
            || bounds.Count != 2
            || !TryGetVector2((OrderedDictionary)bounds[0], out var centerX, out var centerY)
            || !TryGetVector2((OrderedDictionary)bounds[1], out var sizeX, out var sizeY)
            || !spriteDef.TryGetValue("uvs", out List<object> uvs)
            || !spriteDef.TryGetValue("positions", out List<object> positions)
            || !spriteDef.TryGetValue("indices", out List<object> indices))
        {
            ModInterface.Log.Message("Failed to make tiled sprite info");
            info = null;
            return false;
        }

        var uvVecs = uvs.OfType<OrderedDictionary>()
            .Select(o => TryGetVector2(o, out var u, out var v)
                ? new UnityEngine.Vector2(u, v)
                : throw new Exception())
            .ToArray();

        var positionVerts = positions.OfType<OrderedDictionary>()
            .Select(o => TryGetVector2(o, out float x, out float y)
                ? new UnityEngine.Vector2(x, sizeY + y)
                : UnityEngine.Vector2.zero)
            .ToArray();

        var minPos = positionVerts[0];
        var maxPos = positionVerts[0];

        foreach (var v in positionVerts)
        {
            minPos = UnityEngine.Vector2.Min(minPos, v);
            maxPos = UnityEngine.Vector2.Max(maxPos, v);
        }

        // normalize to origin (mesh space only)
        for (int i = 0; i < positionVerts.Length; i++)
            positionVerts[i] -= minPos;

        var sourceSize = maxPos - minPos;

        int sourceWidth = Mathf.RoundToInt(sourceSize.x);
        int sourceHeight = Mathf.RoundToInt(sourceSize.y);

        if (sourceWidth <= 0 || sourceHeight <= 0)
        {
            ModInterface.Log.Message("Invalid sprite dimensions");
            info = null;
            return false;
        }

        var triangles = indices.OfType<int>().ToArray();

        info = new TextureInfoRasterized(
            textureInfo,
            sourceWidth,
            sourceHeight,
            positionVerts,
            uvVecs,
            triangles,
            FilterMode.Bilinear,
            TextureWrapMode.Clamp
        );

        return true;
    }

    private bool TryMakeSpriteInfoTiledMirror(
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

        ITextureInfo finalTexture = new TextureInfoCenterMirrored(
            raster,
            targetWidth,
            targetHeight
        );

        finalTexture = new TextureInfoCache(cachePath, finalTexture);

        info = new SpriteInfoTexture(
            finalTexture,
            new Rect(0, 0, targetWidth, targetHeight)
        );

        return true;
    }

    private bool TryMakeSpriteInfoTiled(
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

    private bool TryGetSpriteLookup(
        (SerializedFile file, OrderedDictionary def) spriteCollection,
        out Dictionary<string, OrderedDictionary> spriteLookup,
        out TextureInfoRaw textureInfo)
    {
        if (spriteCollection.def.TryGetValue("spriteDefinitions", out List<object> spriteDefinitions)
            && spriteCollection.def.TryGetValue("textures", out List<object> textureDefs))
        {
            if (!(textureDefs.Count == 1
                    && textureDefs.First() is OrderedDictionary idData
                    && UnityAssetPath.TryExtract(idData, out var texturePath)))
            {
                spriteLookup = null;
                textureInfo = default;
                return false;
            }

            spriteLookup = spriteDefinitions
                .OfType<OrderedDictionary>()
                .Where(x => x != null && x.Contains("name") && x["name"] is string)
                .DistinctBy(x => (string)x["name"])
                .ToDictionary(x => x["name"] as string, x => x);

            if (!_textureInfo.TryGetValue(spriteCollection.file, out var textures))
            {
                textures = new();
                _textureInfo[spriteCollection.file] = textures;
            }
            else if (textures.TryGetValue(texturePath, out textureInfo))
            {
                return true;
            }

            var textureAsset = _extractor.GetAsset(spriteCollection.file, texturePath);
            if (textureAsset is AssetStudio.Texture2D asTexture2d)
            {
                textureInfo = new TextureInfoRaw(
                    asTexture2d.m_Width,
                    asTexture2d.m_Height,
                    asTexture2d.image_data.GetData(),
                    (UnityEngine.TextureFormat)asTexture2d.m_TextureFormat,
                    UnityEngine.FilterMode.Bilinear,
                    TextureWrapMode.Mirror,
                    false);

                textures[texturePath] = textureInfo;
                return true;
            }
        }

        spriteLookup = null;
        textureInfo = default;
        return false;
    }

    private static bool TryGetVector2(OrderedDictionary dict, out float x, out float y)
    {
        if (dict.TryGetValue("x", out x) && dict.TryGetValue("y", out y)) return true;
        x = y = default;
        return false;
    }
}