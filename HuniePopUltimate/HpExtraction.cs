using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AssetStudio;
using ETAR.AssetStudioPlugin.Extractor;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.Extension.OrderedDictionaryExtension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using NVorbis;
using UnityEngine;

namespace HuniePopUltimate;
public class HpExtraction : BaseExtraction
{
    public static readonly string _dataDir = "HuniePop_Data";
    public static readonly string _assemblyDir = Path.Combine(_dataDir, "Managed");

    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, TextureInfoRaw>> _textureInfo = new();
    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, IGameDefinitionInfo<UnityEngine.AudioClip>>> _audioInfo = new();
    private (SerializedFile, OrderedDictionary) _thumbnailCollection;

    public HpExtraction(string dir)
        : base(Path.Combine(dir, _dataDir),
            Path.Combine(dir, _assemblyDir))
    {

    }

    public void Extract()
    {
        var collectionData = new Dictionary<string, (SerializedFile, OrderedDictionary)>();

        foreach (var file_behviorList in _extractor.ExtractMonobehaviors("tk2dSpriteCollectionData"))
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

        var girls = _extractor.ExtractMonobehaviors("GirlDefinition");

        foreach (var file_girlDefs in _extractor.ExtractMonobehaviors("GirlDefinition"))
        {
            foreach (var girlDef in file_girlDefs.Value)
            {
                ExtractGirl(girlDef, collectionData);
            }
        }


        foreach (var file_locationDef in _extractor.ExtractMonobehaviors("LocationDefinition"))
        {
            foreach (var locationDef in file_locationDef.Value)
            {
                ExtractLocation(file_locationDef.Key, locationDef, collectionData);
            }
        }
    }

    private int _locationCount = 0;

    private ITextureRenderStep[] _finderIconSteps = [
        new TextureRsCellphoneOutline(4f, 0f, 1f),
    ];

    private void ExtractLocation(SerializedFile file, OrderedDictionary locationDef, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData)
    {
        var locationMod = new LocationDataMod(new RelativeId(Plugin.ModId, _locationCount++), InsertStyle.replace);

        if (locationDef.TryGetValue("name", out string locationName))
        {
            locationMod.LocationName = locationName;
            ModInterface.Log.LogInfo(locationName);
        }

        if (_locationNameToIconOutlined.TryGetValue(locationName, out var iconOutlinedName))
        {
            locationMod.FinderLocationIcon = new SpriteInfoTexture(
                new TextureInfoCache(
                    Path.Combine(Plugin.RootDir, "images", $"{locationName}_icon.png"),
                    new TextureInfoSprite(new SpriteInfoInternal(iconOutlinedName), true, _finderIconSteps)
                )
            );
        }
        else if (_locationNameToIconInternal.TryGetValue(locationName, out var iconInternalName))
        {
            locationMod.FinderLocationIcon = new SpriteInfoInternal(iconInternalName);
        }

        using (ModInterface.Log.MakeIndent())
        {
            if (locationDef.TryGetValue("type", out int locationType))
            {
                locationMod.LocationType = locationType == 0
                    ? LocationType.SIM
                    : LocationType.DATE;
            }

            if (locationDef.TryGetValue("spriteCollectionName", out string spriteCollectionName)
                && collectionData.TryGetValue(spriteCollectionName, out var spriteCollection)
                && TryGetSpriteLookup(spriteCollection, out var spriteLookup, out var spriteTextureInfo)
                && locationDef.TryGetValue("backgrounds", out List<object> backgrounds))
            {
                var backgroundSprites = new Dictionary<ClockDaytimeType, IGameDefinitionInfo<UnityEngine.Sprite>>();

                foreach (var background in backgrounds.OfType<OrderedDictionary>())
                {
                    if (background.TryGetValue("backgroundName", out string backgroundName)
                        && !string.IsNullOrEmpty(backgroundName)
                        && spriteLookup.TryGetValue(backgroundName, out var spriteDef)
                        && TryMakeSpriteInfo(spriteDef, spriteTextureInfo, out var spriteInfo)
                        && background.TryGetValue("daytime", out int dayTime))
                    {
                        backgroundSprites[(ClockDaytimeType)dayTime] = spriteInfo;
                    }

                    if (background.TryGetValue("musicDefinition", out OrderedDictionary musicDef)
                        && background.TryGetValue("musicVolume", out float musicVolume)
                        && musicDef.TryGetValue("clip", out OrderedDictionary clip)
                        && UnityAssetPath.TryExtract(clip, out var unityPath)
                        && TryGetAudioClipInfo(file, unityPath, out var clipInfo))
                    {
                        locationMod.BgMusic = new AudioKlipInfo()
                        {
                            AudioClipInfo = clipInfo,
                            Volume = musicVolume
                        };
                    }
                }

                var defaultBg = backgroundSprites.Values.First();
                if (locationMod.LocationType == LocationType.SIM)
                {
                    locationMod.Backgrounds = [
                        backgroundSprites.TryGetValue(ClockDaytimeType.AFTERNOON, out var morningBg) ? morningBg : defaultBg,
                        backgroundSprites.TryGetValue(ClockDaytimeType.AFTERNOON, out var afternoonBg) ? afternoonBg : defaultBg,
                        backgroundSprites.TryGetValue(ClockDaytimeType.EVENING, out var eveningBg) ? eveningBg : defaultBg,
                        backgroundSprites.TryGetValue(ClockDaytimeType.NIGHT, out var nightBg) ? nightBg : defaultBg
                    ];
                }
                else
                {
                    locationMod.Backgrounds = [defaultBg, defaultBg];
                }
            }

            locationMod.DateTimes = [
                ClockDaytimeType.MORNING,
                ClockDaytimeType.AFTERNOON,
                ClockDaytimeType.EVENING,
                ClockDaytimeType.NIGHT,
            ];
            ModInterface.AddDataMod(locationMod);
        }
    }

    private void LogAll(OrderedDictionary dict)
    {
        var keys = dict.Keys.GetEnumerator();
        var vals = dict.Values.GetEnumerator();
        while (keys.MoveNext() && vals.MoveNext())
        {
            ModInterface.Log.LogInfo(keys.Current.ToString());
            using (ModInterface.Log.MakeIndent())
            {
                if (vals.Current is OrderedDictionary subDict)
                {
                    LogAll(subDict);
                }
                else
                {
                    ModInterface.Log.LogInfo($"{vals.Current?.ToString() ?? "null"}");
                }
            }
        }
    }

    private Dictionary<int, HashSet<int>> _uncensoredPhotos = new Dictionary<int, HashSet<int>>(){
        {1,[2]},//tiffany
        {2,[2]},//aiko
        {3,[2]},//kyanna
        {4,[]},//Audrey
        {5,[3]},//lola
        {6,[3]},//nikki
        {7,[2,3]},//jessie
        {8,[2,3]},//beli
        {9,[1,2,3]},//kyu
        {10,[2,3]},//momo
        {11,[2,3]},//celeste
        {12,[2]},//venus
    };

    private Dictionary<string, string> _locationNameToIconOutlined = new Dictionary<string, string>()
    {
        {"Bar","item_unique_whisky"},
        {"Beach","item_date_beach_ball"},
        {"Cafe","item_baggage_caffeine_junkie"},
        {"Campus","item_baggage_intellectually_challenged"},
        {"Gym","item_baggage_abandonment_issues"},
        {"Mall","item_baggage_brand_loyalist"},
        {"Nightclub","item_unique_gin"},
        {"Park","item_date_green_clover"},
    };

    private Dictionary<string, string> _locationNameToIconInternal = new Dictionary<string, string>()
    {
        {"Bedroom","ui_location_icon_room"},
    };

    private void ExtractGirl(OrderedDictionary girlDef, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData)
    {
        var censoredIndex = -1;
        var nudeIndex = -1;
        var wetIndex = -1;

        if (girlDef.TryGetValue("id", out int nativeId))
        {
            switch (nativeId)
            {
                case 2:
                case 4:
                case 3:
                    nudeIndex = 0;
                    wetIndex = 1;
                    break;
                default:
                    censoredIndex = 0;
                    nudeIndex = 2;
                    wetIndex = 3;
                    break;
            }
        }

        _uncensoredPhotos.TryGetValue(nativeId, out var censoredIndexes);

        var girlMod = new GirlDataMod();

        if (girlDef.TryGetValue("firstName", out string firstName))
        {
            ModInterface.Log.LogInfo($"id:{nativeId}, name:{firstName}");
            girlMod.GirlName = firstName;
        }

        // if (girlDef.TryGetValue("spriteCollectionName", out string spriteCollectionName)
        //     && collectionData.TryGetValue(spriteCollectionName, out var spriteCollection)
        //     && TryGetSpriteLookup(spriteCollection, out var spriteLookup, out var spriteTextureInfo)
        //     && girlDef.TryGetValue("pieces", out List<object> pieces))
        // {
        //     var bodyParts = new Dictionary<int, object>();
        //     if (TryExtractGirlPieceArtFrom(girlDef, "headPiece", _catalog.Layers.Head, spriteLookup, collectionId, out var headPart))
        //     {
        //         bodyParts[_catalog.Layers.Head] = headPart;
        //     }
        //     if (TryExtractGirlPieceArtFrom(girlDef, "bodyPiece", _catalog.Layers.Body, spriteLookup, collectionId, out var bodyPart))
        //     {
        //         bodyParts[_catalog.Layers.Body] = bodyPart;
        //     }

        //     var outfits = new Dictionary<int, object>();
        //     var hairStyles = new Dictionary<int, object>();
        //     var eyeParts = new Dictionary<int, object>();
        //     var browParts = new Dictionary<int, object>();
        //     var mouthParts = new Dictionary<int, object>();
        //     var extras = new Dictionary<int, object>();

        //     girlMod.parts = [

        //     ];
        // }

        if (girlDef.TryGetValue("photosSpriteCollectionName", out string photosSpriteCollectionName)
            && collectionData.TryGetValue(photosSpriteCollectionName, out var photoCollection)
            && TryGetSpriteLookup(photoCollection, out var photoLookup, out var photoTextureInfo)
            && TryGetSpriteLookup(_thumbnailCollection, out var thumbLookup, out var thumbTextureInfo)
            && girlDef.TryGetValue("photos", out List<object> photos))
        {
            int j = -1;
            foreach (var girlPhoto in photos.OfType<OrderedDictionary>())
            {
                j++;
                var photoMod = new PhotoDataMod(new RelativeId(Plugin.ModId, Plugin.PhotoModCount++), Hp2BaseMod.Utility.InsertStyle.replace);

                if (girlPhoto.TryGetValue("fullSpriteName", out List<object> fullSpriteName)
                    && girlPhoto.TryGetValue("thumbnailSpriteName", out List<object> thumbnailSpriteName))
                {
                    var thumbnailInfo = new IGameDefinitionInfo<UnityEngine.Sprite>[thumbnailSpriteName.Count];

                    int nameIndex = -1;
                    int i = 0;
                    foreach (var name in thumbnailSpriteName.OfType<string>())
                    {
                        if (!string.IsNullOrEmpty(name)
                            && thumbLookup.TryGetValue(name, out var def)
                            && TryMakeSpriteInfo(def, thumbTextureInfo, out var info))
                        {
                            thumbnailInfo[i] = info;
                        }

                        i++;
                    }

                    var isSinglePhoto = fullSpriteName.Count == 1;

                    foreach (var name in fullSpriteName.OfType<string>())
                    {
                        nameIndex++;
                        var isUncensored = censoredIndexes?.Contains(j) ?? false;

                        if (string.IsNullOrEmpty(name)
                            || !photoLookup.TryGetValue(name, out var photoDef)
                            || (nameIndex == censoredIndex && (isUncensored && !isSinglePhoto)))
                        {
                            continue;
                        }

                        if (TryMakeSpriteInfo(photoDef, photoTextureInfo, out var photoInfo))
                        {
                            if (isSinglePhoto)
                            {
                                if (isUncensored)
                                {
                                    photoMod.BigPhotoUncensored = photoInfo;
                                    photoMod.ThumbnailUncensored = thumbnailInfo[0];
                                }
                                else
                                {
                                    photoMod.BigPhotoCensored = photoInfo;
                                    photoMod.ThumbnailCensored = thumbnailInfo[0];
                                }
                            }
                            else if (nameIndex == censoredIndex)
                            {
                                photoMod.BigPhotoCensored = photoInfo;
                                photoMod.ThumbnailCensored = thumbnailInfo[1];
                            }
                            else if (nameIndex == nudeIndex)
                            {
                                photoMod.BigPhotoUncensored = photoInfo;
                                photoMod.ThumbnailUncensored = thumbnailInfo[0];
                            }
                            else if (nameIndex == wetIndex)
                            {
                                photoMod.BigPhotoWet = photoInfo;
                            }
                        }
                        else
                        {
                            ModInterface.Log.LogWarning("Failed to make sprite info");
                        }
                    }

                    ModInterface.AddDataMod(photoMod);
                }
            }
        }
    }

    private bool TryMakeSpriteInfo(OrderedDictionary spriteDef, TextureInfoRaw textureInfo, out IGameDefinitionInfo<UnityEngine.Sprite> info)
    {
        if (spriteDef.TryGetValue("boundsData", out List<object> bounds)
            && bounds.Count == 2
            && TryGetVector2((OrderedDictionary)bounds[0], out float centerX, out float centerY)
            && TryGetVector2((OrderedDictionary)bounds[1], out float sizeX, out float sizeY)
            && spriteDef.TryGetValue("uvs", out List<object> uvs)
            && spriteDef.TryGetValue("positions", out List<object> positions)
            && spriteDef.TryGetValue("indices", out List<object> indices)
            && spriteDef.TryGetValue("flipped", out object flipped)
            && flipped is int flippedInt)
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

            var ratioX = textureSize.x / sizeX;
            var ratioY = textureSize.y / sizeY;

            var vertScaler = Mathf.Min(Mathf.Min(diffSizeMult.x, diffSizeMult.y), 1f);

            var positionVerts = positions.OfType<OrderedDictionary>().Select(o => TryGetVector2(o, out float x, out float y)
                    ? new UnityEngine.Vector2(x, sizeY + y) * vertScaler
                    : UnityEngine.Vector2.zero).ToArray();

            var triangles = indices.OfType<int>().Select(x => (ushort)x).ToArray();

            //some have negative verts, so shift min to zero
            var minPos = new UnityEngine.Vector2(positionVerts.Min(x => x.x), positionVerts.Min(x => x.y));

            info = new SpriteInfoTexture(textureInfo,
                new Rect(0, 0, sizeX * vertScaler, sizeY * vertScaler),
                positionVerts.Select(x => x - minPos).ToArray(),
                uvVecs,
                indices.OfType<int>().Select(x => (ushort)x).ToArray());

            return true;
        }

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
                    null);

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

    private static bool TryGetVector(OrderedDictionary orderedDictionary, out int x, out int y)
    {
        if (orderedDictionary.TryGetValue("x", out x) && orderedDictionary.TryGetValue("y", out y))
        {
            return true;
        }

        x = default;
        y = default;
        return false;
    }

    private bool TryGetAudioClipInfo(SerializedFile file, UnityAssetPath path, out IGameDefinitionInfo<UnityEngine.AudioClip> clipInfo)
    {
        if (!_audioInfo.TryGetValue(file, out var pathToInfo))
        {
            pathToInfo = new Dictionary<UnityAssetPath, IGameDefinitionInfo<UnityEngine.AudioClip>>();
            _audioInfo[file] = pathToInfo;
        }

        if (pathToInfo.TryGetValue(path, out clipInfo))
        {
            return true;
        }

        var audioClip = (AssetStudio.AudioClip)_extractor.GetAsset(file, path);

        if (audioClip.m_Type == FMODSoundType.OGGVORBIS)
        {
            clipInfo = new AudioClipInfoVorbis(new VorbisReader(new MemoryStream(audioClip.m_AudioData.GetData()), true));
            return true;
        }

        clipInfo = null;
        return false;
    }
}
