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

namespace HuniePopUltimate;

public class HpGirlExtractor
{
    private static readonly Dictionary<UnityAssetPath, PuzzleAffectionType> AFFECTION_TYPES = new()
    {
        {new UnityAssetPath() { FileId = 0, PathId = 9371 }, PuzzleAffectionType.TALENT},
        {new UnityAssetPath() { FileId = 0, PathId = 9370 }, PuzzleAffectionType.SEXUALITY},
        {new UnityAssetPath() { FileId = 0, PathId = 9368 }, PuzzleAffectionType.ROMANCE},
        {new UnityAssetPath() { FileId = 0, PathId = 9366 }, PuzzleAffectionType.FLIRTATION},
    };

    private readonly Extractor _extractor;
    private readonly HpPartsExtractor _parts;
    private readonly HpSpriteCache _sprites;
    private readonly HpItemCache _items;
    private readonly HpAudioCache _audio;
    private readonly HpDialogExtractor _dialog;
    private readonly HpCutsceneExtractor _cutscenes;
    private readonly Dictionary<int, IGirlConfigurator> _configs;
    private readonly IBodySubDataMod<GirlPartSubDefinition> _nudeOutfitPart;
    private readonly UnityEngine.AssetBundle _assetBundle;
    private readonly Dictionary<RelativeId, SingleDatePairData> _singleDatePairData;
    private readonly Action<RelativeId, IEnumerable<(RelativeId, float)>> _addGirlDatePhotos;
    private readonly Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> _addGirlSexPhotos;

    private (SerializedFile, OrderedDictionary) _thumbnailCollection;

    public HpGirlExtractor(
        Extractor extractor,
        HpPartsExtractor parts,
        HpSpriteCache sprites,
        HpAudioCache audio,
        HpItemCache items,
        HpDialogExtractor dialog,
        HpCutsceneExtractor cutscenes,
        Dictionary<int, IGirlConfigurator> configs,
        IBodySubDataMod<GirlPartSubDefinition> nudeOutfitPart,
        UnityEngine.AssetBundle assetBundle,
        Dictionary<RelativeId, SingleDatePairData> singleDatePairData,
        Action<RelativeId, IEnumerable<(RelativeId, float)>> addGirlDatePhotos,
        Action<RelativeId, IEnumerable<(RelativeId, RelativeId)>> addGirlSexPhotos,
        (SerializedFile, OrderedDictionary) thumbnailCollection)
    {
        _extractor = extractor;
        _parts = parts;
        _sprites = sprites;
        _audio = audio;
        _items = items;
        _dialog = dialog;
        _cutscenes = cutscenes;
        _configs = configs;
        _nudeOutfitPart = nudeOutfitPart;
        _assetBundle = assetBundle;
        _singleDatePairData = singleDatePairData;
        _addGirlDatePhotos = addGirlDatePhotos;
        _addGirlSexPhotos = addGirlSexPhotos;
        _thumbnailCollection = thumbnailCollection;
    }

    public void ExtractGirl(SerializedFile file, OrderedDictionary girlDef, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData)
    {
        if (!girlDef.TryGetValue("id", out int nativeId))
        {
            ModInterface.Log.Error("Hp1 girl lacks id");
            return;
        }

        if (!_configs.TryGetValue(nativeId, out var girlConfig))
        {
            HpDebugLog.GirlWarning($"No configurator registered for girl id {nativeId}");
            return;
        }

        ExtractGirlMetadata(nativeId, girlDef, file, girlConfig);
        ExtractGirlBody(nativeId, girlDef, file, collectionData, girlConfig);
        ExtractGirlPhotos(nativeId, girlDef, collectionData, girlConfig);
    }

    private void ExtractGirlMetadata(int nativeId, OrderedDictionary girlDef, SerializedFile file, IGirlConfigurator girlConfig)
    {
        var id = Girls.FromHp1Id(nativeId);

        if (girlDef.TryGetValue("firstName", out string firstName))
        {
            HpDebugLog.GirlMessage($"id:{nativeId}, name:{firstName}");
            girlConfig.Mod.GirlName = firstName;
        }

        if (girlDef.TryGetValue("mostDesiredTrait", out OrderedDictionary mostDesiredTrait)
            && UnityAssetPath.TryExtract(mostDesiredTrait, out var favAffectionPath)
            && AFFECTION_TYPES.TryGetValue(favAffectionPath, out var favAffection))
        {
            girlConfig.Mod.FavoriteAffectionType = favAffection;
        }

        if (girlDef.TryGetValue("leastDesiredTrait", out OrderedDictionary leastDesiredTrait)
            && UnityAssetPath.TryExtract(leastDesiredTrait, out var leastFavAffectionPath)
            && AFFECTION_TYPES.TryGetValue(leastFavAffectionPath, out var leastFavAffection))
        {
            girlConfig.Mod.LeastFavoriteAffectionType = leastFavAffection;
        }

        girlConfig.Mod.FavoriteDialogLines = new();
        using (ModInterface.Log.MakeIndent("talkQueries"))
        {
            ExtractQueries(id, girlConfig, girlDef, file, girlConfig.Mod.FavoriteDialogLines);
        }

        girlConfig.Mod.HerQuestions = new();
        using (ModInterface.Log.MakeIndent("HerQuestions"))
        {
            ExtractQuestions(girlDef, file, girlConfig.Mod.HerQuestions);
        }

        using (ModInterface.Log.MakeIndent("Intro Scene"))
        {
            var pair = _singleDatePairData.GetOrNew(id);

            using (ModInterface.Log.MakeIndent("intro loc"))
            {
                if (girlDef.TryGetValue("introLocation", out OrderedDictionary introLocation)
                    && UnityAssetPath.TryExtract(introLocation, out var introLocationPath))
                {
                    pair.MeetingLocation = LocationIds.FromUnityPath(introLocationPath);
                }
            }

            _cutscenes.ExtractIntroCutscene(id, girlDef, file, pair);
        }
    }

    private void ExtractGirlBody(int nativeId, OrderedDictionary girlDef, SerializedFile file, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData, IGirlConfigurator girlConfig)
    {
        using (ModInterface.Log.MakeIndent("body"))
        {
            var body = new GirlBodyDataMod(new RelativeId(Plugin.ModId, 0), InsertStyle.append)
            {
                Scale = 1.32f,
                bodyName = "HuniePop",
            };

            girlConfig.Mod.bodies ??= new();
            girlConfig.Mod.bodies.Add(body);

            if (!girlDef.TryGetValue("spriteCollectionName", out string spriteCollectionName)
                || !collectionData.TryGetValue(spriteCollectionName, out var spriteCollection)
                || !_sprites.TryGetSpriteLookup(spriteCollection, out var spriteLookup, out var spriteTextureInfo))
            {
                return;
            }

            ExtractBodyParts(girlDef, body, spriteLookup, spriteTextureInfo);
            ExtractOutfits(girlDef, body, spriteLookup, spriteTextureInfo);
            ExtractHairstyles(girlDef, body, spriteLookup, spriteTextureInfo);
            ExtractMiscPieces(girlDef, body, spriteLookup, spriteTextureInfo);
            ExtractUnderwearAndNude(girlDef, nativeId, body, spriteLookup, spriteTextureInfo, girlConfig);

            body.DefaultOutfitId = body.outfits.First().Id;
            body.DefaultHairstyleId = body.hairstyles.First().Id;
            body.DefaultExpressionId = new RelativeId(-1, (int)GirlExpressionType.NEUTRAL);
            body.FailureExpressionId = new RelativeId(-1, (int)GirlExpressionType.UPSET);

            girlConfig.Mod.VoiceVolume = 1f;
            girlConfig.Mod.SexVoiceVolume = 1f;

            girlConfig.ConfigureGirl(body, _assetBundle, _sprites, _audio, _items);
        }
    }

    private void ExtractBodyParts(OrderedDictionary girlDef, GirlBodyDataMod body, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw spriteTextureInfo)
    {
        if (girlDef.TryGetValue("bodyPiece", out OrderedDictionary bodyPiece)
            && _parts.TryMakePartDataMod(GirlPartType.BODY, bodyPiece, spriteLookup, spriteTextureInfo, out var bodyPartMod, out _))
        {
            body.PartBody = bodyPartMod;
        }

        // HP1 has no head layer — head piece goes into the teeth slot
        if (girlDef.TryGetValue("headPiece", out OrderedDictionary headPiece)
            && _parts.TryMakePartDataMod(GirlPartType.OUTFIT, headPiece, spriteLookup, spriteTextureInfo, out var headPartMod, out var headSprite)
            && !(headSprite._rect.Value.width == 64 && headSprite._rect.Value.height == 64)) // skip empty 64x64 placeholders
        {
            body.specialParts ??= new();
            body.specialParts.Add(new GirlSpecialPartDataMod(_parts.NextPartId(), InsertStyle.append)
            {
                Part = headPartMod,
                SortingPartType = GirlPartType.OUTFIT,
                AnimType = DollPartSpecialAnimType.NONE,
                IsToggleable = false,
                SpecialPartName = "head"
            });
        }
    }

    private void ExtractOutfits(OrderedDictionary girlDef, GirlBodyDataMod body, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw spriteTextureInfo)
    {
        body.outfits ??= new();

        if (!girlDef.TryGetValue("pieces", out List<object> pieces)
            || !girlDef.TryGetValue("outfits", out List<object> outfits))
        {
            return;
        }

        var piecesLookup = pieces.OfType<OrderedDictionary>().ToArray();
        var outfitSequence = HpPartsExtractor.StyleSequence.GetEnumerator();

        foreach (var outfit in outfits.OfType<OrderedDictionary>())
        {
            if (outfit.TryGetValue("styleName", out string outfitName)
                && outfit.TryGetValue("artIndex", out int artIndex)
                && piecesLookup.Length > artIndex
                && piecesLookup[artIndex].TryGetValue("art", out List<object> art)
                && art.Count > 0
                && art[0] is OrderedDictionary outfitPartDef
                && _parts.TryMakePartDataMod(GirlPartType.OUTFIT, outfitPartDef, spriteLookup, spriteTextureInfo, out var outfitPart, out _))
            {
                outfitSequence.MoveNext();
                body.outfits.Add(new OutfitDataMod(outfitSequence.Current, InsertStyle.append)
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

    private void ExtractHairstyles(OrderedDictionary girlDef, GirlBodyDataMod body, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw spriteTextureInfo)
    {
        body.hairstyles ??= new();

        if (!girlDef.TryGetValue("pieces", out List<object> pieces)
            || !girlDef.TryGetValue("hairstyles", out List<object> hairstyles))
        {
            return;
        }

        var piecesLookup = pieces.OfType<OrderedDictionary>().ToArray();
        var hairstyleSequence = HpPartsExtractor.StyleSequence.GetEnumerator();

        foreach (var hairstyle in hairstyles.OfType<OrderedDictionary>())
        {
            if (hairstyle.TryGetValue("styleName", out string hairName)
                && hairstyle.TryGetValue("artIndex", out int artIndex)
                && piecesLookup.Length > artIndex
                && piecesLookup[artIndex].TryGetValue("art", out List<object> art)
                && art.Count > 1
                && art[0] is OrderedDictionary frontPartDef
                && _parts.TryMakePartDataMod(GirlPartType.FRONTHAIR, frontPartDef, spriteLookup, spriteTextureInfo, out var frontPart, out _))
            {
                hairstyleSequence.MoveNext();
                var hairstyleMod = new HairstyleDataMod(hairstyleSequence.Current, InsertStyle.append)
                {
                    Name = hairName,
                    FrontHairPart = frontPart,
                    IsNSFW = false,
                    IsCodeUnlocked = false,
                    IsPurchased = false,
                    TightlyPaired = false,
                };

                if (art[1] is OrderedDictionary backPartDef
                    && _parts.TryMakePartDataMod(GirlPartType.BACKHAIR, backPartDef, spriteLookup, spriteTextureInfo, out var backPart, out _))
                {
                    hairstyleMod.BackHairPart = backPart;
                }

                body.hairstyles.Add(hairstyleMod);
            }
        }
    }

    private void ExtractMiscPieces(OrderedDictionary girlDef, GirlBodyDataMod body, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw spriteTextureInfo)
    {
        if (!girlDef.TryGetValue("pieces", out List<object> pieces))
        {
            return;
        }

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

        foreach (var piece in pieces.OfType<OrderedDictionary>())
        {
            if (!piece.TryGetValue("type", out int typeId))
            {
                continue;
            }

            switch (typeId)
            {
                case 0: // expression
                {
                    if (!_parts.TryExtractExpression(piece, spriteLookup, spriteTextureInfo,
                            out var expression, out _, out _, out var mouth, out var face)
                        || !piece.TryGetValue("expressionType", out int expressionType))
                    {
                        break;
                    }

                    switch (expressionType)
                    {
                        case 0:
                            happy = expression;
                            expression.ExpressionType = GirlExpressionType.NEUTRAL;
                            expression.Id = new RelativeId(-1, (int)GirlExpressionType.NEUTRAL);
                            body.PartMouthNeutral = mouth;
                            break;
                        case 1:
                            sad = expression;
                            expression.ExpressionType = GirlExpressionType.EXHAUSTED;
                            expression.Id = new RelativeId(-1, (int)GirlExpressionType.EXHAUSTED);
                            break;
                        case 2:
                            angry = expression;
                            expression.ExpressionType = GirlExpressionType.ANNOYED;
                            expression.Id = new RelativeId(-1, (int)GirlExpressionType.ANNOYED);
                            break;
                        case 3:
                            excited = expression;
                            expression.ExpressionType = GirlExpressionType.EXCITED;
                            expression.Id = new RelativeId(-1, (int)GirlExpressionType.EXCITED);
                            break;
                        case 4:
                            shy = expression;
                            expression.ExpressionType = GirlExpressionType.SHY;
                            expression.Id = new RelativeId(-1, (int)GirlExpressionType.SHY);
                            face.PartType = GirlPartType.BLUSHLIGHT;
                            body.PartBlushLight = face;
                            break;
                        case 5:
                            confused = expression;
                            expression.ExpressionType = GirlExpressionType.CONFUSED;
                            expression.Id = new RelativeId(-1, (int)GirlExpressionType.CONFUSED);
                            break;
                        case 6:
                            horny = expression;
                            expression.ExpressionType = GirlExpressionType.HORNY;
                            expression.Id = new RelativeId(-1, (int)GirlExpressionType.HORNY);
                            face.PartType = GirlPartType.BLUSHHEAVY;
                            body.PartBlushHeavy = face;
                            break;
                        case 7:
                            sick = expression;
                            expression.ExpressionType = GirlExpressionType.UPSET;
                            expression.Id = new RelativeId(-1, (int)GirlExpressionType.UPSET);
                            break;
                    }

                    expression.PartEyesGlow = expression.PartEyes;
                    body.expressions.Add(expression);
                    break;
                }
                case 1:
                case 2:
                case 3: // footwear — potentially usable for shoe items in future
                    break;
                case 4: // phoneme
                {
                    if (piece.TryGetValue("name", out string phomeneName)
                        && piece.TryGetValue("art", out List<object> phonemeArt)
                        && _parts.TryMakePartDataMod(GirlPartType.PHONEMES, phonemeArt.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var phonemePart, out _))
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
                }
                case 5: // extra / accessories
                {
                    if (!piece.TryGetValue("name", out string extraName)
                        || !piece.TryGetValue("limitToOutfits", out string limitToOutfits)
                        || !piece.TryGetValue("art", out List<object> art))
                    {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(limitToOutfits))
                    {
                        ModInterface.Log.Warning($"{extraName} {limitToOutfits}");
                    }

                    body.specialParts ??= new();

                    switch (extraName.ToLower())
                    {
                        case "glasses":
                        {
                            if (_parts.TryMakePartDataMod(GirlPartType.EYEBROWS, art.OfType<OrderedDictionary>().First(x => x != null), spriteLookup, spriteTextureInfo, out var glassesPart, out _))
                            {
                                body.specialParts.Add(new GirlSpecialPartDataMod(_parts.NextPartId(), InsertStyle.append)
                                {
                                    AnimType = DollPartSpecialAnimType.NONE,
                                    SortingPartType = GirlPartType.EYEBROWS,
                                    Part = glassesPart,
                                    SpecialPartName = extraName
                                });
                            }
                            break;
                        }
                        case "earrings":
                        {
                            if (_parts.TryMakePartDataMod(GirlPartType.EYES, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var earringsPart, out _))
                            {
                                body.specialParts.Add(new GirlSpecialPartDataMod(_parts.NextPartId(), InsertStyle.append)
                                {
                                    AnimType = DollPartSpecialAnimType.NONE,
                                    SortingPartType = GirlPartType.EYES,
                                    Part = earringsPart,
                                    SpecialPartName = extraName
                                });
                            }
                            break;
                        }
                        case "hairclip":
                        {
                            if (_parts.TryMakePartDataMod(GirlPartType.EYEBROWS, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var hairclipPart, out _))
                            {
                                body.specialParts.Add(new GirlSpecialPartDataMod(_parts.NextPartId(), InsertStyle.append)
                                {
                                    AnimType = DollPartSpecialAnimType.NONE,
                                    SortingPartType = GirlPartType.EYEBROWS,
                                    Part = hairclipPart,
                                    SpecialPartName = extraName
                                });
                            }
                            break;
                        }
                        case "disguise front hair":
                        {
                            if (_parts.TryMakePartDataMod(GirlPartType.FRONTHAIR, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var disguiseFrontHair, out _))
                            {
                                kyuDisguise ??= MakeKyuDisguise();
                                kyuDisguise.FrontHairPart = disguiseFrontHair;
                            }
                            break;
                        }
                        case "disguise back hair":
                        {
                            if (_parts.TryMakePartDataMod(GirlPartType.BACKHAIR, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var disguiseBackHair, out _))
                            {
                                kyuDisguise ??= MakeKyuDisguise();
                                kyuDisguise.BackHairPart = disguiseBackHair;
                            }
                            break;
                        }
                        case "cowboy hat":
                        {
                            if (_parts.TryMakePartDataMod(GirlPartType.EYEBROWS, art.OfType<OrderedDictionary>().First(), spriteLookup, spriteTextureInfo, out var cowboyHatPart, out _))
                            {
                                body.specialParts.Add(new GirlSpecialPartDataMod(_parts.NextPartId(), InsertStyle.append)
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
                }
                default:
                {
                    if (piece.TryGetValue("layer", out int layerId) && piece.TryGetValue("name", out string pieceName))
                    {
                        ModInterface.Log.Message($"unhandled part type {typeId}, layer {layerId}, name {pieceName}");
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

        if (happy != null && sad != null && angry != null && excited != null
            && shy != null && confused != null && horny != null && sick != null)
        {
            happy.PartMouthOpen = mouthOpen;
            sad.PartMouthOpen = mouthOpen;
            angry.PartMouthOpen = mouthOpen;
            excited.PartMouthOpen = mouthOpen;
            shy.PartMouthOpen = mouthOpen;
            confused.PartMouthOpen = mouthOpen;
            horny.PartMouthOpen = mouthOpen;
            sick.PartMouthOpen = mouthOpen;

            body.expressions.Add(new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.UPSET), InsertStyle.append)
            {
                ExpressionType = GirlExpressionType.UPSET,
                PartEyebrows = angry.PartEyebrows,
                PartEyes = sad.PartEyes,
                PartEyesGlow = sad.PartEyes,
                PartMouthClosed = shy.PartMouthClosed,
                PartMouthOpen = mouthOpen
            });

            body.expressions.Add(new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.DISAPPOINTED), InsertStyle.append)
            {
                ExpressionType = GirlExpressionType.DISAPPOINTED,
                PartEyebrows = sad.PartEyebrows,
                PartEyes = sad.PartEyes,
                PartEyesGlow = sad.PartEyes,
                PartMouthClosed = shy.PartMouthClosed,
                PartMouthOpen = mouthOpen
            });

            body.expressions.Add(new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.INQUISITIVE), InsertStyle.append)
            {
                ExpressionType = GirlExpressionType.INQUISITIVE,
                PartEyebrows = confused.PartEyebrows,
                PartEyes = happy.PartEyes,
                PartEyesGlow = happy.PartEyes,
                PartMouthClosed = happy.PartMouthClosed,
                PartMouthOpen = mouthOpen
            });

            body.expressions.Add(new GirlExpressionDataMod(new RelativeId(-1, (int)GirlExpressionType.SARCASTIC), InsertStyle.append)
            {
                ExpressionType = GirlExpressionType.SARCASTIC,
                PartEyebrows = confused.PartEyebrows,
                PartEyes = happy.PartEyes,
                PartEyesGlow = happy.PartEyes,
                PartMouthClosed = happy.PartMouthClosed,
                PartMouthOpen = mouthOpen
            });
        }

        if (girlDef.TryGetValue("blinkFullPiece", out OrderedDictionary blinkDef)
            && _parts.TryMakePartDataMod(GirlPartType.BLINK, blinkDef, spriteLookup, spriteTextureInfo, out var blinkPart, out _))
        {
            body.PartBlink = blinkPart;
        }
    }

    private void ExtractUnderwearAndNude(OrderedDictionary girlDef, int nativeId, GirlBodyDataMod body, Dictionary<string, OrderedDictionary> spriteLookup, TextureInfoRaw spriteTextureInfo, IGirlConfigurator config)
    {
        var outfitSequence = HpPartsExtractor.StyleSequence.GetEnumerator();
        int namedOutfitCount = body.outfits?.Count ?? 0;
        for (int i = 0; i < namedOutfitCount; i++)
        {
            outfitSequence.MoveNext();
        }

        if (girlDef.TryGetValue("braPiece", out OrderedDictionary braDef)
            && girlDef.TryGetValue("pantiesPiece", out OrderedDictionary pantiesDef)
            && _parts.TryMakePartDataMod(GirlPartType.OUTFIT, braDef, spriteLookup, spriteTextureInfo, out var braPartMod, out var braSprite)
            && _parts.TryMakePartDataMod(GirlPartType.OUTFIT, pantiesDef, spriteLookup, spriteTextureInfo, out var pantiesPartMod, out var pantiesSprite))
        {
            var underwearPart = _parts.MergeParts(
                Path.Combine(Plugin.IMAGES_DIR, $"{nativeId}_underwear.png"),
                [(braPartMod, braSprite), (pantiesPartMod, pantiesSprite)]);

            ((SpriteInfoTexture)underwearPart.SpriteInfo).GetSprite();

            outfitSequence.MoveNext();
            body.outfits.Add(new OutfitDataMod(outfitSequence.Current, InsertStyle.append)
            {
                Name = config.UnderwearName,
                OutfitPart = underwearPart,
                IsCodeUnlocked = false,
                IsPurchased = false,
                IsNSFW = false,
                HideNipples = true,
            });

            outfitSequence.MoveNext();
            body.outfits.Add(new OutfitDataMod(outfitSequence.Current, InsertStyle.append)
            {
                Name = "Topless",
                OutfitPart = pantiesPartMod,
                IsCodeUnlocked = false,
                IsPurchased = true,
                IsNSFW = true,
                HideNipples = true,
            });
        }

        if (_nudeOutfitPart != null)
        {
            outfitSequence.MoveNext();
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
    }

    private void ExtractGirlPhotos(int nativeId, OrderedDictionary girlDef, Dictionary<string, (SerializedFile, OrderedDictionary)> collectionData, IGirlConfigurator girlConfig)
    {
        if (!girlDef.TryGetValue("photosSpriteCollectionName", out string photosSpriteCollectionName)
            || !collectionData.TryGetValue(photosSpriteCollectionName, out var photoCollection)
            || !_sprites.TryGetSpriteLookup(photoCollection, out var photoLookup, out var photoTextureInfo)
            || !_sprites.TryGetSpriteLookup(_thumbnailCollection, out var thumbLookup, out var thumbTextureInfo)
            || !girlDef.TryGetValue("photos", out List<object> photos))
        {
            return;
        }

        var (censoredIndex, nudeIndex, wetIndex) = girlConfig.PhotoIndexes;
        var datePhotos = new List<RelativeId>();

        int j = -1;
        foreach (var girlPhoto in photos.OfType<OrderedDictionary>())
        {
            j++;
            var photoMod = new PhotoDataMod(new RelativeId(Plugin.ModId, Photos.Count++), InsertStyle.replace);

            if (j == 3)
            {
                _addGirlSexPhotos?.Invoke(girlConfig.Mod.Id, [(photoMod.Id, RelativeId.Default)]);
            }
            else
            {
                datePhotos.Add(photoMod.Id);
            }

            if (!girlPhoto.TryGetValue("fullSpriteName", out List<object> fullSpriteName)
                || !girlPhoto.TryGetValue("thumbnailSpriteName", out List<object> thumbnailSpriteName))
            {
                continue;
            }

            var thumbnailInfo = new IGameDefinitionInfo<UnityEngine.Sprite>[thumbnailSpriteName.Count];
            int i = 0;
            foreach (var thumbName in thumbnailSpriteName.OfType<string>())
            {
                if (!string.IsNullOrEmpty(thumbName)
                    && thumbLookup.TryGetValue(thumbName, out var thumbDef)
                    && _sprites.TryMakeSpriteInfo(thumbDef, thumbTextureInfo, out var thumbInfo))
                {
                    thumbnailInfo[i] = thumbInfo;
                }
                i++;
            }

            var isSinglePhoto = fullSpriteName.Count == 1;
            int nameIndex = -1;

            foreach (var name in fullSpriteName.OfType<string>())
            {
                nameIndex++;
                var isNsfw = girlConfig.IsPhotoIndexNsfw(j);

                if (string.IsNullOrEmpty(name)
                    || !photoLookup.TryGetValue(name, out var photoDef)
                    || (nameIndex == censoredIndex && isNsfw && !isSinglePhoto))
                {
                    continue;
                }

                if (!_sprites.TryMakeSpriteInfoTiled(Path.Combine(Plugin.IMAGES_DIR, $"{name}.png"), photoDef, photoTextureInfo, out var photoInfo))
                {
                    continue;
                }

                if (isSinglePhoto)
                {
                    if (isNsfw)
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

            ModInterface.AddDataMod(photoMod);
        }

        int datePhotoIndex = 0;
        _addGirlDatePhotos?.Invoke(girlConfig.Mod.Id, datePhotos.Select(x => (x, datePhotoIndex++ / 4f)));
    }

    private void ExtractQuestions(OrderedDictionary girlDef, SerializedFile file, Dictionary<RelativeId, IHerQuestionDataInfo> questions)
    {
        if (!girlDef.TryGetValue("talkQuestions", out List<object> talkQuestions))
        {
            return;
        }

        var questionCount = 0;
        foreach (var talkQ in talkQuestions.OfType<OrderedDictionary>())
        {
            var questionId = new RelativeId(Plugin.ModId, questionCount++);
            var question = new HerQuestionInfo() { IncorrectAnswers = new() };
            questions[questionId] = question;

            if (!UnityAssetPath.TryExtract(talkQ, out var talkQPath)
                || !_extractor.TryExtractMonoBehavior(file, talkQPath, out var talkQuery)
                || !talkQuery.TryGetValue("steps", out List<object> steps))
            {
                continue;
            }

            OrderedDictionary dialogStep = null;
            OrderedDictionary answersStep = null;

            foreach (var step in steps.OfType<OrderedDictionary>())
            {
                if (!step.TryGetValue("type", out int type))
                {
                    continue;
                }

                if (type == 0)
                {
                    dialogStep = step;
                }
                else if (type == 1)
                {
                    answersStep = step;
                }
            }

            if (dialogStep == null || answersStep == null)
            {
                ModInterface.Log.Warning("Failed to parse questions steps");
            }

            if (dialogStep != null
                && dialogStep.TryGetValue("sceneLine", out OrderedDictionary sceneLine)
                && sceneLine.TryGetValue("dialogLine", out OrderedDictionary dialogLine)
                && _dialog.TryExtractDialogLine(dialogLine, file, _dialog.NextDialogLineId(), out var dialogLineMod))
            {
                question.DialogLine = dialogLineMod;
            }
            else
            {
                ModInterface.Log.Error("Failed to find dialog step");
                continue;
            }

            if (answersStep != null
                && answersStep.TryGetValue("responseOptions", out List<object> responseOptions))
            {
                var answerCount = 0;
                foreach (var option in responseOptions.OfType<OrderedDictionary>())
                {
                    if (TryExtractAnswer(option, file, out var answerMod))
                    {
                        if (answerCount++ == 0)
                        {
                            question.CorrectAnswer = answerMod;
                        }
                        else
                        {
                            question.IncorrectAnswers[new RelativeId(Plugin.ModId, answerCount)] = answerMod;
                        }
                    }
                }
            }
            else
            {
                ModInterface.Log.Error("Failed to find answer step");
            }
        }
    }

    private bool TryExtractAnswer(OrderedDictionary answerDef, SerializedFile file, out HerQuestionAnswerInfo answer)
    {
        if (!answerDef.TryGetValue("text", out string text)
            || !answerDef.TryGetValue("secondary", out bool secondary)
            || !answerDef.TryGetValue("secondaryText", out string secondaryText)
            || !answerDef.TryGetValue("steps", out List<object> optionSteps)
            || !optionSteps.Any()
            || optionSteps[0] is not OrderedDictionary step
            || !step.TryGetValue("type", out int type))
        {
            answer = null;
            return false;
        }

        answer = new() { text = text };
        if (secondary)
        {
            answer.altText = secondaryText;
        }

        if (type == 11)
        {
            return true;
        }

        if (type == 0
            && step.TryGetValue("sceneLine", out OrderedDictionary answerSceneLine)
            && answerSceneLine.TryGetValue("dialogLine", out OrderedDictionary answerDialogLine)
            && _dialog.TryExtractDialogLine(answerDialogLine, file, _dialog.NextDialogLineId(), out answer.Response))
        {
            return true;
        }

        ModInterface.Log.Error($"Unhandled step type {type}");
        answer = null;
        return false;
    }

    private void ExtractQueries(RelativeId girlId, IGirlConfigurator config, OrderedDictionary girlDef, SerializedFile file, Dictionary<RelativeId, IDialogLineDataMod> favoriteDialogLines)
    {
        if (!girlDef.TryGetValue("talkQueries", out List<object> talkQueries))
        {
            return;
        }

        var questionOrder = config.FavQuestionOrder;
        if (questionOrder == null)
        {
            HpDebugLog.GirlWarning($"No question order for girl {girlId}");
            return;
        }

        var questionOrderEnum = questionOrder.GetEnumerator();
        foreach (var talkQ in talkQueries.OfType<OrderedDictionary>())
        {
            if (!UnityAssetPath.TryExtract(talkQ, out var talkQPath)
                || !_extractor.TryExtractMonoBehavior(file, talkQPath, out var talkQuery)
                || !talkQuery.TryGetValue("steps", out List<object> steps))
            {
                continue;
            }

            foreach (var step in steps.OfType<OrderedDictionary>().Skip(1))
            {
                foreach (var line in ExtractQueryLines(step, file))
                {
                    if (!questionOrderEnum.MoveNext())
                    {
                        break;
                    }

                    favoriteDialogLines[questionOrderEnum.Current] = line;
                }
            }
        }
    }

    private IEnumerable<DialogLineDataMod> ExtractQueryLines(OrderedDictionary dialogSceneStep, SerializedFile file)
    {
        if (!dialogSceneStep.TryGetValue("responseOptions", out List<object> responseOptions))
        {
            yield break;
        }

        foreach (var branch in responseOptions.OfType<OrderedDictionary>())
        {
            if (!branch.TryGetValue("steps", out List<object> steps))
            {
                continue;
            }

            foreach (var step in steps.OfType<OrderedDictionary>())
            {
                if (!step.TryGetValue("conditionalBranchs", out List<object> conditionalBranches))
                {
                    continue;
                }

                foreach (var conditionalBranch in conditionalBranches.OfType<OrderedDictionary>())
                {
                    if (!conditionalBranch.TryGetValue("steps", out List<object> conditionalBranchSteps))
                    {
                        continue;
                    }

                    foreach (var conditionalBranchStep in conditionalBranchSteps.OfType<OrderedDictionary>())
                    {
                        if (conditionalBranchStep.TryGetValue("type", out int type)
                            && type == 0
                            && conditionalBranchStep.TryGetValue("sceneLine", out OrderedDictionary sceneLine)
                            && sceneLine.TryGetValue("dialogLine", out OrderedDictionary dialogLine)
                            && _dialog.TryExtractDialogLine(dialogLine, file, _dialog.NextDialogLineId(), out var dialogLineMod))
                        {
                            yield return dialogLineMod;
                        }
                    }
                }
            }
        }
    }

    private static HairstyleDataMod MakeKyuDisguise()
        => new HairstyleDataMod(Styles.KyuDisguise, InsertStyle.append) { HideSpecial = true };
}