// Hp2BaseModdedLoader 2021, by OneSuchKeeper

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using Newtonsoft.Json;
using UnityEngine;

namespace Hp2BaseMod
{
    internal static class GameDataModder
    {

        private class BodyData
        {
            public List<IGirlBodyDataMod> bodyMods = new();
            public Dictionary<RelativeId, List<IBodySubDataMod<GirlPartSubDefinition>>> partMods = new();
            public Dictionary<RelativeId, List<IBodySubDataMod<GirlOutfitSubDefinition>>> outfitMods = new();
            public Dictionary<RelativeId, List<IBodySubDataMod<GirlHairstyleSubDefinition>>> hairstyleMods = new();
            public Dictionary<RelativeId, List<IBodySubDataMod<GirlSpecialPartSubDefinition>>> specialPartMods = new();
            public Dictionary<RelativeId, List<IBodySubDataMod<GirlExpressionSubDefinition>>> expressionMods = new();
        }

        /// <summary>
        /// by default each has one default (0), 12 for the normal girls, 1 for kyu, 2 for nymphojinn. 1+12+1+2=16
        /// </summary>
        private const int DEFAULT_DT_SET_COUNT = 16;

        private static readonly string _defaultDataDir = Path.Combine(Paths.PluginPath, "Hp2BaseMod", "DefaultDataMods");
        private static readonly bool _isDevMode = false;
        private static readonly string _defaultDataPath = @"C:\Git\onesuchkeeper\Hp2BaseMod\Hp2BaseMod\DefaultData.cs";

        //in game this info is stored in the location manager which isn't instantiated at this point
        //I don't love it but for now I'll just copy it here
        private static Dictionary<int, ClockDaytimeType> _locationIdToDateTime = new Dictionary<int, ClockDaytimeType>(){
            {9, ClockDaytimeType.MORNING},
            {10, ClockDaytimeType.MORNING},
            {11, ClockDaytimeType.MORNING},
            {12, ClockDaytimeType.AFTERNOON},
            {13, ClockDaytimeType.AFTERNOON},
            {14, ClockDaytimeType.AFTERNOON},
            {15, ClockDaytimeType.EVENING},
            {16, ClockDaytimeType.EVENING},
            {17, ClockDaytimeType.EVENING},
            {18, ClockDaytimeType.NIGHT},
            {19, ClockDaytimeType.NIGHT},
            {20, ClockDaytimeType.NIGHT},
        };

        private static HashSet<int> _specialDateLocationIds = new HashSet<int>(){
            23,//outer space
            26,//airplane bathroom
        };

        public static void Mod(GameData gameData)
        {
            ModInterface.Log.LogInfo($"Loaded data sources: [{string.Join(", ", ModInterface.Save.SourceGUID_Id.Select(x => $"{x.Key} - {x.Value}"))}]");

            //okay, here's the plan
            //1) get all of the default data
            //2) register all the default data
            //3) if in dev mode, make mods for and save default data
            //4) get all the mods and create empties and register
            //5) get sub mods create empties and register
            //6) apply mods
            try
            {
                var assetProvider = ModInterface.Assets;
                var gameDataProvider = ModInterface.GameData;

                using (ModInterface.Log.MakeIndent("Modifying GameData"))
                {
                    //grab dicts
                    var abilityDataDict = GetDataDict<AbilityDefinition>(gameData, typeof(AbilityData), "_abilityData");
                    var ailmentDataDict = GetDataDict<AilmentDefinition>(gameData, typeof(AilmentData), "_ailmentData");
                    var codeDataDict = GetDataDict<CodeDefinition>(gameData, typeof(CodeData), "_codeData");
                    var cutsceneDataDict = GetDataDict<CutsceneDefinition>(gameData, typeof(CutsceneData), "_cutsceneData");
                    var dialogTriggerDataDict = GetDataDict<DialogTriggerDefinition>(gameData, typeof(DialogTriggerData), "_dialogTriggerData");
                    var dlcDataDict = GetDataDict<DlcDefinition>(gameData, typeof(DlcData), "_dlcData");
                    var energyDataDict = GetDataDict<EnergyDefinition>(gameData, typeof(EnergyData), "_energyData");
                    var girlDataDict = GetDataDict<GirlDefinition>(gameData, typeof(GirlData), "_girlData");
                    var girlPairDataDict = GetDataDict<GirlPairDefinition>(gameData, typeof(GirlPairData), "_girlPairData");
                    var itemDataDict = GetDataDict<ItemDefinition>(gameData, typeof(ItemData), "_itemData");
                    var locationDataDict = GetDataDict<LocationDefinition>(gameData, typeof(LocationData), "_locationData");
                    var photoDataDict = GetDataDict<PhotoDefinition>(gameData, typeof(PhotoData), "_photoData");
                    var questionDataDict = GetDataDict<QuestionDefinition>(gameData, typeof(QuestionData), "_questionData");
                    var tokenDataDict = GetDataDict<TokenDefinition>(gameData, typeof(TokenData), "_tokenData");

                    // register default sub data
                    using (ModInterface.Log.MakeIndent("registering default sub data"))
                    {
                        ModInterface.Log.LogInfo("dialog triggers");
                        foreach (var dt in dialogTriggerDataDict.Values)
                        {
                            var expansion = ExpandedDialogTriggerDefinition.Get(dt);
                            var id = new RelativeId(-1, dt.id);

                            // lines are looked up by trigger id and girl id.
                            var girlIndex = 0;//each line set corresponds with a girl
                            foreach (var lineSet in dt.dialogLineSets)
                            {
                                var girlId = new RelativeId(-1, girlIndex);

                                var lineIndexLookup = expansion.GirlIdToLineIdToLineIndex.GetOrNew(girlId);
                                var lineIdLookup = expansion.GirlIdToLineIndexToLineId.GetOrNew(girlId);

                                var lineIndex = 0;
                                foreach (var line in lineSet.dialogLines)
                                {
                                    var lineId = new RelativeId(-1, lineIndex);

                                    lineIndexLookup[lineId] = lineIndex;
                                    lineIdLookup[lineIndex] = lineId;

                                    lineIndex++;
                                }

                                girlIndex++;
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girls"))
                        {
                            foreach (var girl in girlDataDict.Values)
                            {
                                using (ModInterface.Log.MakeIndent($"Id: {girl.id}, Name: {girl.name}"))
                                {
                                    void defaultStyleExpansion(ExpandedStyleDefinition expansion, int index)
                                    {
                                        expansion.IsNSFW = false;
                                        expansion.IsPurchased = index > 6;
                                        expansion.IsCodeUnlocked = index == 6;
                                    }

                                    var expansion = ExpandedGirlDefinition.Get(girl);
                                    var id = new RelativeId(-1, girl.id);

                                    var body = new GirlBodySubDefinition(girl)
                                    {
                                        BodyName = "HuniePop 2",
                                        LocationIdToOutfitId = new(){
                                            {Locations.MassageSpa, new GirlStyleInfo() { HairstyleId = Styles.Relaxing, OutfitId = Styles.Relaxing}},
                                            {Locations.Aquarium, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity}},
                                            {Locations.SecludedCabana, new GirlStyleInfo() { HairstyleId = Styles.Relaxing, OutfitId = Styles.Relaxing}},
                                            {Locations.PoolsideBar, new GirlStyleInfo() { HairstyleId = Styles.Water, OutfitId = Styles.Water}},
                                            {Locations.GolfCourse, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity}},
                                            {Locations.CruiseShip, new GirlStyleInfo() { HairstyleId = Styles.Water, OutfitId = Styles.Water}},
                                            {Locations.RooftopLounge, new GirlStyleInfo() { HairstyleId = Styles.Romantic, OutfitId = Styles.Romantic}},
                                            {Locations.Casino, new GirlStyleInfo() { HairstyleId = Styles.Party, OutfitId = Styles.Party}},
                                            {Locations.PrivateTable, new GirlStyleInfo() { HairstyleId = Styles.Romantic, OutfitId = Styles.Romantic}},
                                            {Locations.SecretGrotto, new GirlStyleInfo() { HairstyleId = Styles.Water, OutfitId = Styles.Water}},
                                            {Locations.RoyalSuite, new GirlStyleInfo() { HairstyleId = Styles.Sexy, OutfitId = Styles.Sexy}},
                                            {Locations.AirplaneBathroom, new GirlStyleInfo() { HairstyleId = Styles.Activity, OutfitId = Styles.Activity}},
                                            {Locations.OuterSpace, new GirlStyleInfo() { HairstyleId = Styles.Sexy, OutfitId = Styles.Sexy}},
                                        }
                                    };
                                    expansion.Bodies.Add(new RelativeId(-1, 0), body);

                                    expansion.DialogTriggerIndex = girl.id;

                                    ModInterface.Log.LogInfo($"{girl.parts.Count} parts");
                                    MapRelativeIdRange(body.PartIdToIndex, body.PartIndexToId, girl.parts.Count);

                                    ModInterface.Log.LogInfo($"{girl.expressions.Count} expressions");
                                    MapRelativeIdRange(expansion.ExpressionIdToIndex, expansion.ExpressionIndexToId, girl.expressions.Count);

                                    ModInterface.Log.LogInfo($"{girl.hairstyles.Count} hairstyles");
                                    MapRelativeIdRange(expansion.HairstyleIdToIndex, expansion.HairstyleIndexToId, girl.hairstyles.Count);

                                    int i = 0;
                                    var hairShowingSpecials = new List<RelativeId>();
                                    foreach (var hairstyle in girl.hairstyles)
                                    {
                                        var hairstyleId = expansion.HairstyleIndexToId[i];
                                        var hairstyleExpansion = hairstyle.Expansion();
                                        defaultStyleExpansion(hairstyleExpansion, i++);

                                        if (!hairstyle.hideSpecials)
                                        {
                                            hairShowingSpecials.Add(hairstyleId);
                                        }
                                    }

                                    ModInterface.Log.LogInfo($"{girl.outfits.Count} outfits");
                                    MapRelativeIdRange(expansion.OutfitIdToIndex, expansion.OutfitIndexToId, girl.outfits.Count);
                                    i = 0;
                                    girl.outfits.ForEach(x => defaultStyleExpansion(x.Expansion(), i++));

                                    if (id == Girls.KyuId)
                                    {
                                        body.BackPos = girl.specialEffectOffset;
                                    }
                                    else
                                    {
                                        body.HeadPos = girl.specialEffectOffset;
                                    }

                                    ModInterface.Log.LogInfo($"{girl.specialParts.Count} special parts");
                                    MapRelativeIdRange(body.SpecialPartIdToIndex, body.SpecialPartIndexToId, girl.specialParts.Count);
                                    foreach (var part in girl.specialParts)
                                    {
                                        part.Expansion().RequiredHairstyles = hairShowingSpecials.ToList();
                                    }
                                }
                            }
                        }

                        ModInterface.Log.LogInfo("pairs");
                        foreach (var def in girlPairDataDict.Values)
                        {
                            var id = new RelativeId(-1, def.id);
                            var expansion = ExpandedGirlPairDefinition.Get(id);

                            expansion.PairStyle = new PairStyleInfo()
                            {
                                MeetingGirlOne = new GirlStyleInfo()
                                {
                                    HairstyleId = new RelativeId(-1, (int)def.meetingStyleTypeOne),
                                    OutfitId = new RelativeId(-1, (int)def.meetingStyleTypeOne)
                                },
                                MeetingGirlTwo = new GirlStyleInfo()
                                {
                                    HairstyleId = new RelativeId(-1, (int)def.meetingStyleTypeTwo),
                                    OutfitId = new RelativeId(-1, (int)def.meetingStyleTypeTwo)
                                },
                                SexGirlOne = new GirlStyleInfo()
                                {
                                    HairstyleId = new RelativeId(-1, (int)def.sexStyleTypeOne),
                                    OutfitId = new RelativeId(-1, (int)def.sexStyleTypeOne)
                                },
                                SexGirlTwo = new GirlStyleInfo()
                                {
                                    HairstyleId = new RelativeId(-1, (int)def.sexStyleTypeTwo),
                                    OutfitId = new RelativeId(-1, (int)def.sexStyleTypeTwo)
                                }
                            };
                        }

                        ModInterface.Log.LogInfo("locations");
                        foreach (var def in locationDataDict.Values)
                        {
                            var id = new RelativeId(-1, def.id);
                            var expansion = ExpandedLocationDefinition.Get(id);

                            if (def.locationType == LocationType.DATE && !_specialDateLocationIds.Contains(def.id))
                            {
                                expansion.AllowNormal = true;
                                expansion.PostBoss = false;
                                expansion.AllowNonStop = true;
                            }

                            if (_locationIdToDateTime.TryGetValue(def.id, out var time))
                            {
                                expansion.DateTimes ??= new List<ClockDaytimeType>();
                                expansion.DateTimes.Add(time);
                            }
                        }

                        using (ModInterface.Log.MakeIndent("Questions"))
                        {
                            foreach (var def in questionDataDict.Values)
                            {
                                var expansion = def.Expansion();
                                MapRelativeIdRange(expansion.AnswerIdToIndex, expansion.AnswerIndexToId, def.questionAnswers.Count);
                            }
                        }
                    }

                    ModInterface.Log.LogInfo("loading internal assets");
                    ModInterface.Log.IncreaseIndent();
                    foreach (var entry in abilityDataDict) { assetProvider.Load(entry.Value); }
                    foreach (var entry in cutsceneDataDict) { assetProvider.Load(entry.Value); }
                    foreach (var entry in dialogTriggerDataDict) { assetProvider.Load(entry.Value); }
                    foreach (var entry in energyDataDict) { assetProvider.Load(entry.Value); }
                    foreach (var entry in girlDataDict) { assetProvider.Load(entry.Value); }
                    foreach (var entry in itemDataDict) { assetProvider.Load(entry.Value); }
                    foreach (var entry in locationDataDict) { assetProvider.Load(entry.Value); }
                    foreach (var entry in photoDataDict) { assetProvider.Load(entry.Value); }
                    foreach (var entry in tokenDataDict) { assetProvider.Load(entry.Value); }
                    ModInterface.Log.DecreaseIndent();

                    // dev output default mods
                    if (_isDevMode)
                    {
                        try
                        {
                            ModInterface.Log.LogInfo("Generating DefaultData.cs");
                            ModInterface.Log.IncreaseIndent();
                            Hp2UiSonUtility.MakeDefaultDataDotCs(gameData, _defaultDataPath);
                            ModInterface.Log.DecreaseIndent();

                            ModInterface.Log.LogInfo("Generating Dev Files");
                            ModInterface.Log.IncreaseIndent();
                            SaveDataMods(abilityDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new AbilityDataMod(x.Value, assetProvider))), nameof(AbilityDataMod));
                            SaveDataMods(ailmentDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new AilmentDataMod(x.Value))), nameof(AilmentDataMod));
                            SaveDataMods(codeDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new CodeDataMod(x.Value))), nameof(CodeDataMod));
                            SaveDataMods(cutsceneDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new CutsceneDataMod(x.Value, assetProvider))), nameof(CutsceneDataMod));
                            SaveDataMods(dialogTriggerDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new DialogTriggerDataMod(x.Value, assetProvider))), nameof(DialogTriggerDataMod));
                            SaveDataMods(dlcDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new DlcDataMod(x.Value))), nameof(DlcDataMod));
                            SaveDataMods(energyDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new EnergyDataMod(x.Value, assetProvider))), nameof(EnergyDataMod));

                            var girlDts = dialogTriggerDataDict.Values.Where(x => IsGirlDialogTrigger(x));
                            SaveDataMods(girlDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new GirlDataMod(x.Value, assetProvider, girlDts))), nameof(GirlDataMod));

                            SaveDataMods(girlPairDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new GirlPairDataMod(x.Value))), nameof(GirlPairDataMod));
                            SaveDataMods(itemDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new ItemDataMod(x.Value, assetProvider))), nameof(ItemDataMod));
                            SaveDataMods(locationDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new LocationDataMod(x.Value, assetProvider))), nameof(LocationDataMod));
                            SaveDataMods(photoDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new PhotoDataMod(x.Value, assetProvider))), nameof(PhotoDataMod));
                            SaveDataMods(questionDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new QuestionDataMod(x.Value))), nameof(QuestionDataMod));
                            SaveDataMods(tokenDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new TokenDataMod(x.Value, assetProvider))), nameof(TokenDataMod));
                            ModInterface.Log.DecreaseIndent();

                            ModInterface.Log.LogInfo("Generating Prefabs File");
                            ModInterface.Log.IncreaseIndent();
                            assetProvider.SaveToFolder(Path.Combine(_defaultDataDir, "InternalData"));
                            ModInterface.Log.DecreaseIndent();
                        }
                        catch (Exception e)
                        {
                            ModInterface.Log.LogInfo($"Failed generating dev data: {e}");
                        }
                    }

                    // grab data mods
                    ModInterface.Log.LogInfo("grabbing data mods from the mod interface");
                    ModInterface.Log.IncreaseIndent();

                    var abilityDataMods = ModInterface.AbilityDataMods;
                    var ailmentDataMods = ModInterface.AilmentDataMods;
                    var codeDataMods = ModInterface.CodeDataMods;
                    var cutsceneDataMods = ModInterface.CutsceneDataMods;
                    var dialogTriggerDataMods = ModInterface.DialogTriggerDataMods;
                    var dlcDataMods = ModInterface.DlcDataMods;
                    var energyDataMods = ModInterface.EnergyDataMods;
                    var girlDataMods = ModInterface.GirlDataMods;
                    var girlPairDataMods = ModInterface.GirlPairDataMods;
                    var itemDataMods = ModInterface.ItemDataMods;
                    var locationDataMods = ModInterface.LocationDataMods;
                    var photoDataMods = ModInterface.PhotoDataMods;
                    var questionDataMods = ModInterface.QuestionDataMods;
                    var tokenDataMods = ModInterface.TokenDataMods;

                    var GirlToBodyToMods = new Dictionary<RelativeId, Dictionary<RelativeId, BodyData>>();
                    var dialogLineModsByIdByDialogTriggerByGirlId = new Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, List<IDialogLineDataMod>>>>();

                    foreach (var girlMod in girlDataMods)
                    {
                        var bodyDataDict = GirlToBodyToMods.GetOrNew(girlMod.Id);
                        if (girlMod.GetBodyMods() is not null and var bodyMods)
                        {
                            foreach (var bodyMod in bodyMods)
                            {
                                var bodyData = bodyDataDict.GetOrNew(bodyMod.Id);
                                bodyData.bodyMods.Add(bodyMod);
                                AddGirlSubMods(bodyMod.GetPartDataMods(), bodyData.partMods);
                                AddGirlSubMods(bodyMod.GetExpressions(), bodyData.expressionMods);
                                AddGirlSubMods(bodyMod.GetOutfits(), bodyData.outfitMods);
                                AddGirlSubMods(bodyMod.GetHairstyles(), bodyData.hairstyleMods);
                                AddGirlSubMods(bodyMod.GetSpecialPartMods(), bodyData.specialPartMods);

                                //parts have mirrors and alts, only 1 deep
                                var subParts = bodyData.partMods.SelectMany(x => x.Value).SelectMany(x => x.GetPartDataMods()).Where(x => x != null).ToArray();
                                if (subParts.Any())
                                {
                                    foreach (var subMod in subParts)
                                    {
                                        bodyData.partMods.GetOrNew(subMod.Id).Add(subMod);
                                    }
                                }
                            }
                        }

                        var linesByDialogTriggerId = girlMod.GetLinesByDialogTriggerId();

                        if (linesByDialogTriggerId != null)
                        {
                            if (!dialogLineModsByIdByDialogTriggerByGirlId.ContainsKey(girlMod.Id))
                            {
                                dialogLineModsByIdByDialogTriggerByGirlId.Add(girlMod.Id, new());
                            }

                            foreach (var dtId_Lines in linesByDialogTriggerId)
                            {
                                if (!dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id].ContainsKey(dtId_Lines.Item1))
                                {
                                    dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id].Add(dtId_Lines.Item1, new());
                                }

                                foreach (var lineMod in dtId_Lines.Item2)
                                {
                                    if (!dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtId_Lines.Item1].ContainsKey(lineMod.Id))
                                    {
                                        dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtId_Lines.Item1].Add(lineMod.Id, new());
                                    }

                                    dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtId_Lines.Item1][lineMod.Id].Add(lineMod);
                                }
                            }
                        }
                    }

                    ModInterface.Log.DecreaseIndent();

                    // create and register missing empty mods for used ids, all need to exist before any are setup because they reference one another
                    ModInterface.Log.LogInfo("creating data for new ids");
                    ModInterface.Log.IncreaseIndent();
                    PreProcess(abilityDataDict, abilityDataMods, GameDataType.Ability, assetProvider);
                    PreProcess(ailmentDataDict, ailmentDataMods, GameDataType.Ailment, assetProvider);
                    PreProcess(codeDataDict, codeDataMods, GameDataType.Code, assetProvider);
                    PreProcess(cutsceneDataDict, cutsceneDataMods, GameDataType.Cutscene, assetProvider);
                    PreProcess(dialogTriggerDataDict, dialogTriggerDataMods, GameDataType.DialogTrigger, assetProvider);
                    PreProcess(dlcDataDict, dlcDataMods, GameDataType.Dlc, assetProvider);
                    PreProcess(energyDataDict, energyDataMods, GameDataType.Energy, assetProvider);
                    PreProcess(girlDataDict, girlDataMods, GameDataType.Girl, assetProvider);
                    PreProcess(girlPairDataDict, girlPairDataMods, GameDataType.GirlPair, assetProvider);
                    PreProcess(itemDataDict, itemDataMods, GameDataType.Item, assetProvider);
                    PreProcess(locationDataDict, locationDataMods, GameDataType.Location, assetProvider);
                    PreProcess(photoDataDict, photoDataMods, GameDataType.Photo, assetProvider);
                    PreProcess(questionDataDict, questionDataMods, GameDataType.Question, assetProvider);
                    PreProcess(tokenDataDict, tokenDataMods, GameDataType.Token, assetProvider);

                    using (ModInterface.Log.MakeIndent("Creating and registering data for new ids for sub mods"))
                    {
                        foreach (var girlId_BodyToMods in GirlToBodyToMods)
                        {
                            var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_BodyToMods.Key)];
                            var expansion = ExpandedGirlDefinition.Get(girlDef);

                            using (ModInterface.Log.MakeIndent($"{girlId_BodyToMods.Key} {girlDef.girlName}"))
                            {
                                using (ModInterface.Log.MakeIndent("Bodies"))
                                {
                                    foreach (var id_bodyMods in girlId_BodyToMods.Value)
                                    {
                                        var body = expansion.Bodies.GetOrNew(id_bodyMods.Key);

                                        using (ModInterface.Log.MakeIndent($"body {id_bodyMods.Key}, total bodies {expansion.Bodies.Count}"))
                                        {
                                            using (ModInterface.Log.MakeIndent("parts"))
                                            {
                                                RegisterUnregisteredIds(body.PartIdToIndex,
                                                    body.PartIndexToId,
                                                    body.Parts.Count,
                                                    id_bodyMods.Value.partMods.Select(x => x.Key).Distinct(),
                                                    body.Parts);
                                            }

                                            using (ModInterface.Log.MakeIndent("expressions"))
                                            {
                                                RegisterUnregisteredIds(expansion.ExpressionIdToIndex,
                                                    expansion.ExpressionIndexToId,
                                                    body.Expressions.Count,
                                                    id_bodyMods.Value.expressionMods.Select(x => x.Key).Distinct(),
                                                    body.Expressions);
                                            }

                                            using (ModInterface.Log.MakeIndent("outfits"))
                                            {
                                                RegisterUnregisteredIds(expansion.OutfitIdToIndex,
                                                    expansion.OutfitIndexToId,
                                                    body.Outfits.Count,
                                                    id_bodyMods.Value.outfitMods.Select(x => x.Key).Distinct(),
                                                    body.Outfits);
                                            }

                                            using (ModInterface.Log.MakeIndent("hairstyles"))
                                            {
                                                RegisterUnregisteredIds(expansion.HairstyleIdToIndex,
                                                    expansion.HairstyleIndexToId,
                                                    body.Hairstyles.Count,
                                                    id_bodyMods.Value.hairstyleMods.Select(x => x.Key).Distinct(),
                                                    body.Hairstyles);
                                            }

                                            using (ModInterface.Log.MakeIndent("specials"))
                                            {
                                                RegisterUnregisteredIds(body.SpecialPartIdToIndex,
                                                    body.SpecialPartIndexToId,
                                                    body.SpecialParts.Count,
                                                    id_bodyMods.Value.specialPartMods.Select(x => x.Key).Distinct(),
                                                    body.SpecialParts);
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var id_body in expansion.Bodies)
                            {
                                var bodyMods = girlId_BodyToMods.Value[id_body.Key];

                                AddMissingIndexedDefinitions(bodyMods.expressionMods.Keys.Select(x => expansion.ExpressionIdToIndex[x]), id_body.Value.Expressions, expansion.ExpressionIdToIndex.Count);
                                AddMissingIndexedDefinitions(bodyMods.hairstyleMods.Keys.Select(x => expansion.HairstyleIdToIndex[x]), id_body.Value.Hairstyles, expansion.HairstyleIdToIndex.Count);
                                AddMissingIndexedDefinitions(bodyMods.outfitMods.Keys.Select(x => expansion.OutfitIdToIndex[x]), id_body.Value.Outfits, expansion.OutfitIdToIndex.Count);
                            }
                        }

                        using (ModInterface.Log.MakeIndent("dialog triggers"))
                        {
                            using (ModInterface.Log.MakeIndent("Girl dt indexes"))
                            {
                                int nextIndex = DEFAULT_DT_SET_COUNT;
                                foreach (var girlId in girlDataMods.Select(x => x.Id).Distinct())
                                {
                                    var girlExpansion = ExpandedGirlDefinition.Get(girlId);

                                    if (girlExpansion.DialogTriggerIndex == -1)
                                    {
                                        girlExpansion.DialogTriggerIndex = nextIndex;
                                        nextIndex++;
                                    }
                                }
                            }

                            using (ModInterface.Log.MakeIndent("line sets"))
                            {
                                foreach (var dt in dialogTriggerDataDict.Values.Where(x => IsGirlDialogTrigger(x)))
                                {
                                    var expansion = dt.Expansion();

                                    if (!expansion.GirlIdToLineIndexToLineId.ContainsKey(RelativeId.Default))
                                    {
                                        dt.dialogLineSets.Add(new DialogTriggerLineSet());

                                        expansion.GirlIdToLineIdToLineIndex[RelativeId.Default] = new() { { RelativeId.Default, -1 } };
                                        expansion.GirlIdToLineIndexToLineId[RelativeId.Default] = new() { { -1, RelativeId.Default } };
                                    }

                                    foreach (var girlRuntime in girlDataDict.Keys)
                                    {
                                        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girlRuntime);

                                        if (!expansion.GirlIdToLineIndexToLineId.ContainsKey(girlId))
                                        {
                                            expansion.GirlIdToLineIndexToLineId[girlId] = new();
                                            expansion.GirlIdToLineIdToLineIndex[girlId] = new();
                                            dt.dialogLineSets.Add(new DialogTriggerLineSet());
                                        }
                                    }
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girl lines"))
                        {
                            foreach (var girlId_DialogLineModsByIdByDialogTrigger in dialogLineModsByIdByDialogTriggerByGirlId)
                            {
                                using (ModInterface.Log.MakeIndent($"{girlId_DialogLineModsByIdByDialogTrigger.Key}"))
                                {
                                    var girlExpansion = ExpandedGirlDefinition.Get(girlId_DialogLineModsByIdByDialogTrigger.Key);

                                    foreach (var dialogTriggerId_DialogLineModsById in girlId_DialogLineModsByIdByDialogTrigger.Value)
                                    {
                                        var dialogTrigger = gameDataProvider.GetDialogTrigger(dialogTriggerId_DialogLineModsById.Key);

                                        using (ModInterface.Log.MakeIndent($"dt {dialogTriggerId_DialogLineModsById.Key} with {dialogTrigger.dialogLineSets.Count} line sets"))
                                        {
                                            var dialogTriggerSet = dialogTrigger.dialogLineSets[girlExpansion.DialogTriggerIndex];
                                            var expansion = ExpandedDialogTriggerDefinition.Get(dialogTriggerId_DialogLineModsById.Key);

                                            RegisterUnregisteredIds(expansion.GirlIdToLineIdToLineIndex[girlId_DialogLineModsByIdByDialogTrigger.Key],
                                                expansion.GirlIdToLineIndexToLineId[girlId_DialogLineModsByIdByDialogTrigger.Key],
                                                dialogTriggerSet.dialogLines.Count,
                                                dialogTriggerId_DialogLineModsById.Value.Select(x => x.Key),
                                                dialogTriggerSet.dialogLines);
                                        }
                                    }
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("Favorites"))
                        {
                            var idToMods = new Dictionary<RelativeId, List<IQuestionDataMod>>();

                            foreach (var mod in questionDataMods)
                            {
                                idToMods.GetOrNew(mod.Id).Add(mod);
                            }


                            foreach (var id_mods in idToMods)
                            {
                                using (ModInterface.Log.MakeIndent($"Question {id_mods.Key}"))
                                {
                                    var question = questionDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Question, id_mods.Key)];
                                    var expansion = ExpandedQuestionDefinition.Get(id_mods.Key);
                                    var answers = id_mods.Value.SelectManyNN(x => x.GetAnswerIds()).Distinct();

                                    var index = question.questionAnswers.Count;
                                    foreach (var id in answers.Where(x => !expansion.AnswerIdToIndex.ContainsKey(x)))
                                    {
                                        ModInterface.Log.LogInfo($"Adding answer {id} at index {index}");
                                        expansion.AnswerIdToIndex[id] = index;
                                        expansion.AnswerIndexToId[index++] = id;
                                        question.questionAnswers.Add(null);
                                    }
                                }
                            }
                        }
                    }

                    ModInterface.Log.DecreaseIndent();

                    //Requesting internal data
                    using (ModInterface.Log.MakeIndent("cataloging internal assets"))
                    {
                        assetProvider.FulfilInternalAssetRequests();
                    }

                    //setup defs
                    using (ModInterface.Log.MakeIndent("applying mod data to game data"))
                    {
                        using (ModInterface.Log.MakeIndent("Abilities"))
                        {
                            SetData(abilityDataDict, abilityDataMods, gameDataProvider, assetProvider, GameDataType.Ability);
                        }

                        using (ModInterface.Log.MakeIndent("Ailments"))
                        {
                            SetData(ailmentDataDict, ailmentDataMods, gameDataProvider, assetProvider, GameDataType.Ailment);
                        }

                        using (ModInterface.Log.MakeIndent("Codes"))
                        {
                            SetData(codeDataDict, codeDataMods, gameDataProvider, assetProvider, GameDataType.Code);
                        }

                        using (ModInterface.Log.MakeIndent("Cutscenes"))
                        {
                            SetData(cutsceneDataDict, cutsceneDataMods, gameDataProvider, assetProvider, GameDataType.Cutscene);
                        }

                        using (ModInterface.Log.MakeIndent("Dialog triggers"))
                        {
                            SetData(dialogTriggerDataDict, dialogTriggerDataMods, gameDataProvider, assetProvider, GameDataType.DialogTrigger);
                        }

                        using (ModInterface.Log.MakeIndent("Dlcs"))
                        {
                            SetData(dlcDataDict, dlcDataMods, gameDataProvider, assetProvider, GameDataType.Dlc);
                        }

                        using (ModInterface.Log.MakeIndent("Energy"))
                        {
                            SetData(energyDataDict, energyDataMods, gameDataProvider, assetProvider, GameDataType.Energy);
                        }

                        using (ModInterface.Log.MakeIndent("Girls"))
                        {
                            SetData(girlDataDict, girlDataMods, gameDataProvider, assetProvider, GameDataType.Girl);

                            foreach (var girlId_BodyToMods in GirlToBodyToMods)
                            {
                                var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_BodyToMods.Key)];
                                var expansion = ExpandedGirlDefinition.Get(girlId_BodyToMods.Key);

                                using (ModInterface.Log.MakeIndent($"Girl {girlId_BodyToMods.Key} - {girl.girlName}"))
                                {
                                    using (ModInterface.Log.MakeIndent($"Bodies"))
                                    {
                                        foreach (var bodyId_Mods in girlId_BodyToMods.Value)
                                        {
                                            var body = expansion.Bodies[bodyId_Mods.Key];
                                            using (ModInterface.Log.MakeIndent($"Body {bodyId_Mods.Key} - {girl.girlName}"))
                                            {
                                                SetSubDefMods(bodyId_Mods.Value.partMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, body.GetPart);
                                                SetSubDefMods(bodyId_Mods.Value.outfitMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, (x) => expansion.GetOutfit(body, x));
                                                SetSubDefMods(bodyId_Mods.Value.hairstyleMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, (x) => expansion.GetHairstyle(body, x));
                                                SetSubDefMods(bodyId_Mods.Value.specialPartMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, body.GetSpecialPart);
                                                SetSubDefMods(bodyId_Mods.Value.expressionMods, gameDataProvider, assetProvider, girlId_BodyToMods.Key, body, (x) => expansion.GetExpression(body, x));

                                                foreach (var mod in bodyId_Mods.Value.bodyMods)
                                                {
                                                    mod.SetData(body, gameDataProvider, assetProvider, girlId_BodyToMods.Key);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girl lines"))
                        {
                            foreach (var girlId_ModsByIdByDialogTrigger in dialogLineModsByIdByDialogTriggerByGirlId)
                            {
                                foreach (var dialogTriggerId_ModsById in girlId_ModsByIdByDialogTrigger.Value)
                                {
                                    var dialogTrigger = dialogTriggerDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.DialogTrigger, dialogTriggerId_ModsById.Key)];
                                    var dtExpansion = ExpandedDialogTriggerDefinition.Get(dialogTriggerId_ModsById.Key);

                                    foreach (var lineId_Mods in dialogTriggerId_ModsById.Value)
                                    {
                                        if (dtExpansion.TryGetLine(dialogTrigger, girlId_ModsByIdByDialogTrigger.Key, lineId_Mods.Key, out var line))
                                        {
                                            foreach (var mod in lineId_Mods.Value.OrderBy(x => x.LoadPriority))
                                            {
                                                mod.SetData(line, gameDataProvider, assetProvider);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girl pairs"))
                        {
                            SetData(girlPairDataDict, girlPairDataMods, gameDataProvider, assetProvider, GameDataType.GirlPair);

                            using (ModInterface.Log.MakeIndent("styles"))
                            {
                                foreach (var pairMod in girlPairDataMods)
                                {
                                    var expansion = ExpandedGirlPairDefinition.Get(pairMod.Id);
                                    expansion.PairStyle = pairMod.GetStyles();
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("items"))
                        {
                            SetData(itemDataDict, itemDataMods, gameDataProvider, assetProvider, GameDataType.Item);
                        }

                        using (ModInterface.Log.MakeIndent("locations"))
                        {
                            SetData(locationDataDict, locationDataMods, gameDataProvider, assetProvider, GameDataType.Location);
                        }

                        using (ModInterface.Log.MakeIndent("photos"))
                        {
                            SetData(photoDataDict, photoDataMods, gameDataProvider, assetProvider, GameDataType.Photo);
                        }

                        using (ModInterface.Log.MakeIndent("questions"))
                        {
                            SetData(questionDataDict, questionDataMods, gameDataProvider, assetProvider, GameDataType.Question);
                        }

                        using (ModInterface.Log.MakeIndent("tokens"))
                        {
                            SetData(tokenDataDict, tokenDataMods, gameDataProvider, assetProvider, GameDataType.Token);
                        }
                    }

                    using (ModInterface.Log.MakeIndent("registering functional mods"))
                    {
                        ModInterface.Log.LogInfo("ailments");
                        ModInterface.Data.RegisterFunctionalAilments(ailmentDataMods.OfType<IFunctionalAilmentDataMod>());
                    }
                }

                using (ModInterface.Log.MakeIndent("verifying gamedata integrity"))
                {
                    var lailani = ModInterface.GameData.GetGirl(Girls.LailaniId);

                    using (ModInterface.Log.MakeIndent("girls"))
                    {
                        foreach (var girl in Game.Data.Girls.GetAll())
                        {
                            girl.badFoodTypes ??= new();
                            if (!girl.badFoodTypes.Any())
                            {
                                girl.badFoodTypes.Add((ItemFoodType)(-1));
                            }

                            if (girl.herQuestions.IsNullOrEmpty())
                            {
                                girl.herQuestions = lailani.herQuestions;
                            }
                            girl.uniqueItemDefs ??= new();
                            girl.shoesItemDefs ??= new();
                            girl.baggageItemDefs ??= new();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModInterface.Log.LogError($"{e}");
            }

            // using (ModInterface.Log.MakeIndent("-DEBUG-"))
            // {
            //     using (ModInterface.Log.MakeIndent("All questions:"))
            //     {
            //         foreach (var question in Game.Data.Questions.GetAll())
            //         {
            //             ModInterface.Log.LogInfo($"{question.id} {question.questionName} - {string.Join(", ", question.questionAnswers)}");
            //         }
            //     }

            //     using (ModInterface.Log.MakeIndent("All NSFW outfits:"))
            //     {
            //         foreach (var girl in Game.Data.Girls.GetAll())
            //         {
            //             var girlExpansion = girl.Expansion();
            //             ModInterface.Log.LogInfo($"{girl.girlName}, NSFW outfit ids: {string.Join(", ", girlExpansion.OutfitIdToIndex.Keys.Select(x => (x, girlExpansion.GetOutfit(x))).Where(x => x.Item2.Expansion().IsNSFW).Select(x => x.Item1))}");
            //         }
            //     }

            //     using (ModInterface.Log.MakeIndent("All location arrive logic:"))
            //     {
            //         foreach (var location in Game.Data.Locations.GetAll())
            //         {
            //             using (ModInterface.Log.MakeIndent(location.locationName))
            //             {
            //                 using (ModInterface.Log.MakeIndent("Arrival"))
            //                 {
            //                     foreach (var bundle in location.arriveBundleList)
            //                     {
            //                         GameDataLogUtility.LogLogicBundle(bundle);
            //                     }
            //                 }

            //                 using (ModInterface.Log.MakeIndent("Departure"))
            //                 {
            //                     foreach (var bundle in location.departBundleList)
            //                     {
            //                         GameDataLogUtility.LogLogicBundle(bundle);
            //                     }
            //                 }
            //             }
            //         }
            //     }
            // }
        }

        #region Dev

        // there are inconsistencies in how dialog triggers are handled, yay.
        // Some are indexed by girl id, others are made specifically for the hub, and there's no easy way to differentiate them
        // so here's just a manual list, maybe add this to default data later. If someone wants to mod these they can make a dll I'm not gonna support it via json
        // greeting = 49
        // valediction = 50
        // nap = 51
        // wakeup = 52
        // wing check = 53
        // pre nymph = 54
        // pos nymph = 55
        // nonstop = 56
        // wardrobe = 57
        // album = 58
        private static bool IsGirlDialogTrigger(DialogTriggerDefinition dt) => dt.id < 49 || dt.id > 58;

        private static void SaveDataMods(IEnumerable<KeyValuePair<string, DataMod>> mods, string name)
        {
            ModInterface.Log.LogInfo($"Dev: saving default {name}");

            var folderPath = Path.Combine(_defaultDataDir, name);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            ModInterface.Log.IncreaseIndent();
            foreach (var mod in mods)
            {
                var filePath = Path.Combine(folderPath, $"{mod.Key}.json");

                ModInterface.Log.LogInfo($"Dev: Saving {mod.Key} to {filePath}");

                File.WriteAllText(filePath, JsonConvert.SerializeObject(mod.Value, Formatting.Indented));
            }
            ModInterface.Log.DecreaseIndent();
        }

        #endregion Dev

        private static void AddGirlSubMods<T>(IEnumerable<IBodySubDataMod<T>> subMods,
            Dictionary<RelativeId, List<IBodySubDataMod<T>>> subModListsById)
        {
            if (subMods != null && subMods.Any())
            {
                foreach (var subMod in subMods.Where(x => x != null))
                {
                    var subModList = subModListsById.GetOrNew(subMod.Id);
                    subModList.Add(subMod);
                }
            }
        }

        private static Dictionary<int, T> GetDataDict<T>(GameData gameData, Type dataType, string dataName)
            => AccessTools.DeclaredField(dataType, "_definitions")
                          .GetValue(AccessTools.DeclaredField(typeof(GameData), dataName)
                          .GetValue(gameData)) as Dictionary<int, T>;

        private static void PreProcess<D>(Dictionary<int, D> dict, IEnumerable<IGameDataMod<D>> mods, GameDataType type, AssetProvider assetProvider)
            where D : Definition, new()
        {
            foreach (var mod in mods)
            {
                var runtimeId = ModInterface.Data.GetRuntimeDataId(type, mod.Id);

                if (!dict.ContainsKey(runtimeId))
                {
                    var newDef = ScriptableObject.CreateInstance<D>();
                    newDef.id = runtimeId;

                    dict.Add(runtimeId, newDef);
                }

                mod.RequestInternals(assetProvider);
            }
        }

        private static void SetData<T>(Dictionary<int, T> data,
                                       IEnumerable<IGameDataMod<T>> mods,
                                       GameDefinitionProvider gameDataProvider,
                                       AssetProvider prefabProvider,
                                       GameDataType type)
        {
            //first split into groups of the same id
            var modsByRuntimeId = new Dictionary<int, List<IGameDataMod<T>>>();
            foreach (var mod in mods)
            {
                var runtimeId = ModInterface.Data.GetRuntimeDataId(type, mod.Id);
                if (!modsByRuntimeId.ContainsKey(runtimeId))
                {
                    modsByRuntimeId.Add(runtimeId, new List<IGameDataMod<T>>() { mod });
                }
                else
                {
                    modsByRuntimeId[runtimeId].Add(mod);
                }
            }

            // then set the data
            foreach (var entry in modsByRuntimeId)
            {
                // order by the load priority
                foreach (var mod in entry.Value.OrderBy(x => x.LoadPriority))
                {
                    mod.SetData(data[entry.Key], gameDataProvider, prefabProvider);
                }
            }
        }

        private static void MapRelativeIdRange(IDictionary<RelativeId, int> idToIndex, IDictionary<int, RelativeId> indexToId, int count, int startingIndex = 0)
        {
            for (int i = startingIndex; i < startingIndex + count; i++)
            {
                idToIndex[new RelativeId(-1, i)] = i;
                indexToId[i] = new RelativeId(-1, i);
            }
        }

        private static void RegisterUnregisteredIds<T>(IDictionary<RelativeId, int> idToIndex,
            IDictionary<int, RelativeId> indexToId,
            int startingIndex,
            IEnumerable<RelativeId> ids,
            List<T> gameData)
        where T : class, new()
        {
            foreach (var id in ids.Where(x => !idToIndex.ContainsKey(x)))
            {
                idToIndex[id] = startingIndex;
                indexToId[startingIndex++] = id;
                var newData = new T();
                gameData.Add(newData);
                //ModInterface.Log.LogInfo($"New GameData for id {id}");
            }
        }

        private static void SetSubDefMods<T>(Dictionary<RelativeId, List<IBodySubDataMod<T>>> idToMods,
            GameDefinitionProvider gameData,
            AssetProvider assetProvider,
            RelativeId girlId,
            GirlBodySubDefinition bodyDef,
            Func<RelativeId, T> getTarget)
        {
            using (ModInterface.Log.MakeIndent(typeof(T).Name))
            {
                foreach (var id_mods in idToMods)
                {
                    using (ModInterface.Log.MakeIndent(id_mods.Key.ToString()))
                    {
                        var target = getTarget.Invoke(id_mods.Key);

                        foreach (var mod in id_mods.Value.OrderBy(x => x.LoadPriority))
                        {
                            mod.SetData(target, gameData, assetProvider, girlId, bodyDef);
                        }
                    }
                }
            }
        }

        private static void AddMissingIndexedDefinitions<T>(IEnumerable<int> indexes, List<T> definitions, int total)
        where T : class, new()
        {
            foreach (var index in indexes)
            {
                if (index > definitions.Count - 1)
                {
                    for (int i = index - definitions.Count; i > 0; i--)
                    {
                        definitions.Add(null);
                    }

                    definitions.Add(new());
                }
                else
                {
                    definitions[index] = new();
                }
            }

            while (definitions.Count > total)
            {
                definitions.Add(null);
            }
        }
    }
}
