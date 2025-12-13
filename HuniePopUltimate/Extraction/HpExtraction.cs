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


    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, TextureInfoRaw>> _textureInfo = new();
    private Dictionary<SerializedFile, Dictionary<UnityAssetPath, AudioClipInfoVorbisLazy>> _audioInfo = new();
    //private Dictionary<SerializedFile, Dictionary<UnityAssetPath, OrderedDictionary>> _cutscenes = new();
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

        foreach (var file_girlDefs in _extractor.ExtractMonoBehaviors("GirlDefinition"))
        {
            foreach (var girlDef in file_girlDefs.Value)
            {
                ExtractGirl(file_girlDefs.Key, girlDef, collectionData);
            }
        }

        foreach (var file_locationDef in _extractor.ExtractMonoBehaviors("LocationDefinition"))
        {
            foreach (var locationDef in file_locationDef.Value)
            {
                ExtractLocation(file_locationDef.Key, locationDef, collectionData);
            }
        }

        AddHp2GirlLocMods();

        foreach (var mod in _hpGirlIdToMod.Values)
        {
            ModInterface.AddDataMod(mod);
        }

        // make all textures read-only now that they've been processed
        foreach (var textureInfo in _textureInfo.Values.SelectMany(x => x.Values))
        {
            textureInfo.GetTexture().Apply(false, true);
        }
    }

    private bool TryExtractAudioDef(OrderedDictionary AudioDef, SerializedFile file, out AudioClipInfoVorbisLazy clipInfo)
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
                    foreach (var entry in list)
                    {
                        if (entry is OrderedDictionary orderedDict) LogAll(orderedDict);
                        else ModInterface.Log.Message($"{vals.Current?.ToString() ?? "null"}");
                    }
                }
            }
            else
            {
                ModInterface.Log.Message($"{keys.Current} : {vals.Current?.ToString() ?? "null"}");
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

    private void ExtractGirl(SerializedFile file, OrderedDictionary girlDef, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData)
    {
        var censoredIndex = -1;
        var nudeIndex = -1;
        var wetIndex = -1;

        if (!girlDef.TryGetValue("id", out int nativeId))
        {
            ModInterface.Log.Error("Hp2 girl lacks id");
            return;
        }

        var girlMod = GetGirlMod(nativeId);

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

        _uncensoredPhotos.TryGetValue(nativeId, out var censoredIndexes);

        if (girlDef.TryGetValue("firstName", out string firstName))
        {
            ModInterface.Log.Message($"id:{nativeId}, name:{firstName}");
            girlMod.GirlName = firstName;
        }

        if (girlDef.TryGetValue("mostDesiredTrait", out OrderedDictionary mostDesiredTrait)
            && UnityAssetPath.TryExtract(mostDesiredTrait, out var favAffectionType)
            && _affectionTypes.TryGetValue(favAffectionType, out var affectionType))
        {
            girlMod.FavoriteAffectionType = affectionType;
        }

        if (girlDef.TryGetValue("leastDesiredTrait", out OrderedDictionary leastDesiredTrait)
            && UnityAssetPath.TryExtract(leastDesiredTrait, out var leastFavAffectionType)
            && _affectionTypes.TryGetValue(leastFavAffectionType, out affectionType))
        {
            girlMod.LeastFavoriteAffectionType = affectionType;
        }

        girlMod.FavoriteDialogLines = new();
        using (ModInterface.Log.MakeIndent("talkQueries"))
        {
            ExtractQueries(Girls.FromHp1Id(nativeId), girlDef, file, girlMod.FavoriteDialogLines);
            foreach (var foo in girlMod.FavoriteDialogLines.Keys)
            {
                ModInterface.Log.Message(foo.ToString());
            }
        }

        using (ModInterface.Log.MakeIndent("body"))
        {
            girlMod.bodies ??= new() {
                new GirlBodyDataMod(new RelativeId(Plugin.ModId,0), InsertStyle.append) {
                    Scale = 1.32f,
                    bodyName = "HuniePop",
                }
            };

            var body = (GirlBodyDataMod)girlMod.bodies.First();

            if (girlDef.TryGetValue("spriteCollectionName", out string spriteCollectionName)
                        && collectionData.TryGetValue(spriteCollectionName, out var spriteCollection)
                        && TryGetSpriteLookup(spriteCollection, out var spriteLookup, out var spriteTextureInfo))
            {
                //body
                if (girlDef.TryGetValue("bodyPiece", out OrderedDictionary bodyPiece)
                    && TryMakePartDataMod(GirlPartType.BODY, bodyPiece, spriteLookup, spriteTextureInfo, out var bodyPartMod, out var bodySprite))
                {
                    body.PartBody = bodyPartMod;
                }

                //there is no head layer, so we make the head into teeth. Yeh.
                if (girlDef.TryGetValue("headPiece", out OrderedDictionary headPiece)
                        && TryMakePartDataMod(GirlPartType.OUTFIT, headPiece, spriteLookup, spriteTextureInfo, out var headPartMod, out var headSprite)
                        && !(headSprite._rect.Value.width == 64 && headSprite._rect.Value.height == 64))//there's empty ones that are 64 by 64 exactly
                {
                    body.specialParts ??= new();
                    body.specialParts.Add(new GirlSpecialPartDataMod(new RelativeId(Plugin.ModId, _partCount++), InsertStyle.append)
                    {
                        Part = headPartMod,
                        SortingPartType = GirlPartType.OUTFIT,
                        AnimType = DollPartSpecialAnimType.NONE,
                        IsToggleable = false,
                        SpecialPartName = "head"
                    });
                }

                //pieces
                var outfitCount = 0;
                var hairstyleCount = 0;
                if (girlDef.TryGetValue("pieces", out List<object> pieces))
                {
                    var piecesLookup = pieces.OfType<OrderedDictionary>().ToArray();

                    //outfits
                    body.outfits ??= new();
                    if (girlDef.TryGetValue("outfits", out List<object> outfits))
                    {
                        foreach (var outfit in outfits.OfType<OrderedDictionary>())
                        {
                            if (outfit.TryGetValue("styleName", out string outfitName)
                                && outfit.TryGetValue("artIndex", out int artIndex)
                                && piecesLookup.Length > artIndex
                                && piecesLookup[artIndex].TryGetValue("art", out List<object> art)
                                && art.Count > 0
                                && art[0] is OrderedDictionary outfitPartDef
                                && TryMakePartDataMod(GirlPartType.OUTFIT, outfitPartDef, spriteLookup, spriteTextureInfo,
                                    out var outfitPart, out var outfitSpriteInfo))
                            {
                                ModInterface.Log.Message($"Outfit {outfitName} - {outfitCount}");

                                body.outfits.Add(new OutfitDataMod(new RelativeId(Plugin.ModId, outfitCount++), InsertStyle.append)
                                {
                                    Name = outfitName,
                                    OutfitPart = outfitPart,
                                    IsNSFW = false,
                                    IsCodeUnlocked = false,
                                    IsPurchased = false,
                                    HideNipples = true,
                                    HideSpecial = false,
                                    TightlyPaired = false,
                                });
                            }
                        }
                    }

                    //hairstyles
                    body.hairstyles ??= new();
                    if (girlDef.TryGetValue("hairstyles", out List<object> hairstyles))
                    {
                        foreach (var hairstyle in hairstyles.OfType<OrderedDictionary>())
                        {
                            if (hairstyle.TryGetValue("styleName", out string hairName)
                                && hairstyle.TryGetValue("artIndex", out int artIndex)
                                && piecesLookup.Length > artIndex
                                && piecesLookup[artIndex].TryGetValue("art", out List<object> art)
                                && art.Count > 1
                                && art[0] is OrderedDictionary frontPartDef
                                && TryMakePartDataMod(GirlPartType.FRONTHAIR, frontPartDef, spriteLookup, spriteTextureInfo, out var frontPart, out var frontSpriteInfo))
                            {
                                var hairstyleMod = new HairstyleDataMod(new RelativeId(Plugin.ModId, hairstyleCount++), InsertStyle.append)
                                {
                                    Name = hairName,
                                    FrontHairPart = frontPart,
                                    IsNSFW = false,
                                    IsCodeUnlocked = false,
                                    IsPurchased = false,
                                    TightlyPaired = false,
                                };

                                //they don't all have backs
                                if (art[1] is OrderedDictionary backPartDef
                                    && TryMakePartDataMod(GirlPartType.BACKHAIR, backPartDef, spriteLookup, spriteTextureInfo, out var backPart, out var backSpriteInfo))
                                {
                                    hairstyleMod.BackHairPart = backPart;
                                }

                                body.hairstyles.Add(hairstyleMod);
                            }
                        }
                    }

                    //misc
                    GirlExpressionDataMod happy = null;
                    GirlExpressionDataMod sad = null;
                    GirlExpressionDataMod angry = null;
                    GirlExpressionDataMod excited = null;
                    GirlExpressionDataMod shy = null;
                    GirlExpressionDataMod confused = null;
                    GirlExpressionDataMod horny = null;
                    GirlExpressionDataMod sick = null;
                    GirlPartDataMod mouthOpen = new();
                    HairstyleDataMod kyuDisguise = null;

                    body.expressions ??= new();

                    foreach (var piece in piecesLookup)
                    {
                        if (piece.TryGetValue("type", out int typeId))
                        {
                            switch (typeId)
                            {
                                case 0://expression
                                    if (TryExtractExpression(piece, spriteLookup, spriteTextureInfo,
                                        out var expression, out var brows, out var eyes, out var mouth, out var face)
                                        && piece.TryGetValue("expressionType", out int expressionType))
                                    {
                                        switch (expressionType)
                                        {
                                            case 0://happy
                                                happy = expression;
                                                expression.ExpressionType = GirlExpressionType.NEUTRAL;
                                                expression.Id = new RelativeId(-1, (int)GirlExpressionType.NEUTRAL);
                                                body.PartMouthNeutral = mouth;
                                                break;
                                            case 1://sad
                                                expression.ExpressionType = GirlExpressionType.EXHAUSTED;
                                                sad = expression;
                                                expression.Id = new RelativeId(-1, (int)GirlExpressionType.EXHAUSTED);
                                                break;
                                            case 2://angry
                                                expression.ExpressionType = GirlExpressionType.ANNOYED;
                                                angry = expression;
                                                expression.Id = new RelativeId(-1, (int)GirlExpressionType.ANNOYED);
                                                break;
                                            case 3://excited
                                                expression.ExpressionType = GirlExpressionType.EXCITED;
                                                excited = expression;
                                                expression.Id = new RelativeId(-1, (int)GirlExpressionType.EXCITED);
                                                break;
                                            case 4://shy
                                                expression.ExpressionType = GirlExpressionType.SHY;
                                                shy = expression;
                                                expression.Id = new RelativeId(-1, (int)GirlExpressionType.SHY);

                                                face.PartType = GirlPartType.BLUSHLIGHT;
                                                body.PartBlushLight = face;
                                                break;
                                            case 5://confused
                                                expression.ExpressionType = GirlExpressionType.CONFUSED;
                                                confused = expression;
                                                expression.Id = new RelativeId(-1, (int)GirlExpressionType.CONFUSED);
                                                break;
                                            case 6://horny
                                                expression.ExpressionType = GirlExpressionType.HORNY;
                                                horny = expression;
                                                expression.Id = new RelativeId(-1, (int)GirlExpressionType.HORNY);

                                                face.PartType = GirlPartType.BLUSHHEAVY;
                                                body.PartBlushHeavy = face;
                                                break;
                                            case 7://sick
                                                expression.ExpressionType = GirlExpressionType.UPSET;
                                                sick = expression;
                                                expression.Id = new RelativeId(-1, (int)GirlExpressionType.UPSET);
                                                break;
                                        }

                                        //TODO: glow eyes
                                        expression.PartEyesGlow = expression.PartEyes;
                                        body.expressions.Add(expression);
                                    }
                                    break;
                                case 1:
                                case 2:
                                case 3://footwear
                                    //maybe use these for shoes items? It'd look a little weird but maybe
                                    break;
                                case 4://phomene
                                    if (piece.TryGetValue("name", out string phomeneName)
                                        && piece.TryGetValue("art", out List<object> phomeneArtCollection)
                                        && TryMakePartDataMod(GirlPartType.PHONEMES, phomeneArtCollection.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var phonemePart, out _))
                                    {
                                        switch (phomeneName)
                                        {
                                            case "a,e,i,l":
                                                body.Phonemes_aeil = phonemePart;
                                                body.PhonemesTeeth_aeil = phonemePart;
                                                mouthOpen = phonemePart;
                                                break;
                                            case "b,m,p":
                                                body.Phonemes_neutral = phonemePart;
                                                body.PhonemesTeeth_neutral = phonemePart;
                                                break;
                                            case "o,q,u,w":
                                                body.Phonemes_oquw = phonemePart;
                                                body.PhonemesTeeth_oquw = phonemePart;
                                                break;
                                            case "f,v":
                                                body.Phonemes_fv = phonemePart;
                                                body.PhonemesTeeth_fv = phonemePart;
                                                break;
                                            case "c,d,g,h,j,k,n,r,s,t,x,y,z":
                                                body.Phonemes_other = phonemePart;
                                                body.PhonemesTeeth_other = phonemePart;
                                                break;
                                        }
                                    }
                                    break;
                                case 5://extra
                                    if (piece.TryGetValue("name", out string extraName)
                                        && piece.TryGetValue("limitToOutfits", out string limitToOutfits)
                                        && piece.TryGetValue("art", out List<object> art))
                                    {
                                        if (!string.IsNullOrWhiteSpace(limitToOutfits))
                                        {
                                            ModInterface.Log.Warning($"{extraName} {limitToOutfits}");
                                        }

                                        body.specialParts ??= new();
                                        switch (extraName.ToLower())
                                        {
                                            case "glasses":
                                                if (TryMakePartDataMod(GirlPartType.EYEBROWS, art.OfType<OrderedDictionary>().First(x => x != null), spriteLookup, spriteTextureInfo, out var glassesPart, out _))
                                                {

                                                    body.specialParts.Add(new GirlSpecialPartDataMod(new RelativeId(Plugin.ModId, _partCount++), InsertStyle.append)
                                                    {
                                                        AnimType = DollPartSpecialAnimType.NONE,
                                                        SortingPartType = GirlPartType.EYEBROWS,
                                                        Part = glassesPart,
                                                        SpecialPartName = extraName
                                                    });
                                                }
                                                break;
                                            case "earrings":
                                                if (TryMakePartDataMod(GirlPartType.EYES, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var earringsPart, out _))
                                                {
                                                    body.specialParts.Add(new GirlSpecialPartDataMod(new RelativeId(Plugin.ModId, _partCount++), InsertStyle.append)
                                                    {
                                                        AnimType = DollPartSpecialAnimType.NONE,
                                                        SortingPartType = GirlPartType.EYES,
                                                        Part = earringsPart,
                                                        SpecialPartName = extraName
                                                    });
                                                }
                                                break;
                                            case "hairclip":
                                                if (TryMakePartDataMod(GirlPartType.EYEBROWS, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var hairclipPart, out _))
                                                {
                                                    body.specialParts.Add(new GirlSpecialPartDataMod(new RelativeId(Plugin.ModId, _partCount++), InsertStyle.append)
                                                    {
                                                        AnimType = DollPartSpecialAnimType.NONE,
                                                        SortingPartType = GirlPartType.EYEBROWS,
                                                        Part = hairclipPart,
                                                        SpecialPartName = extraName
                                                    });
                                                }
                                                break;
                                            case "disguise front hair":
                                                if (TryMakePartDataMod(GirlPartType.FRONTHAIR, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var disguiseFrontHair, out _))
                                                {
                                                    kyuDisguise ??= new();
                                                    kyuDisguise.FrontHairPart = disguiseFrontHair;
                                                }
                                                break;
                                            case "disguise back hair":
                                                if (TryMakePartDataMod(GirlPartType.BACKHAIR, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var disguiseBackHair, out _))
                                                {
                                                    kyuDisguise ??= new();
                                                    kyuDisguise.BackHairPart = disguiseBackHair;
                                                }
                                                break;
                                            case "cowboy hat":
                                                if (TryMakePartDataMod(GirlPartType.EYEBROWS, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var cowboyHatPart, out _))
                                                {
                                                    body.specialParts.Add(new GirlSpecialPartDataMod(new RelativeId(Plugin.ModId, _partCount++), InsertStyle.append)
                                                    {
                                                        AnimType = DollPartSpecialAnimType.NONE,
                                                        SortingPartType = GirlPartType.EYEBROWS,
                                                        Part = cowboyHatPart,
                                                        SpecialPartName = extraName
                                                    });
                                                }
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    if (piece.TryGetValue("layer", out int layerId)
                                    && piece.TryGetValue("name", out string pieceName))
                                    {
                                        ModInterface.Log.Message($"unhandled part type {typeId}, for layer {layerId}, named {pieceName}");
                                    }
                                    break;
                            }
                        }
                    }

                    if (kyuDisguise != null)
                    {
                        kyuDisguise.Name = "Human Disguise";
                        body.hairstyles.Add(kyuDisguise);
                    }

                    happy.PartMouthOpen = mouthOpen;
                    sad.PartMouthOpen = mouthOpen;
                    angry.PartMouthOpen = mouthOpen;
                    excited.PartMouthOpen = mouthOpen;
                    shy.PartMouthOpen = mouthOpen;
                    confused.PartMouthOpen = mouthOpen;
                    horny.PartMouthOpen = mouthOpen;
                    sick.PartMouthOpen = mouthOpen;

                    //upset
                    body.expressions.Add(new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.UPSET), InsertStyle.append)
                    {
                        ExpressionType = GirlExpressionType.UPSET,
                        PartEyebrows = angry.PartEyebrows,
                        PartEyes = sad.PartEyes,
                        PartEyesGlow = sad.PartEyes,
                        PartMouthClosed = shy.PartMouthClosed,
                        PartMouthOpen = mouthOpen
                    });

                    //disappointed
                    body.expressions.Add(new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.DISAPPOINTED), InsertStyle.append)
                    {
                        ExpressionType = GirlExpressionType.DISAPPOINTED,
                        PartEyebrows = sad.PartEyebrows,
                        PartEyes = sad.PartEyes,
                        PartEyesGlow = sad.PartEyes,
                        PartMouthClosed = shy.PartMouthClosed,
                        PartMouthOpen = mouthOpen
                    });

                    //inquisitive
                    body.expressions.Add(new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.INQUISITIVE), InsertStyle.append)
                    {
                        ExpressionType = GirlExpressionType.INQUISITIVE,
                        PartEyebrows = confused.PartEyebrows,
                        PartEyes = happy.PartEyes,
                        PartEyesGlow = happy.PartEyes,
                        PartMouthClosed = happy.PartMouthClosed,
                        PartMouthOpen = mouthOpen
                    });

                    //sarcastic
                    body.expressions.Add(new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.SARCASTIC), InsertStyle.append)
                    {
                        ExpressionType = GirlExpressionType.SARCASTIC,
                        PartEyebrows = confused.PartEyebrows,
                        PartEyes = happy.PartEyes,
                        PartEyesGlow = happy.PartEyes,
                        PartMouthClosed = happy.PartMouthClosed,
                        PartMouthOpen = mouthOpen
                    });

                    //blink
                    if (girlDef.TryGetValue("blinkFullPiece", out OrderedDictionary blinkDef)
                        && TryMakePartDataMod(GirlPartType.BLINK, blinkDef, spriteLookup, spriteTextureInfo, out var blinkPart, out _))
                    {
                        body.PartBlink = blinkPart;
                    }
                }

                //underwear
                if (girlDef.TryGetValue("braPiece", out OrderedDictionary braDef)
                    && girlDef.TryGetValue("pantiesPiece", out OrderedDictionary pantiesDef)
                    && TryMakePartDataMod(GirlPartType.OUTFIT, braDef, spriteLookup, spriteTextureInfo, out var braPartMod, out var braSprite)
                    && TryMakePartDataMod(GirlPartType.OUTFIT, pantiesDef, spriteLookup, spriteTextureInfo, out var pantiesPartMod, out var pantiesSprite))
                {
                    var underwearPart = MergeParts(Path.Combine(Plugin.IMAGES_DIR, $"{nativeId}_underwear.png"), [
                        (braPartMod, braSprite),
                        (pantiesPartMod, pantiesSprite)
                    ]);

                    //pre render the sprite so that we can make the sprite sheets readonly after
                    ((SpriteInfoTexture)underwearPart.SpriteInfo).GetSprite();

                    body.outfits.Add(new OutfitDataMod(new RelativeId(Plugin.ModId, outfitCount++), InsertStyle.append)
                    {
                        Name = GetUnderwearName(nativeId),
                        OutfitPart = underwearPart,
                        IsCodeUnlocked = false,
                        IsPurchased = false,
                        IsNSFW = false,
                        HideNipples = true,
                    });

                    body.outfits.Add(new OutfitDataMod(new RelativeId(Plugin.ModId, outfitCount++), InsertStyle.append)
                    {
                        Name = "Topless",
                        OutfitPart = pantiesPartMod,
                        IsCodeUnlocked = false,
                        IsPurchased = false,
                        IsNSFW = true,
                        HideNipples = true,
                    });
                }

                //nude
                if (_nudeOutfitPart != null)
                {
                    body.outfits.Add(new OutfitDataMod(_nudeOutfitPart.Id, InsertStyle.append)
                    {
                        Name = "Nude",
                        OutfitPart = _nudeOutfitPart,
                        IsCodeUnlocked = false,
                        IsPurchased = false,
                        IsNSFW = true,
                        HideNipples = false,
                    });
                }

                //defaults
                body.DefaultOutfitId = body.outfits.First().Id;
                body.DefaultHairstyleId = body.hairstyles.First().Id;
                body.DefaultExpressionId = new RelativeId(-1, (int)GirlExpressionType.NEUTRAL); ;
                body.FailureExpressionId = new RelativeId(-1, (int)GirlExpressionType.UPSET); ;

                girlMod.VoiceVolume = 1f;
                girlMod.SexVoiceVolume = 1f;

                Tweak(girlMod, nativeId);
            }
        }

        if (girlDef.TryGetValue("photosSpriteCollectionName", out string photosSpriteCollectionName)
            && collectionData.TryGetValue(photosSpriteCollectionName, out var photoCollection)
            && TryGetSpriteLookup(photoCollection, out var photoLookup, out var photoTextureInfo)
            && TryGetSpriteLookup(_thumbnailCollection, out var thumbLookup, out var thumbTextureInfo)
            && girlDef.TryGetValue("photos", out List<object> photos))
        {
            var datePhotos = new List<RelativeId>();

            int j = -1;
            foreach (var girlPhoto in photos.OfType<OrderedDictionary>())
            {
                j++;
                var photoMod = new PhotoDataMod(new RelativeId(Plugin.ModId, Plugin._photoModCount++), Hp2BaseMod.Utility.InsertStyle.replace);
                //single date add photos
                if (j == 3)
                {
                    m_AddGirlSexPhotos?.Invoke(girlMod.Id, [(photoMod.Id, RelativeId.Default)]);
                }
                else
                {
                    datePhotos.Add(photoMod.Id);
                }

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
                    }

                    ModInterface.AddDataMod(photoMod);
                }
            }

            int datePhotoIndex = 0;
            m_AddGirlDatePhotos?.Invoke(girlMod.Id, datePhotos.Select(x => (x, datePhotoIndex++ / 4f)));
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

    private bool TryGetAudioClipInfo(SerializedFile file, UnityAssetPath path, out AudioClipInfoVorbisLazy clipInfo)
    {
        var pathToInfo = _audioInfo.GetOrNew(file);

        if (pathToInfo.TryGetValue(path, out clipInfo))
        {
            return true;
        }

        var audioClip = (AssetStudio.AudioClip)_extractor.GetAsset(file, path);

        if (audioClip.m_Type == FMODSoundType.OGGVORBIS)
        {
            clipInfo = new AudioClipInfoVorbisLazy(audioClip.m_AudioData);
            return true;
        }

        clipInfo = null;
        return false;
    }
}
