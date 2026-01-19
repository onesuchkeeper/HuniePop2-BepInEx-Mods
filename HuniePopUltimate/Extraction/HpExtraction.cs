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
    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, AudioClipInfoVorbis>> _audioInfo = new();
    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, AudioClipInfoPCMStreamed>> _audioStreamedInfo = new();
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
            if (id == RelativeId.Default)
            {
                return null;
            }

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

    public HpExtraction(string dir,
        Action<RelativeId, IEnumerable<(RelativeId, float)>> addGirlDatePhotos,
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        Action<RelativeId, UnityEngine.Sprite> setCharmSprite,
        IBodySubDataMod<GirlPartSubDefinition> nudeOutfitPart)
        : base(Path.Combine(dir, _dataDir),
            Path.Combine(dir, _assemblyDir))
    {
        m_AddGirlDatePhotos = addGirlDatePhotos;
        m_AddGirlSexPhotos = addGirlSexPhotos;
        m_SetCharmSprite = setCharmSprite;
        _nudeOutfitPart = nudeOutfitPart;
    }

    public void Extract()
    {
        var collectionData = new Dictionary<string, (SerializedFile, OrderedDictionary)>();

        foreach (var file_behviorList in _extractor.ExtractMonoBehaviors("tk2dSpriteCollectionData"))
        {
            foreach (var behavior in file_behviorList.Value)
            {
                if (behavior.TryGetValue("spriteCollectionName", out string name))
                {
                    collectionData[name] = (file_behviorList.Key, behavior);
                }
            }
        }

        _thumbnailCollection = collectionData["AllPhotoIconsSpriteCollection"];

        foreach (var file_dialogTriggers in _extractor.ExtractMonoBehaviors("DialogTriggerDefinition"))
        {
            foreach (var dtDef in file_dialogTriggers.Value)
            {
                ExtractDialogTrigger(dtDef, file_dialogTriggers.Key);
            }
        }

        if (Plugin.AddCharacters.Value)
        {
            foreach (var file_girlDefs in _extractor.ExtractMonoBehaviors("GirlDefinition"))
            {
                foreach (var girlDef in file_girlDefs.Value)
                {
                    ExtractGirl(file_girlDefs.Key, girlDef, collectionData);
                }
            }
        }

        foreach (var file_locationDef in _extractor.ExtractMonoBehaviors("LocationDefinition"))
        {
            foreach (var locationDef in file_locationDef.Value)
            {
                ExtractLocation(file_locationDef.Key, locationDef, collectionData);
            }
        }

        AddHp2GirlMods();

        foreach (var mod in _hpGirlIdToMod.Values)
        {
            ModInterface.AddDataMod(mod);
        }

        // make all textures read-only now that they've been processed
        foreach (var textureInfo in _textureInfo.Values.SelectMany(x => x.Values))
        {
            textureInfo.GetTexture().Apply(false, true);
        }

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
                    case 0:
                        ExtractGoldfish(item, itemIconSpriteLookup, textureInfoRaw);
                        break;
                    case 1:
                        ExtractWeirdThing(item, itemIconSpriteLookup, textureInfoRaw);
                        break;
                }
            }
        }
        else
        {
            ModInterface.Log.Warning("Failed to find sharedassets0.assets");
        }
    }

    private bool TryExtractAudioDef(OrderedDictionary AudioDef, SerializedFile file, out AudioClipInfoVorbis clipInfo)
    {
        if (AudioDef.TryGetValue("clip", out OrderedDictionary clip)
            && UnityAssetPath.TryExtract(clip, out var unityPath)
            && TryGetAudioClipInfo(file, unityPath, out clipInfo))
        {
            return true;
        }
        clipInfo = null;
        return false;
    }

    private bool TryExtractAudioDefStreamed(OrderedDictionary AudioDef, SerializedFile file, string directory, out AudioClipInfoPCMStreamed clipInfo)
    {
        if (AudioDef.TryGetValue("clip", out OrderedDictionary clip)
            && UnityAssetPath.TryExtract(clip, out var unityPath)
            && TryGetAudioClipInfoStreamed(file, directory, unityPath, out clipInfo))
        {
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
                {
                    LogAll(subDict);
                }
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
                            if (entry is OrderedDictionary orderedDict) LogAll(orderedDict);
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

    private bool TryMakeSpriteInfo(OrderedDictionary spriteDef, TextureInfoRaw textureInfo, out SpriteInfoTexture info)
    {
        if (spriteDef.TryGetValue("boundsData", out List<object> bounds)
            && bounds.Count == 2
            && TryGetVector2((OrderedDictionary)bounds[0], out float centerX, out float centerY)
            && TryGetVector2((OrderedDictionary)bounds[1], out float sizeX, out float sizeY)
            && spriteDef.TryGetValue("uvs", out List<object> uvs)
            && spriteDef.TryGetValue("positions", out List<object> positions)
            && spriteDef.TryGetValue("indices", out List<object> indices))
        {
            var uvVecs = uvs.OfType<OrderedDictionary>().Select(o => TryGetVector2(o, out float u, out float v)
                    ? new UnityEngine.Vector2(u, v)
                    : throw new Exception())
                .ToArray();

            //scale verts down to fit in texture rect
            //annoying but unity won't accept verts outside and
            //sprites should be scaled up to their ui anyways
            var textureSize = textureInfo.GetSize();
            var spriteSize = new UnityEngine.Vector2(sizeX, sizeY);
            var diffSizeMult = textureSize / spriteSize;

            var vertScaler = Mathf.Min(Mathf.Min(diffSizeMult.x, diffSizeMult.y), 1f);

            var positionVerts = positions.OfType<OrderedDictionary>().Select(o => TryGetVector2(o, out float x, out float y)
                    ? new UnityEngine.Vector2(x, sizeY + y) * vertScaler
                    : UnityEngine.Vector2.zero).ToArray();

            //some have negative verts, so shift min to zero
            if (positionVerts.Any())
            {
                var minPos = new UnityEngine.Vector2(positionVerts[0].x, positionVerts[0].y);

                foreach (var pos in positionVerts.Skip(1))
                {
                    minPos.x = Mathf.Min(minPos.x, pos.x);
                    minPos.y = Mathf.Min(minPos.y, pos.y);
                }

                positionVerts = positionVerts.Select(x => x - minPos).ToArray();
            }

            var triangles = indices.OfType<int>().Select(x => (ushort)x).ToArray();

            info = new SpriteInfoTexture(textureInfo,
                new Rect(0, 0, sizeX * vertScaler, sizeY * vertScaler),
                positionVerts,
                uvVecs,
                triangles);

            return true;
        }

        ModInterface.Log.Message($"Failed to make sprite info");
        info = null;
        return false;
    }

    private bool TryGetSpriteLookup((SerializedFile file, OrderedDictionary def) spriteCollection,
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
                //there are duplicate keys, so 
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
                textureInfo = new TextureInfoRaw(asTexture2d.m_Width,
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

    private static bool TryGetVector2(OrderedDictionary orderedDictionary, out float x, out float y)
    {
        if (orderedDictionary.TryGetValue("x", out x) && orderedDictionary.TryGetValue("y", out y))
        {
            return true;
        }

        x = default;
        y = default;
        return false;
    }

    private bool TryGetAudioClipInfo(SerializedFile file, UnityAssetPath path, out AudioClipInfoVorbis clipInfo)
    {
        var pathToInfo = _audioInfo.GetOrNew(file);

        if (pathToInfo.TryGetValue(path, out clipInfo))
        {
            return true;
        }

        var audioClip = (AssetStudio.AudioClip)_extractor.GetAsset(file, path);

        if (audioClip.m_Type == FMODSoundType.OGGVORBIS)
        {
            clipInfo = new AudioClipInfoVorbis(audioClip.m_AudioData);
            return true;
        }

        clipInfo = null;
        return false;
    }

    private bool TryGetAudioClipInfoStreamed(SerializedFile file, string directory, UnityAssetPath path, out AudioClipInfoPCMStreamed clipInfo)
    {
        var pathToInfo = _audioStreamedInfo.GetOrNew(file);

        if (pathToInfo.TryGetValue(path, out clipInfo))
        {
            return true;
        }

        var audioClip = (AssetStudio.AudioClip)_extractor.GetAsset(file, path);

        if (audioClip.m_Type == FMODSoundType.OGGVORBIS)
        {
            clipInfo = new AudioClipInfoPCMStreamed(audioClip.m_AudioData, Path.Combine(directory, path.ToString() + ".wav"));
            return true;
        }

        clipInfo = null;
        return false;
    }
}
