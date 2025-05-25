using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AssetStudio;
using BepInEx;
using ETAR.AssetStudioPlugin.Extractor;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.Extension.OrderedDictionaryExtension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;

namespace HuniePopUltimate;
public class HpExtraction : BaseExtraction
{
    public static readonly string _dataDir = "HuniePop_Data";
    public static readonly string _assemblyDir = Path.Combine(_dataDir, "Managed");

    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, ITextureInfo>> _texturePaths = new();
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

        foreach (var file_girlDefs in girls)
        {
            foreach (var girlDef in file_girlDefs.Value)
            {
                ExtractGirl(girlDef, collectionData);
            }
        }

        // int i = 1;
        // foreach (var file_texturePath_slicedSprites in _file_texturePath_slicedSprites)
        // {
        //     foreach (var texturePath_slicedSprites in file_texturePath_slicedSprites.Value)
        //     {
        //         var textureAsset = _extractor.GetAsset(file_texturePath_slicedSprites.Key, texturePath_slicedSprites.Key);

        //         if (textureAsset is Texture2D asTexture2d)
        //         {
        //             foreach (var slicedSprite in texturePath_slicedSprites.Value)
        //             {
        //                 ModInterface.AddDataMod(new PhotoDataMod(new RelativeId(-1, i++), Hp2BaseMod.Utility.InsertStyle.replace)
        //                 {
        //                     BigPhotoCensored = new SpriteInfoBytes()
        //                     {
        //                         Data = asTexture2d.image_data.GetData(),
        //                         Format = UnityEngine.TextureFormat.DXT5,
        //                         Width = asTexture2d.m_Width,
        //                         Height = asTexture2d.m_Height,
        //                         Vertices = slicedSprite.vectors,
        //                         Indices = slicedSprite.triangles,
        //                         Uvs = slicedSprite.uvs
        //                     }
        //                 });
        //             }
        //         }
        //     }
        // }
    }

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

        if (girlDef.TryGetValue("firstName", out string firstName))
        {
            ModInterface.Log.LogInfo(firstName);
        }

        if (girlDef.TryGetValue("photosSpriteCollectionName", out string photosSpriteCollectionName)
            && collectionData.TryGetValue(photosSpriteCollectionName, out var photoCollection)
            && TryGetSpriteLookup(photoCollection, out var photoLookup, out var photoTextureInfo)
            && TryGetSpriteLookup(_thumbnailCollection, out var thumbLookup, out var thumbTextureInfo)
            && girlDef.TryGetValue("photos", out List<object> photos))
        {
            foreach (var girlPhoto in photos.OfType<OrderedDictionary>())
            {
                var photoMod = new PhotoDataMod(new RelativeId(Plugin.ModId, Plugin.PhotoModCount++), Hp2BaseMod.Utility.InsertStyle.replace);

                if (girlPhoto.TryGetValue("fullSpriteName", out List<object> fullSpriteName)
                    && girlPhoto.TryGetValue("thumbnailSpriteName", out List<object> thumbnailSpriteName))
                {
                    int nameIndex = -1;

                    ModInterface.Log.LogInfo($"fullCount: {fullSpriteName.Count}, thumbCount: {thumbnailSpriteName.Count}");

                    var thumbnailInfo = new IGameDefinitionInfo<UnityEngine.Sprite>[thumbnailSpriteName.Count];

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

                    foreach (var name in fullSpriteName.OfType<string>())
                    {
                        nameIndex++;

                        if (string.IsNullOrEmpty(name)
                            || !photoLookup.TryGetValue(name, out var photoDef))
                        {
                            continue;
                        }

                        if (TryMakeSpriteInfo(photoDef, photoTextureInfo, out var photoInfo))
                        {
                            if (fullSpriteName.Count == 1)
                            {
                                photoMod.BigPhotoCensored = photoInfo;

                                photoMod.ThumbnailCensored = thumbnailInfo[0];
                                photoMod.ThumbnailUncensored = thumbnailInfo[0];
                                photoMod.ThumbnailWet = thumbnailInfo[0];
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
                                photoMod.ThumbnailWet = thumbnailInfo[0];
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

    private bool TryMakeSpriteInfo(OrderedDictionary spriteDef, ITextureInfo textureInfo, out IGameDefinitionInfo<UnityEngine.Sprite> info)
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

            var positionVecs = positions.OfType<OrderedDictionary>().Select(o => TryGetVector2(o, out float x, out float y)
                    ? new UnityEngine.Vector2(x, sizeY + y)
                    : UnityEngine.Vector2.zero).ToArray();

            var triangles = indices.OfType<int>().Select(x => (ushort)x).ToArray();

            //some have negative verts, so shift min to zero
            var minPos = new UnityEngine.Vector2(positionVecs.Min(x => x.x), positionVecs.Min(x => x.y));

            info = new SpriteInfoTexture(textureInfo,
                new UnityEngine.Rect(sizeX - centerX, sizeY - centerY, sizeX, sizeY),
                positionVecs.Select(x => x - minPos).ToArray(),
                uvVecs,
                indices.OfType<int>().Select(x => (ushort)x).ToArray());

            return true;
        }

        info = null;
        return false;
    }

    private bool TryGetSpriteLookup((SerializedFile file, OrderedDictionary def) spriteCollection,
        out Dictionary<string, OrderedDictionary> spriteLookup,
        out ITextureInfo textureInfo)
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

            if (!_texturePaths.TryGetValue(spriteCollection.file, out var textures))
            {
                textures = new();
                _texturePaths[spriteCollection.file] = textures;
            }
            else if (textures.TryGetValue(texturePath, out textureInfo))
            {
                return true;
            }

            var textureAsset = _extractor.GetAsset(spriteCollection.file, texturePath);
            if (textureAsset is Texture2D asTexture2d)
            {
                textureInfo = new TextureInfoRaw(asTexture2d.m_Width,
                    asTexture2d.m_Height,
                    asTexture2d.image_data.GetData(),
                    (UnityEngine.TextureFormat)asTexture2d.m_TextureFormat,
                    UnityEngine.FilterMode.Bilinear,
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
}

// namespace HuniepopUltimate;
// public class HpExtraction : BaseExtraction
// {
//     public static readonly string _dataDir = "HuniePop_Data";
//     public static readonly string _assemblyDir = Path.Combine(_dataDir, "Managed");
//     public static readonly string TexturesPath = "Textures";
//     public static readonly string AudioPath = "Audio";
//     public static readonly string AtlasTexturesDir = "AtlasTextures";
//     public static readonly string GirlsPath = "Girls";
//     public static readonly string AudioSetsPath = "AudioSets";
//     public static readonly string TranslationsPath = "Translations";

//     private IReadOnlyDictionary<string, int> _monthLookup = new Dictionary<string, int>() {
//         {"jan", 1},
//         {"feb", 2},
//         {"mar", 3},
//         {"apr", 4},
//         {"may", 5},
//         {"jun", 6},
//         {"jul", 7},
//         {"aug", 8},
//         {"sep", 9},
//         {"oct", 10},
//         {"nov", 11},
//         {"dec", 12}
//     };

//     private IReadOnlyDictionary<string, int> _cupSizeLookup = new Dictionary<string, int>() {
//         {"a", 0},
//         {"b", 1},
//         {"c", 2},
//         {"d", 3},
//         {"dd", 4},
//         {"f", 5}
//     };

//     private IReadOnlyDictionary<int, List<int>> _voiceProfile;

//     private IReadOnlyDictionary<int, string> _underwearNames;
//     private IReadOnlyDictionary<int, int> _extraPartLayerLookup;
//     private IReadOnlyDictionary<int, int> _expressionLookup;
//     private int _specialOnId;
//     private int _specialOffId;
//     private int _eyesHalfBlinkId;
//     private int _eyesFullBlinkId;

//     private Translation _en;
//     private Dictionary<SerializedFile, HashSet<UnityAssetPath>> _sprites = new();
//     private Dictionary<SerializedFile, HashSet<UnityAssetPath>> _audio = new();
//     private Dictionary<int, IData<int>> _audioSets = new();
//     private Dictionary<int, IData<int>> _girls = new();
//     private Dictionary<int, IData<int>> _atlasTextures = new();

//     // private Extractor _extractor;
//     // private HtCatalog _catalog;
//     // private ISerializerManager<string, int> _serializer;
//     // private ISourceManager<string, int> _sourceManager;
//     // private int _sourceId;
//     // private string _exportPath;
//     // private IProgress _progress;

//     public HpExtraction(string dir,
//         HtCatalog catalog,
//         ISerializerManager<string, int> serializer,
//         int sourceId,
//         ISourceManager<string, int> sourceManager)
//         : base(Path.Combine(dir, _dataDir),
//             Path.Combine(dir, _assemblyDir),
//             catalog,
//             serializer,
//             sourceId,
//             sourceManager)
//     {
//         _en = new();
//         _en.Locale = "en";
//         _en.AddMessage(_sourceId.ToString(), "HuniePop");

//         _specialOffId = GetId("Off");
//         _en.AddMessage(_specialOffId.ToString(), "Off");
//         _specialOnId = GetId("On");
//         _en.AddMessage(_specialOnId.ToString(), "On");

//         _eyesHalfBlinkId = GetId("eyeHalfClosed");
//         _en.AddMessage(_eyesHalfBlinkId.ToString(), "Half-Closed");
//         _eyesFullBlinkId = GetId("eyeClosed");
//         _en.AddMessage(_eyesFullBlinkId.ToString(), "Closed");

//         _underwearNames = new Dictionary<int, string>(){
//             {_catalog.Models.Character.Tiffany, "Underwear"},
//             {_catalog.Models.Character.Aiko, "Naughty Teacher"},
//             {_catalog.Models.Character.Kyanna, "Underwear"},
//             {_catalog.Models.Character.Audrey, "Underwear"},
//             {_catalog.Models.Character.Lola, "Underwear"},
//             {_catalog.Models.Character.Nikki, "Unprepared"},
//             {_catalog.Models.Character.Jessie, "Underwear"},
//             {_catalog.Models.Character.Beli, "Underwear"},
//             {_catalog.Models.Character.Kyu, "Underwear"},
//             {_catalog.Models.Character.Momo, "Underwear"},
//             {_catalog.Models.Character.Celeste, "Human Garments"},
//             {_catalog.Models.Character.Venus, "Underwear"}
//         };

//         _extraPartLayerLookup = new Dictionary<int, int>(){
//             {0, _catalog.Layers.BackHair},
//             {1, _catalog.Layers.Body},
//             {2, _catalog.Layers.Bra},
//             {3, _catalog.Layers.Panties},
//             {4, _catalog.Layers.Footwear},
//             {5, _catalog.Layers.Outfit},
//             {6, _catalog.Layers.Head},
//             {7, _catalog.Layers.Face},
//             {8, _catalog.Layers.Mouth},
//             {9, _catalog.Layers.Eyes},
//             {10, _catalog.Layers.FrontHair},
//             {11, _catalog.Layers.OverHairEyebrows},
//             {12, _catalog.Layers.Front},
//             {13, _catalog.Layers.Front}
//         };

//         _expressionLookup = new Dictionary<int, int>(){
//             {0, GetId("expressionId_Happy")},
//             {1, GetId("expressionId_Sad")},
//             {2, GetId("expressionId_Angry")},
//             {3, GetId("expressionId_Excited")},
//             {4, GetId("expressionId_Shy")},
//             {5, GetId("expressionId_Confused")},
//             {6, GetId("expressionId_Horny")},
//             {7, GetId("expressionId_Sick")},
//         };

//         _en.AddMessage(_expressionLookup[0].ToString(), "Happy");
//         _en.AddMessage(_expressionLookup[1].ToString(), "Sad");
//         _en.AddMessage(_expressionLookup[2].ToString(), "Angry");
//         _en.AddMessage(_expressionLookup[3].ToString(), "Excited");
//         _en.AddMessage(_expressionLookup[4].ToString(), "Shy");
//         _en.AddMessage(_expressionLookup[5].ToString(), "Confused");
//         _en.AddMessage(_expressionLookup[6].ToString(), "Horny");
//         _en.AddMessage(_expressionLookup[7].ToString(), "Sick");

//         _voiceProfile = new Dictionary<int, List<int>>(){
//             {_catalog.Models.Character.Celeste, new List<int>(){
//                 214,
//                 1763
//             }},
//             {_catalog.Models.Character.Momo, new List<int>(){
//                 1397,
//                 1588,
//                 1308
//             }},
//             {_catalog.Models.Character.Kyu, new List<int>(){
//                 460,
//                 2769,
//                 878,
//             }},
//             {_catalog.Models.Character.Venus, new List<int>(){
//                 1315,
//                 433,
//                 991
//             }},
//             {_catalog.Models.Character.Aiko, new List<int>(){
//                 1518
//             }}
//         };
//     }

//     public void ExportAssets(string directory, IProgress progress)
//     {
//         var unitAlotment = 100f / (_audio.Sum(x => x.Value.Count) + _sprites.Sum(x => x.Value.Count) + _girls.Count + _atlasTextures.Count + _audioSets.Count + 1);

//         var audioDir = Path.Combine(directory, AudioPath);
//         ExportUnityAssets(audioDir, _audio, (s) => progress.Report(unitAlotment, $"Exported audio {s}"));

//         var spriteDir = Path.Combine(directory, TexturesPath);
//         ExportUnityAssets(spriteDir, _sprites, (s) => progress.Report(unitAlotment, $"Exported texture {s}"));

//         var audioSetsDir = Path.Combine(directory, AudioSetsPath);
//         Directory.CreateDirectory(audioSetsDir);
//         foreach (var audioSet in _audioSets)
//         {
//             ExportData(audioSetsDir, audioSet.Key, audioSet.Value);
//             progress.Report(unitAlotment, $"Exported audio set {audioSet.Key}");
//         }

//         var atlasPath = Path.Combine(directory, AtlasTexturesDir);
//         Directory.CreateDirectory(atlasPath);
//         foreach (var atlas in _atlasTextures)
//         {
//             ExportData(atlasPath, atlas.Key, atlas.Value);
//             progress.Report(unitAlotment, $"Exported atlas texture {atlas.Key}");
//         }

//         var girlsPath = Path.Combine(directory, GirlsPath);
//         Directory.CreateDirectory(girlsPath);
//         foreach (var girl in _girls)
//         {
//             ExportData(girlsPath, girl.Key, girl.Value);
//             progress.Report(unitAlotment, $"Exported girl {girl.Key}");
//         }

//         var translationPath = Path.Combine(directory, TranslationsPath);
//         Directory.CreateDirectory(translationPath);
//         ResourceSaver.Save(_en, Path.Combine(translationPath, "tr_en.res"));
//     }

//     public void Extract(Sourceage.IO.Progress progress)
//     {
//         var girls = _extractor.ExtractMonobehaviors("GirlDefinition");

//         var unitAlotment = 100f / girls.Sum(x => x.Value.Count);

//         var collectionData = new Dictionary<string, (SerializedFile, OrderedDictionary)>();

//         foreach (var file_behviorList in _extractor.ExtractMonobehaviors("tk2dSpriteCollectionData"))
//         {
//             foreach (var behavior in file_behviorList.Value)
//             {
//                 if (behavior.TryGetValue("spriteCollectionName", out string name))
//                 {
//                     collectionData[name] = (file_behviorList.Key, behavior);
//                 }
//             }
//         }

//         foreach (var file_girlDef in girls)
//         {
//             foreach (var girlDef in file_girlDef.Value)
//             {
//                 if (girlDef.TryGetValue("id", out int nativeId)
//                     && TryLocalizeGirlId(nativeId, out var id)
//                     && TryExtractGirl(id, girlDef, collectionData, file_girlDef.Key, out var extracted))
//                 {
//                     _girls[id] = extracted;

//                     progress.Report(unitAlotment, $"Processed girl {id}");
//                 }
//                 else
//                 {
//                     progress.Report(unitAlotment, $"Failed to process a girl definition");
//                 }
//             }
//         }
//     }

//     private bool TryLocalizeGirlId(int nativeId, out int localizedId)
//     {
//         switch (nativeId)
//         {
//             case 1:
//                 localizedId = _catalog.Models.Character.Tiffany;
//                 return true;
//             case 2:
//                 localizedId = _catalog.Models.Character.Aiko;
//                 return true;
//             case 3:
//                 localizedId = _catalog.Models.Character.Kyanna;
//                 return true;
//             case 4:
//                 localizedId = _catalog.Models.Character.Audrey;
//                 return true;
//             case 5:
//                 localizedId = _catalog.Models.Character.Lola;
//                 return true;
//             case 6:
//                 localizedId = _catalog.Models.Character.Nikki;
//                 return true;
//             case 7:
//                 localizedId = _catalog.Models.Character.Jessie;
//                 return true;
//             case 8:
//                 localizedId = _catalog.Models.Character.Beli;
//                 return true;
//             case 9:
//                 localizedId = _catalog.Models.Character.Kyu;
//                 return true;
//             case 10:
//                 localizedId = _catalog.Models.Character.Momo;
//                 return true;
//             case 11:
//                 localizedId = _catalog.Models.Character.Celeste;
//                 return true;
//             case 12:
//                 localizedId = _catalog.Models.Character.Venus;
//                 return true;
//         }
//         localizedId = 0;
//         return false;
//     }

//     private bool TryExtractGirl(int girlId,
//         OrderedDictionary girlDef,
//         Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData,
//         SerializedFile sourceFile,
//         out IData<int> girl)
//     {
//         if (!_audio.TryGetValue(sourceFile, out var audio))
//         {
//             audio = new();
//             _audio[sourceFile] = audio;
//         }

//         var fileId = GetUnityFileId(sourceFile);

//         girl = new Data<int>();
//         girl.SetComponent(_catalog.Models.TypeId, _catalog.Serializers.Int, _catalog.Models.Character.ModelTypeId);

//         //venus's first name is actually her last
//         if (girlId == _catalog.Models.Character.Venus)
//         {
//             var id = GetGirlPropertyId(girlId, "FirstName");
//             _en.AddMessage(id.ToString(), "Theiatena");

//             girl.SetComponent(_catalog.Models.Character.FirstName,
//                 _catalog.Serializers.Int,
//                 id);
//         }
//         else if (girlDef.TryGetValue("firstName", out string firstName))
//         {
//             var id = GetGirlPropertyId(girlId, "FirstName");
//             _en.AddMessage(id.ToString(), firstName);

//             girl.SetComponent(_catalog.Models.Character.FirstName,
//                 _catalog.Serializers.Int,
//                 id);
//         }

//         HandleDetails(girlDef, girl, girlId);

//         if (TryExtractBody(girlId, girlDef, collectionData, out var body))
//         {
//             girl.SetComponent(_catalog.Models.Character.Body,
//                 _catalog.Serializers.Data,
//                 body);
//         }

//         if (girlDef.TryGetValue("photosSpriteCollectionName", out string photosSpriteCollectionName)
//             && collectionData.TryGetValue(photosSpriteCollectionName, out var collection)
//             && girlDef.TryGetValue("photos", out List<object> photos))
//         {
//             foreach (var girlPhoto in photos.OfType<OrderedDictionary>())
//             {
//                 //test, just extract the one
//                 if (TryExtractPhoto(girlPhoto, collection, out var photoPart))
//                 {
//                     girl.SetComponent(_catalog.Models.Character.Promo,
//                         _catalog.Serializers.Data,
//                         photoPart);
//                     break;
//                 }
//             }
//         }

//         if (!girlDef.TryGetValue("baseVoiceVolume", out float baseVoiceVolume))
//         {
//             baseVoiceVolume = 0.8f;
//         }

//         if (_voiceProfile.TryGetValue(girlId, out var voiceProfile))
//         {
//             var pitch = 1f;
//             var ids = new List<object>();
//             foreach (var audioPathId in voiceProfile)
//             {
//                 var unityId = new UnityAssetPath() { FileId = 0, PathId = audioPathId };
//                 audio.Add(unityId);
//                 ids.Add(GetUnityPathAssetId(fileId, unityId));
//             }

//             var id = GetGirlPropertyId(girlId, "voiceProfile");

//             var audioSet = new Data<int>();
//             audioSet.SetComponent(_catalog.Models.AudioSet.Volume, _catalog.Serializers.Float, baseVoiceVolume);
//             audioSet.SetComponent(_catalog.Models.AudioSet.Pitch, _catalog.Serializers.Float, pitch);
//             audioSet.SetComponent(_catalog.Models.AudioSet.AudioIds, _catalog.Serializers.List(_catalog.Serializers.Int), ids);
//             audioSet.SetComponent(_catalog.Models.AudioSet.QueueId, _catalog.Serializers.Int, _catalog.Sound.VoiceQueue);

//             _audioSets[id] = audioSet;

//             girl.SetComponent(_catalog.Models.Character.VoiceProfile,
//                 _catalog.Serializers.Int,
//                 id);
//         }

//         return true;
//     }

//     private bool TryGetSpriteLookup((SerializedFile, OrderedDictionary) spriteCollection,
//         out Dictionary<string, OrderedDictionary> spriteLookup,
//         out int collectionId)
//     {
//         if (spriteCollection.Item2.TryGetValue("spriteDefinitions", out List<object> spriteDefinitions)
//             && spriteCollection.Item2.TryGetValue("textures", out List<object> textures))
//         {
//             //there should only be the one texture for these atlases
//             //idk how I want to handle multiple, are they refernced somewhere? which texture a sprite uses?, I should, but for now just use the one
//             if (!(textures.Count == 1
//                     && textures.First() is OrderedDictionary idData
//                     && UnityAssetPath.TryExtract(idData, out var texturePath)))
//             {
//                 spriteLookup = null;
//                 collectionId = default;
//                 return false;
//             }

//             if (!_sprites.TryGetValue(spriteCollection.Item1, out var sprites))
//             {
//                 sprites = new();
//                 _sprites[spriteCollection.Item1] = sprites;
//             }
//             var fileId = GetUnityFileId(spriteCollection.Item1);

//             sprites.Add(texturePath);

//             collectionId = GetUnityPathAssetId(fileId, texturePath);

//             spriteLookup = spriteDefinitions
//                 .OfType<OrderedDictionary>()
//                 .Where(x => x != null && x.Contains("name") && x["name"] is string)
//                 //there are duplicate keys, so 
//                 .DistinctBy(x => (string)x["name"])
//                 .ToDictionary(x => x["name"] as string, x => x);
//             return true;
//         }

//         spriteLookup = null;
//         collectionId = default;
//         return false;
//     }

//     private bool TryExtractPhoto(OrderedDictionary girlPhoto,
//         (SerializedFile, OrderedDictionary) spriteCollection,
//         out IData<int> photoPart)
//     {
//         if (TryGetSpriteLookup(spriteCollection, out var spriteLookup, out var atlasId)
//             && girlPhoto.TryGetValue("fullSpriteName", out List<object> fullSpriteName))
//         {
//             //for now just get the first one, but I suspect I'll have to stitch a bunch together...
//             var spriteName = fullSpriteName.OfType<string>().FirstOrDefault();
//             if (spriteName != null
//                 && spriteLookup.TryGetValue(spriteName, out var spriteDef))
//             {
//                 if (spriteDef.TryGetValue("uvs", out List<object> uvs)
//                     && spriteDef.TryGetValue("positions", out List<object> positions))
//                 {
//                     var vertexes = new List<object>();
//                     var enumerator = uvs.OfType<OrderedDictionary>().Zip(positions.OfType<OrderedDictionary>()).GetEnumerator();

//                     //I actually want the vertexes in 0,1,3,2 order to make a closed polygon, so take them 4 at a time
//                     while (enumerator.MoveNext())
//                     {
//                         vertexes.Add(ExtractVertex(enumerator.Current.First, enumerator.Current.Second));

//                         if (!enumerator.MoveNext()) { throw new Exception(); }
//                         vertexes.Add(ExtractVertex(enumerator.Current.First, enumerator.Current.Second));

//                         if (!enumerator.MoveNext()) { throw new Exception(); }
//                         var hold = ExtractVertex(enumerator.Current.First, enumerator.Current.Second);

//                         if (!enumerator.MoveNext()) { throw new Exception(); }
//                         vertexes.Add(ExtractVertex(enumerator.Current.First, enumerator.Current.Second));

//                         vertexes.Add(hold);
//                     }

//                     photoPart = _catalog.Models.PolygonPart.Create(-600, -450, atlasId, _catalog.Layers.Back, null, vertexes);
//                     return true;
//                 }
//             }
//         }

//         photoPart = null;
//         return false;
//     }

//     private IData<int> ExtractVertex(OrderedDictionary uv, OrderedDictionary pos)
//     {
//         if (uv.TryGetValue("x", out float u)
//             && uv.TryGetValue("y", out float v)
//             && pos.TryGetValue("x", out float x)
//             && pos.TryGetValue("y", out float y))
//         {
//             var vertex = new Data<int>();
//             vertex.SetComponent(_catalog.Models.Vertex.x, _catalog.Serializers.Float, x);
//             vertex.SetComponent(_catalog.Models.Vertex.y, _catalog.Serializers.Float, -y);
//             vertex.SetComponent(_catalog.Models.Vertex.u, _catalog.Serializers.Float, u);
//             vertex.SetComponent(_catalog.Models.Vertex.v, _catalog.Serializers.Float, 1 - v);
//             return vertex;
//         }

//         throw new Exception();
//     }

//     private bool TryExtractBody(int girlId,
//         OrderedDictionary girlDef,
//         Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData,
//         out IData<int> body)
//     {
//         if (girlDef.TryGetValue("spriteCollectionName", out string spriteCollectionName)
//             && collectionData.TryGetValue(spriteCollectionName, out var spriteCollection)
//             && TryGetSpriteLookup(spriteCollection, out var spriteLookup, out var collectionId))
//         {
//             var bodyParts = new Dictionary<int, object>();
//             if (TryExtractGirlPieceArtFrom(girlDef, "headPiece", _catalog.Layers.Head, spriteLookup, collectionId, out var headPart))
//             {
//                 bodyParts[_catalog.Layers.Head] = headPart;
//             }
//             if (TryExtractGirlPieceArtFrom(girlDef, "bodyPiece", _catalog.Layers.Body, spriteLookup, collectionId, out var bodyPart))
//             {
//                 bodyParts[_catalog.Layers.Body] = bodyPart;
//             }

//             var outfits = new Dictionary<int, object>();
//             var hairStyles = new Dictionary<int, object>();
//             var eyeParts = new Dictionary<int, object>();
//             var browParts = new Dictionary<int, object>();
//             var mouthParts = new Dictionary<int, object>();
//             var extras = new Dictionary<int, object>();

//             //parts
//             if (girlDef.TryGetValue("pieces", out List<object> pieces))
//             {
//                 var piecesLookup = pieces.OfType<OrderedDictionary>().ToArray();
//                 HandleStyleListFrom(girlDef, "outfits", girlId, spriteLookup, piecesLookup, collectionId, outfits);
//                 HandleStyleListFrom(girlDef, "hairstyles", girlId, spriteLookup, piecesLookup, collectionId, hairStyles);

//                 //all the other parts are searched for, there's no registry :(
//                 int extraCount = 0;
//                 foreach (var piece in piecesLookup)
//                 {
//                     if (piece.TryGetValue("type", out int typeId))
//                     {
//                         switch (typeId)
//                         {
//                             case 0://expression
//                                 if (TryExtractExpression(piece, spriteLookup, collectionId, out var brows, out var eyes, out var mouth))
//                                 {
//                                     if (piece.TryGetValue("expressionType", out int expressionType))
//                                     {
//                                         expressionType = _expressionLookup[expressionType];
//                                     }
//                                     else
//                                     {
//                                         expressionType = 0;
//                                     }

//                                     browParts[expressionType] = brows;
//                                     eyeParts[expressionType] = eyes;
//                                     mouthParts[expressionType] = mouth;
//                                 }
//                                 break;
//                             //1 and 3 are outfit and hairstyle, already handled above
//                             case 3://foot
//                                 //just ignore for now, they're not seen
//                                 break;
//                             case 4://phomene
//                                 break;
//                             case 5://extra
//                                 if (TryExtractGirlPiece(piece, spriteLookup, collectionId, out var extra))
//                                 {
//                                     var id = GetGirlPropertyId(girlId, $"extra{extraCount++}");
//                                     if (piece.TryGetValue("name", out string name))
//                                     {
//                                         _en.AddMessage(id.ToString(), name);
//                                     }
//                                     extras[id] = extra;
//                                 }
//                                 break;
//                         }
//                     }
//                 }
//             }

//             //underwear
//             var underwearParts = new Dictionary<int, object>();
//             if (TryExtractGirlPieceArtFrom(girlDef, "braPiece", _catalog.Layers.Bra, spriteLookup, collectionId, out var braPart))
//             {
//                 underwearParts[_catalog.Layers.Bra] = braPart;
//             }
//             if (TryExtractGirlPieceArtFrom(girlDef, "pantiesPiece", _catalog.Layers.Bra, spriteLookup, collectionId, out var pantiesPart))
//             {
//                 underwearParts[_catalog.Layers.Panties] = pantiesPart;
//             }

//             var underwearId = GetGirlPropertyId(girlId, $"outfit_underwear");
//             if (!_underwearNames.TryGetValue(girlId, out var underwearName))
//             {
//                 underwearName = "Underwear";
//             }
//             _en.AddMessage(underwearId.ToString(), underwearName);
//             outfits[underwearId] = HandlePartDict(underwearParts);

//             var toplessId = GetGirlPropertyId(girlId, $"outfit_topless");
//             _en.AddMessage(toplessId.ToString(), "Topless");
//             outfits[toplessId] = pantiesPart;

//             //eyelids
//             if (TryExtractGirlPieceArtFrom(girlDef, "blinkHalfPiece", _catalog.Layers.Eyelids, spriteLookup, collectionId, out var blinkHalfPart))
//             {
//                 eyeParts[_eyesHalfBlinkId] = blinkHalfPart;
//             }
//             if (TryExtractGirlPieceArtFrom(girlDef, "blinkFullPiece", _catalog.Layers.Eyelids, spriteLookup, collectionId, out var blinkFullPart))
//             {
//                 eyeParts[_eyesFullBlinkId] = blinkFullPart;
//             }

//             //I have no idea why jessie's 'Celebrity' fronthair is positioned differently
//             //likely there's a value in its spriteDef that shifts it that I'm not considering,
//             //but idk what that is, so, just manually correct it here
//             if (girlId == _catalog.Models.Character.Jessie)
//             {
//                 var id = GetGirlPropertyId(girlId, "hairstyles3");
//                 if (hairStyles.TryGetValue(id, out var hairstyle4Obj)
//                     && hairstyle4Obj is IData<int> hairstyle4
//                     && hairstyle4.TryGetComponent<Dictionary<int, object>>(_catalog.Models.CompositePart.Parts, out var parts)
//                     && parts.TryGetValue(_catalog.Layers.FrontHair, out var frontHairObj)
//                     && frontHairObj is IData<int> frontHair
//                     && frontHair.TryGetComponent<int>(_catalog.Models.TexturePart.X, out var x)
//                     && frontHair.TryGetComponent<int>(_catalog.Models.TexturePart.Y, out var y))
//                 {
//                     frontHair.SetComponent(_catalog.Models.TexturePart.X, _catalog.Serializers.Int, x + 82);
//                     frontHair.SetComponent(_catalog.Models.TexturePart.Y, _catalog.Serializers.Int, y + 25);
//                 }
//             }

//             //and while we're at it lets make kyu's disguise a hairstyle instead of an extra
//             if (girlId == _catalog.Models.Character.Kyu)
//             {
//                 var disguiseFrontHairId = GetGirlPropertyId(girlId, "extra1");
//                 var disguiseBackHairId = GetGirlPropertyId(girlId, "extra2");

//                 if (extras.TryGetValue(disguiseFrontHairId, out var frontHair)
//                     && extras.TryGetValue(disguiseBackHairId, out var backHair))
//                 {
//                     extras.Remove(disguiseFrontHairId);
//                     extras.Remove(disguiseBackHairId);

//                     var disguiseHairstyleId = GetGirlPropertyId(girlId, "hairstyleDisguise");
//                     _en.AddMessage(disguiseHairstyleId.ToString(), "Disguise");

//                     hairStyles[disguiseHairstyleId] = _catalog.Models.CompositePart.Create(new Dictionary<int, object>(){
//                         {_catalog.Layers.FrontHair, frontHair},
//                         {_catalog.Layers.BackHair, backHair}
//                     });
//                 }
//             }

//             //lola's tights aren't going to be visible
//             if (girlId == _catalog.Models.Character.Lola)
//             {
//                 var tightsId = GetGirlPropertyId(girlId, "extra0");
//                 extras.Remove(tightsId);
//             }

//             //neither are tiffany's
//             if (girlId == _catalog.Models.Character.Tiffany)
//             {
//                 var tightsId = GetGirlPropertyId(girlId, "extra0");
//                 extras.Remove(tightsId);
//             }

//             //and kyanna's earings are on her fronthair layer which screws with my non-ordered layers, so move them back to the eyes
//             if (girlId == _catalog.Models.Character.Kyanna)
//             {
//                 var earRingsId = GetGirlPropertyId(girlId, "extra0");

//                 if (extras.TryGetValue(earRingsId, out var earRingsObj)
//                     && earRingsObj is IData<int> earRings)
//                 {
//                     earRings.SetComponent(_catalog.Models.TexturePart.Layer, _catalog.Serializers.Int, _catalog.Layers.Eyes);
//                 }
//             }

//             var huniepopBody = new Dictionary<int, object>();

//             if (bodyParts.Any())
//             {
//                 huniepopBody[_catalog.Layers.Body] = HandlePartDict(bodyParts);
//             }

//             if (outfits.Any())
//             {
//                 huniepopBody[_catalog.Layers.Outfit] = _catalog.Models.DynamicPart.Create(outfits);
//             }

//             if (hairStyles.Any())
//             {
//                 huniepopBody[_catalog.Layers.Hairstyle] = _catalog.Models.DynamicPart.Create(hairStyles);
//             }

//             if (browParts.Any())
//             {
//                 huniepopBody[_catalog.Layers.OverHairEyebrows] = _catalog.Models.DynamicPart.Create(browParts);
//             }

//             if (eyeParts.Any())
//             {
//                 huniepopBody[_catalog.Layers.Eyes] = _catalog.Models.DynamicPart.Create(eyeParts);
//             }

//             if (mouthParts.Any())
//             {
//                 huniepopBody[_catalog.Layers.Mouth] = _catalog.Models.DynamicPart.Create(mouthParts);
//             }

//             foreach (var extra in extras)
//             {
//                 huniepopBody[extra.Key] = _catalog.Models.DynamicPart.Create(new Dictionary<int, object>(){
//                     {_specialOnId, extra.Value},
//                     {_specialOffId, _catalog.Models.DummyPart.Create()}
//                 });
//             }

//             body = _catalog.Models.DynamicPart.Create(new(){
//                 {_sourceId, _catalog.Models.CompositePart.Create(huniepopBody)}
//             });
//             return true;
//         }

//         body = null;
//         return false;
//     }

//     private void HandleStyleListFrom(OrderedDictionary source,
//         string entryName,
//         int girlId,
//         Dictionary<string, OrderedDictionary> spriteLookup,
//         OrderedDictionary[] piecesLookop,
//         int collectionId,
//         Dictionary<int, object> partsById)
//     {
//         if (source.TryGetValue(entryName, out List<object> styles))
//         {
//             int i = 0;
//             foreach (var style in styles.OfType<OrderedDictionary>())
//             {
//                 var id = GetGirlPropertyId(girlId, $"{entryName}{i++}");
//                 if (TryExtractStyle(style, id, spriteLookup, piecesLookop, collectionId, out var extracted))
//                 {
//                     partsById[id] = extracted;
//                 }
//             }
//         }
//     }

//     private bool TryExtractStyle(OrderedDictionary outfit,
//         int id,
//         Dictionary<string, OrderedDictionary> spriteLookup,
//         OrderedDictionary[] piecesLookop,
//         int collectionId,
//         out object part)
//     {
//         if (outfit.TryGetValue("styleName", out string styleName)
//             && outfit.TryGetValue("artIndex", out int artIndex))
//         {
//             _en.AddMessage(id.ToString(), styleName);

//             var piece = piecesLookop[artIndex];

//             return TryExtractGirlPiece(piece, spriteLookup, collectionId, out part);
//         }

//         part = null;
//         return false;
//     }

//     private bool TryExtractExpression(OrderedDictionary piece,
//         Dictionary<string, OrderedDictionary> spriteLookup,
//         int collectionId,
//         out object brows,
//         out object eyes,
//         out object mouth)
//     {
//         brows = null;
//         eyes = null;
//         mouth = null;

//         if (piece.TryGetValue("art", out List<object> artCollection))
//         {
//             var it = artCollection.OfType<OrderedDictionary>().GetEnumerator();
//             if (!it.MoveNext()) { return false; }
//             var eyeArt = it.Current;
//             if (!it.MoveNext()) { return false; }
//             var browArt = it.Current;
//             if (!it.MoveNext()) { return false; }
//             var mouthArt = it.Current;

//             if (!TryExtractGirlPieceArt(browArt, _catalog.Layers.OverHairEyebrows, spriteLookup, collectionId, out var browsObj))
//             {
//                 return false;
//             }
//             brows = browsObj;

//             if (!TryExtractGirlPieceArt(eyeArt, _catalog.Layers.Eyes, spriteLookup, collectionId, out var eyesObj))
//             {
//                 return false;
//             }
//             eyes = eyesObj;

//             if (!TryExtractGirlPieceArt(mouthArt, _catalog.Layers.Mouth, spriteLookup, collectionId, out var mouthObj))
//             {
//                 return false;
//             }
//             mouth = mouthObj;

//             return true;
//         }

//         return false;
//     }


//     private bool TryExtractGirlPiece(OrderedDictionary piece,
//         Dictionary<string, OrderedDictionary> spriteLookup,
//         int collectionId,
//         out object part)
//     {
//         if (piece.TryGetValue("art", out List<object> artCollection)
//             && piece.TryGetValue("type", out int typeId))
//         {
//             var parts = new Dictionary<int, object>();

//             switch (typeId)
//             {
//                 case 0://expression
//                     break;
//                 case 1://hairstyles
//                     var it = artCollection.OfType<OrderedDictionary>().GetEnumerator();

//                     if (it.MoveNext())
//                     {
//                         if (TryExtractGirlPieceArt(it.Current, _catalog.Layers.FrontHair, spriteLookup, collectionId, out var frontPart))
//                         {
//                             parts[_catalog.Layers.FrontHair] = frontPart;
//                         }

//                         if (it.MoveNext())
//                         {
//                             if (TryExtractGirlPieceArt(it.Current, _catalog.Layers.BackHair, spriteLookup, collectionId, out var backPart))
//                             {
//                                 parts[_catalog.Layers.BackHair] = backPart;
//                             }
//                         }
//                     }
//                     break;
//                 case 2://outfits
//                     var outfitPieceIndex = 0;
//                     foreach (var art in artCollection.OfType<OrderedDictionary>())
//                     {
//                         if (TryExtractGirlPieceArt(art, _catalog.Layers.Outfit, spriteLookup, collectionId, out var extractedPart))
//                         {
//                             parts[outfitPieceIndex++] = extractedPart;
//                         }
//                     }
//                     break;
//                 case 3://foot
//                     break;
//                 case 4://phomeme
//                     break;
//                 case 5://extra
//                     if (piece.TryGetValue("layer", out int layerId))
//                     {
//                         layerId = _extraPartLayerLookup[layerId];
//                     }
//                     else
//                     {
//                         layerId = _catalog.Layers.Error;
//                     }

//                     var extraPieceIndex = 0;
//                     foreach (var art in artCollection.OfType<OrderedDictionary>())
//                     {
//                         if (TryExtractGirlPieceArt(art, layerId, spriteLookup, collectionId, out var extractedPart))
//                         {
//                             parts[extraPieceIndex++] = extractedPart;
//                         }
//                     }
//                     break;
//             }

//             if (parts.Any())
//             {
//                 part = HandlePartDict(parts);
//                 return true;
//             }
//         }

//         part = null;
//         return false;
//     }

//     private bool TryExtractGirlPieceArtFrom(OrderedDictionary source,
//                 string name,
//                 int layerId,
//                 Dictionary<string, OrderedDictionary> spriteLookup,
//                 int atlasId,
//                 out IData<int> part)
//     {
//         if (source.TryGetValue(name, out OrderedDictionary girlArtPiece))
//         {
//             return TryExtractGirlPieceArt(girlArtPiece, layerId, spriteLookup, atlasId, out part);
//         }

//         part = null;
//         return false;
//     }

//     private bool TryExtractSprite(OrderedDictionary spriteDef, int partId, int atlasId, out bool isFlipped)
//     {
//         if (spriteDef.TryGetValue("uvs", out List<object> uvs))
//         {
//             //simple sprites with reasonable uvs, wow wow wow
//             if (uvs.Count == 4)
//             {
//                 if (uvs[2] is OrderedDictionary upperleft
//                     && uvs[1] is OrderedDictionary lowerRight
//                     && upperleft.TryGetValue("x", out float upperleftX)
//                     && upperleft.TryGetValue("y", out float upperleftY)
//                     && lowerRight.TryGetValue("x", out float lowerRightX)
//                     && lowerRight.TryGetValue("y", out float lowerRightY)
//                     && spriteDef["flipped"] is int flipped)
//                 {
//                     isFlipped = flipped == 1;

//                     //correct y direction to match godot
//                     upperleftY = 1 - upperleftY;
//                     lowerRightY = 1 - lowerRightY;

//                     //transpose
//                     if (isFlipped)
//                     {
//                         (lowerRightY, upperleftY) = (upperleftY, lowerRightY);
//                         (lowerRightX, upperleftX) = (upperleftX, lowerRightX);
//                     }

//                     var atlasTexture = new Data<int>();
//                     atlasTexture.SetComponent(_catalog.Models.Atlas.upLeftX, _catalog.Serializers.Float, upperleftX);
//                     atlasTexture.SetComponent(_catalog.Models.Atlas.upLeftY, _catalog.Serializers.Float, upperleftY);
//                     atlasTexture.SetComponent(_catalog.Models.Atlas.lowRightX, _catalog.Serializers.Float, lowerRightX);
//                     atlasTexture.SetComponent(_catalog.Models.Atlas.lowRightY, _catalog.Serializers.Float, lowerRightY);
//                     atlasTexture.SetComponent(_catalog.Models.Atlas.atlasId, _catalog.Serializers.Int, atlasId);
//                     _atlasTextures[partId] = atlasTexture;

//                     return true;
//                 }
//             }
//         }

//         isFlipped = default;
//         return false;
//     }

//     private bool TryExtractGirlPieceArt(OrderedDictionary girlArtPiece,
//         int layerId,
//         Dictionary<string, OrderedDictionary> spriteLookup,
//         int atlasId,
//         out IData<int> part)
//     {
//         if (girlArtPiece.TryGetValue("spriteName", out string spriteName)
//             && !string.IsNullOrWhiteSpace(spriteName)
//             && girlArtPiece.TryGetValue("x", out int x)
//             && girlArtPiece.TryGetValue("y", out int y)
//             && spriteLookup.TryGetValue(spriteName, out var spriteDef))
//         {
//             var partId = GetId($"atlas{atlasId}_{spriteName}");

//             if (TryExtractSprite(spriteDef, partId, atlasId, out var isFlipped))
//             {
//                 //shift center to hips
//                 x -= 930;
//                 y -= 800;

//                 part = _catalog.Models.TexturePart.Create(x, y, partId, layerId);

//                 //for now add this to the part until transpose support in atlas texture is added to godot
//                 if (isFlipped)
//                 {
//                     part.SetComponent(_catalog.Models.TexturePart.Rotation, _catalog.Serializers.Float, -Mathf.Pi / 2);
//                     part.SetComponent(_catalog.Models.TexturePart.FlipV, _catalog.Serializers.Bool, true);
//                 }

//                 return true;
//             }
//         }

//         part = null;
//         return false;
//     }

//     private object HandlePartDict(Dictionary<int, object> parts)
//         => parts.Count == 1 ? parts.First().Value : _catalog.Models.CompositePart.Create(parts);

//     private void HandleDetails(OrderedDictionary girlDef, IData<int> girl, int girlId)
//     {
//         //the details are strings identified by index, but I want data security dammit so I guess I
//         //will painfully move index by index
//         if (girlDef.TryGetValue("details", out List<object> details))
//         {
//             var it = details.OfType<string>().GetEnumerator();

//             //lastName
//             if (!it.MoveNext()) { return; }
//             if (it.Current != "-N/A-")
//             {
//                 var lastNameId = GetGirlPropertyId(girlId, "LastName");
//                 _en.AddMessage(lastNameId.ToString(), it.Current);

//                 girl.SetComponent(_catalog.Models.Character.LastName,
//                     _catalog.Serializers.Int,
//                     lastNameId);
//             }

//             //age
//             if (!it.MoveNext()) { return; }
//             if (int.TryParse(it.Current, out var ageParsed))
//             {
//                 girl.SetComponent(_catalog.Models.Character.Age,
//                     _catalog.Serializers.Int,
//                     ageParsed);
//             }

//             //education
//             if (!it.MoveNext()) { return; }
//             var educationId = GetGirlPropertyId(girlId, "Education");
//             _en.AddMessage(educationId.ToString(), it.Current);

//             girl.SetComponent(_catalog.Models.Character.Education,
//                 _catalog.Serializers.Int,
//                 educationId);

//             //height
//             if (!it.MoveNext()) { return; }
//             if (UnitConversion.TryParseDistanceToCm(it.Current, out var height))
//             {
//                 girl.SetComponent(_catalog.Models.Character.Height,
//                     _catalog.Serializers.Float,
//                     height);
//             }

//             //weight
//             if (!it.MoveNext()) { return; }
//             if (UnitConversion.TryParseWeightToKilo(it.Current, out var weight))
//             {
//                 girl.SetComponent(_catalog.Models.Character.Weight,
//                     _catalog.Serializers.Float,
//                     weight);
//             }

//             //occupation
//             if (!it.MoveNext()) { return; }
//             var occupationId = GetGirlPropertyId(girlId, "Occupation");
//             _en.AddMessage(occupationId.ToString(), it.Current);

//             girl.SetComponent(_catalog.Models.Character.Occupation,
//                 _catalog.Serializers.Int,
//                 occupationId);

//             //cupSize
//             if (!it.MoveNext()) { return; }
//             if (_cupSizeLookup.TryGetValue(it.Current.ToLower(), out var cupSize))
//             {
//                 girl.SetComponent(_catalog.Models.Character.CupSize,
//                     _catalog.Serializers.Int,
//                     cupSize);
//             }

//             //birthday
//             if (!it.MoveNext()) { return; }
//             var monthStr = it.Current.Substr(0, 3).ToLower();
//             if (_monthLookup.TryGetValue(monthStr, out var month))
//             {
//                 girl.SetComponent(_catalog.Models.Character.BirthMonth,
//                     _catalog.Serializers.Int,
//                     month);
//             }
//             var dayStr = it.Current.Substring(3, it.Current.Length - 5).Trim().ToLower();
//             if (int.TryParse(dayStr, out var day))
//             {
//                 girl.SetComponent(_catalog.Models.Character.BirthDay,
//                     _catalog.Serializers.Int,
//                     day);
//             }

//             //hobby
//             if (!it.MoveNext()) { return; }
//             var hobbyId = GetGirlPropertyId(girlId, "Hobby");
//             _en.AddMessage(hobbyId.ToString(), it.Current);

//             girl.SetComponent(_catalog.Models.Character.FavColor,
//                 _catalog.Serializers.Int,
//                 hobbyId);

//             //favColor
//             if (!it.MoveNext()) { return; }
//             var colorId = GetGirlPropertyId(girlId, "FavColor");
//             _en.AddMessage(colorId.ToString(), it.Current);

//             girl.SetComponent(_catalog.Models.Character.FavColor,
//                 _catalog.Serializers.Int,
//                 colorId);

//             //favSeason
//             if (!it.MoveNext()) { return; }
//             var seasonId = GetGirlPropertyId(girlId, "FavSeason");
//             _en.AddMessage(seasonId.ToString(), it.Current);

//             girl.SetComponent(_catalog.Models.Character.FavSeason,
//                 _catalog.Serializers.Int,
//                 seasonId);

//             //favHangout
//             if (!it.MoveNext()) { return; }
//             var hangoutId = GetGirlPropertyId(girlId, "FavHangout");
//             _en.AddMessage(hangoutId.ToString(), it.Current);

//             girl.SetComponent(_catalog.Models.Character.FavHangout,
//                 _catalog.Serializers.Int,
//                 hangoutId);
//         }
//     }
// }
