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
            23,//outerspace
            26,//airplane bathroom
        };

        public static void Mod(GameData gameData)
        {
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
                                        BodyName = "HuniePop 2"
                                    };
                                    expansion.Bodies.Add(new RelativeId(-1, 0), body);

                                    expansion.DialogTriggerIndex = girl.id;

                                    ModInterface.Log.LogInfo($"{girl.parts.Count} parts");
                                    MapRelativeIdRange(expansion.PartIdToIndex, expansion.PartIndexToId, girl.parts.Count);

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
                                    MapRelativeIdRange(expansion.SpecialPartIdToIndex, expansion.SpecialPartIndexToId, girl.specialParts.Count);
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
                            expansion.GirlIdToLocationStyleInfo ??= new Dictionary<RelativeId, GirlStyleInfo>();

                            foreach (var girl in girlDataDict.Values)
                            {
                                var girlId = new RelativeId(-1, girl.id);

                                expansion.GirlIdToLocationStyleInfo[girlId] = new GirlStyleInfo()
                                {
                                    HairstyleId = new RelativeId(-1, (int)def.dateGirlStyleType),
                                    OutfitId = new RelativeId(-1, (int)def.dateGirlStyleType)
                                };
                            }

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
                            SaveDataMods(locationDataDict.Select(x => new KeyValuePair<string, DataMod>(x.Value.name, new LocationDataMod(x.Value, girlDataDict.Values, assetProvider))), nameof(LocationDataMod));
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

                    var partModsByIdByGirl = new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<GirlPartSubDefinition>>>>();
                    var expressionModsByIdByGirl = new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<GirlExpressionSubDefinition>>>>();
                    var outfitModsByIdByGirl = new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<GirlOutfitSubDefinition>>>>();
                    var hairstyleModsByIdByGirl = new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<GirlHairstyleSubDefinition>>>>();
                    var specialPartModsByIdByGirl = new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<GirlSpecialPartSubDefinition>>>>();
                    var dialogLineModsByIdByDialogTriggerByGirlId = new Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<DialogLine>>>>>();

                    foreach (var girlMod in girlDataMods)
                    {
                        var partListsById = partModsByIdByGirl.GetOrNew(girlMod.Id);

                        var bodyMods = girlMod.GetBodyMods();
                        if (bodyMods != null)
                        {
                            AddGirlSubMods(bodyMods.SelectManyNN(x => x.GetExpressions()), expressionModsByIdByGirl.GetOrNew(girlMod.Id), partListsById);
                            AddGirlSubMods(bodyMods.SelectManyNN(x => x.GetOutfits()), outfitModsByIdByGirl.GetOrNew(girlMod.Id), partListsById);
                            AddGirlSubMods(bodyMods.SelectManyNN(x => x.GetHairstyles()), hairstyleModsByIdByGirl.GetOrNew(girlMod.Id), partListsById);
                            AddGirlSubMods(bodyMods.SelectManyNN(x => x.GetSpecialPartMods()), specialPartModsByIdByGirl.GetOrNew(girlMod.Id), partListsById);
                        }

                        //parts have mirrors and alts, only 1 deep
                        var subParts = partListsById.SelectMany(x => x.Value).SelectMany(x => x.GetPartDataMods()).Where(x => x != null).ToArray();
                        if (subParts.Any())
                        {
                            foreach (var subMod in subParts)
                            {
                                partListsById.GetOrNew(subMod.Id).Add(subMod);
                            }
                        }

                        var parts = girlMod.GetBodyMods()?.SelectManyNN(x => x.GetPartDataMods());
                        if (parts != null
                            && parts.Any())
                        {
                            foreach (var part in parts.Where(x => x != null))
                            {
                                partListsById.GetOrNew(part.Id).Add(part);
                            }
                        }

                        var linesByDialogTriggerId = girlMod.GetLinesByDialogTriggerId();

                        if (linesByDialogTriggerId != null)
                        {
                            if (!dialogLineModsByIdByDialogTriggerByGirlId.ContainsKey(girlMod.Id))
                            {
                                dialogLineModsByIdByDialogTriggerByGirlId.Add(girlMod.Id, new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<DialogLine>>>>());
                            }

                            foreach (var dtId_Lines in linesByDialogTriggerId)
                            {
                                if (!dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id].ContainsKey(dtId_Lines.Item1))
                                {
                                    dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id].Add(dtId_Lines.Item1, new Dictionary<RelativeId, List<IGirlSubDataMod<DialogLine>>>());
                                }

                                foreach (var lineMod in dtId_Lines.Item2)
                                {
                                    if (!dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtId_Lines.Item1].ContainsKey(lineMod.Id))
                                    {
                                        dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtId_Lines.Item1].Add(lineMod.Id, new List<IGirlSubDataMod<DialogLine>>());
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

                    // for these sub mods we can create empties and load them at the same time because they don't reference one another
                    using (ModInterface.Log.MakeIndent("creating and registering data for new ids for sub mods"))
                    {
                        using (ModInterface.Log.MakeIndent("dialog trigger indexes"))
                        {
                            int nextIndex = 16;//by default each has one default (0), 12 for the normal girls, 1 for kyu, 2 for nymphojinn. 1+12+1+2=16
                            foreach (var girlId in girlDataMods.Select(x => x.Id).Distinct())
                            {
                                var girlExpansion = ExpandedGirlDefinition.Get(girlId);

                                if (girlExpansion.DialogTriggerIndex == -1)
                                {
                                    girlExpansion.DialogTriggerIndex = nextIndex;

                                    foreach (var dialogTrigger in dialogTriggerDataDict.Values)
                                    {
                                        dialogTrigger.dialogLineSets.Add(new DialogTriggerLineSet());

                                        var expansion = dialogTrigger.Expansion();
                                        expansion.GirlIdToLineIdToLineIndex[girlId] = new() { { RelativeId.Default, -1 } };
                                        expansion.GirlIdToLineIndexToLineId[girlId] = new() { { -1, RelativeId.Default } };
                                    }

                                    nextIndex++;
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girl parts"))
                        {
                            foreach (var girlId_PartModsById in partModsByIdByGirl)
                            {
                                var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_PartModsById.Key)];
                                var expansion = ExpandedGirlDefinition.Get(girlDef);

                                using (ModInterface.Log.MakeIndent($"{girlId_PartModsById.Key} {girlDef.girlName}"))
                                {
                                    RegisterUnregisteredIds(expansion.PartIdToIndex,
                                        expansion.PartIndexToId,
                                        girlDef.parts.Count,
                                        girlId_PartModsById.Value.Select(x => x.Key).Distinct(),
                                        girlDef.parts);
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girl expressions"))
                        {
                            foreach (var girlId_ExpressionModsById in expressionModsByIdByGirl)
                            {
                                var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ExpressionModsById.Key)];
                                var expansion = ExpandedGirlDefinition.Get(girlDef);

                                using (ModInterface.Log.MakeIndent($"{girlId_ExpressionModsById.Key} {girlDef.girlName}"))
                                {
                                    RegisterUnregisteredIds(expansion.ExpressionIdToIndex,
                                        expansion.ExpressionIndexToId,
                                        girlDef.expressions.Count,
                                        girlId_ExpressionModsById.Value.Select(x => x.Key).Distinct(),
                                        girlDef.expressions);
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girl outfits"))
                        {
                            foreach (var girlId_OutfitModsById in outfitModsByIdByGirl)
                            {
                                var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_OutfitModsById.Key)];
                                var expansion = ExpandedGirlDefinition.Get(girlDef);

                                using (ModInterface.Log.MakeIndent($"{girlId_OutfitModsById.Key} {girlDef.girlName}"))
                                {
                                    RegisterUnregisteredIds(expansion.OutfitIdToIndex,
                                        expansion.OutfitIndexToId,
                                        girlDef.outfits.Count,
                                        girlId_OutfitModsById.Value.Select(x => x.Key).Distinct(),
                                        girlDef.outfits);
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girl hairstyles"))
                        {
                            foreach (var girlId_HairstyleModsById in hairstyleModsByIdByGirl)
                            {
                                var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_HairstyleModsById.Key)];
                                var expansion = ExpandedGirlDefinition.Get(girlDef);

                                using (ModInterface.Log.MakeIndent($"{girlId_HairstyleModsById.Key} {girlDef.girlName}"))
                                {
                                    RegisterUnregisteredIds(expansion.HairstyleIdToIndex,
                                        expansion.HairstyleIndexToId,
                                        girlDef.hairstyles.Count,
                                        girlId_HairstyleModsById.Value.Select(x => x.Key).Distinct(),
                                        girlDef.hairstyles);
                                }
                            }
                        }

                        using (ModInterface.Log.MakeIndent("girl special parts"))
                        {
                            foreach (var girlId_SpecialPartModsById in specialPartModsByIdByGirl)
                            {
                                var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_SpecialPartModsById.Key)];
                                var expansion = ExpandedGirlDefinition.Get(girlDef);

                                using (ModInterface.Log.MakeIndent($"{girlId_SpecialPartModsById.Key} {girlDef.girlName}"))
                                {
                                    RegisterUnregisteredIds(expansion.SpecialPartIdToIndex,
                                        expansion.SpecialPartIndexToId,
                                        girlDef.specialParts.Count,
                                        girlId_SpecialPartModsById.Value.Select(x => x.Key).Distinct(),
                                        girlDef.specialParts);
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

                        //TODO: I honestly have no idea what this was for...
                        using (ModInterface.Log.MakeIndent("location slots"))
                        {
                            foreach (var location in ModInterface.Data.GetIds(GameDataType.Location).Where(x => x.SourceId != -1))
                            {
                                //TODO: do good stuff here pls
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
                        using (ModInterface.Log.MakeIndent("abilities"))
                        {
                            SetData(abilityDataDict, abilityDataMods, gameDataProvider, assetProvider, GameDataType.Ability);
                        }

                        using (ModInterface.Log.MakeIndent("ailments"))
                        {
                            SetData(ailmentDataDict, ailmentDataMods, gameDataProvider, assetProvider, GameDataType.Ailment);
                        }

                        using (ModInterface.Log.MakeIndent("codes"))
                        {
                            SetData(codeDataDict, codeDataMods, gameDataProvider, assetProvider, GameDataType.Code);
                        }

                        using (ModInterface.Log.MakeIndent("cutscenes"))
                        {
                            SetData(cutsceneDataDict, cutsceneDataMods, gameDataProvider, assetProvider, GameDataType.Cutscene);
                        }

                        using (ModInterface.Log.MakeIndent("dialog triggers"))
                        {
                            SetData(dialogTriggerDataDict, dialogTriggerDataMods, gameDataProvider, assetProvider, GameDataType.DialogTrigger);
                        }

                        using (ModInterface.Log.MakeIndent("dlcs"))
                        {
                            SetData(dlcDataDict, dlcDataMods, gameDataProvider, assetProvider, GameDataType.Dlc);
                        }

                        using (ModInterface.Log.MakeIndent("energy"))
                        {
                            SetData(energyDataDict, energyDataMods, gameDataProvider, assetProvider, GameDataType.Energy);
                        }

                        using (ModInterface.Log.MakeIndent("girls"))
                        {
                            SetData(girlDataDict, girlDataMods, gameDataProvider, assetProvider, GameDataType.Girl);

                            using (ModInterface.Log.MakeIndent("parts"))
                            {
                                foreach (var girlId_ModsByPartId in partModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByPartId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsByPartId.Key);

                                    foreach (var partId_Mods in girlId_ModsByPartId.Value)
                                    {
                                        var part = expansion.GetPart(partId_Mods.Key);

                                        foreach (var mod in partId_Mods.Value.OrderBy(x => x.LoadPriority))
                                        {
                                            mod.SetData(ref part, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByPartId.Key, girl);
                                        }
                                    }
                                }
                            }

                            using (ModInterface.Log.MakeIndent("expressions"))
                            {
                                foreach (var girlId_ModsByExpressionId in expressionModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByExpressionId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsByExpressionId.Key);

                                    foreach (var expressionId_Mods in girlId_ModsByExpressionId.Value)
                                    {
                                        var expression = expansion.GetExpression(expressionId_Mods.Key);

                                        foreach (var mod in expressionId_Mods.Value.OrderBy(x => x.LoadPriority))
                                        {
                                            mod.SetData(ref expression, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByExpressionId.Key, girl);
                                        }
                                    }
                                }
                            }

                            using (ModInterface.Log.MakeIndent("outfits"))
                            {
                                foreach (var girlId_ModsByOutfitId in outfitModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByOutfitId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsByOutfitId.Key);

                                    foreach (var outfitId_Mods in girlId_ModsByOutfitId.Value)
                                    {
                                        var outfit = expansion.GetOutfit(outfitId_Mods.Key);

                                        foreach (var mod in outfitId_Mods.Value.OrderBy(x => x.LoadPriority))
                                        {
                                            mod.SetData(ref outfit, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByOutfitId.Key, girl);
                                        }
                                    }
                                }
                            }

                            using (ModInterface.Log.MakeIndent("hairstyles"))
                            {
                                foreach (var girlId_ModsByHairstyleId in hairstyleModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByHairstyleId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsByHairstyleId.Key);

                                    foreach (var hairstyleId_Mods in girlId_ModsByHairstyleId.Value)
                                    {
                                        var hairstyle = expansion.GetHairstyle(hairstyleId_Mods.Key);

                                        foreach (var mod in hairstyleId_Mods.Value.OrderBy(x => x.LoadPriority))
                                        {
                                            mod.SetData(ref hairstyle, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByHairstyleId.Key, girl);
                                        }
                                    }
                                }
                            }

                            using (ModInterface.Log.MakeIndent("special parts"))
                            {
                                foreach (var girlId_ModsBySpecialPartId in specialPartModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsBySpecialPartId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsBySpecialPartId.Key);

                                    using (ModInterface.Log.MakeIndent($"{girlId_ModsBySpecialPartId.Key} {girl.girlName}"))
                                    {
                                        foreach (var specialPartId_Mods in girlId_ModsBySpecialPartId.Value)
                                        {
                                            using (ModInterface.Log.MakeIndent($"Special Part Id {specialPartId_Mods.Key}"))
                                            {
                                                var specialPart = expansion.GetSpecialPart(specialPartId_Mods.Key);

                                                foreach (var mod in specialPartId_Mods.Value.OrderBy(x => x.LoadPriority))
                                                {
                                                    mod.SetData(ref specialPart, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsBySpecialPartId.Key, girl);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            using (ModInterface.Log.MakeIndent("lines"))
                            {
                                foreach (var girlId_ModsByIdByDialogTrigger in dialogLineModsByIdByDialogTriggerByGirlId)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByIdByDialogTrigger.Key)];

                                    foreach (var dialogTriggerId_ModsById in girlId_ModsByIdByDialogTrigger.Value)
                                    {
                                        var dialogTrigger = dialogTriggerDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.DialogTrigger, dialogTriggerId_ModsById.Key)];
                                        var expansion = ExpandedDialogTriggerDefinition.Get(dialogTriggerId_ModsById.Key);

                                        foreach (var lineId_Mods in dialogTriggerId_ModsById.Value)
                                        {
                                            if (expansion.TryGetLine(dialogTrigger, girlId_ModsByIdByDialogTrigger.Key, lineId_Mods.Key, out var line))
                                            {
                                                foreach (var mod in lineId_Mods.Value.OrderBy(x => x.LoadPriority))
                                                {
                                                    mod.SetData(ref line, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByIdByDialogTrigger.Key, girl);
                                                }
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

                            ModInterface.Log.LogInfo("styles");
                            foreach (var locationMod in locationDataMods)
                            {
                                var styles = locationMod.GetStyles();

                                if (styles != null)
                                {
                                    var expansion = ExpandedLocationDefinition.Get(locationMod.Id);
                                    expansion.GirlIdToLocationStyleInfo ??= new Dictionary<RelativeId, GirlStyleInfo>();

                                    foreach (var info in styles)
                                    {
                                        expansion.GirlIdToLocationStyleInfo[info.Item1] = info.Item2;
                                    }
                                }
                            }
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
            }
            catch (Exception e)
            {
                ModInterface.Log.LogError($"{e}");
            }
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

        private static void AddGirlSubMods<T>(IEnumerable<IGirlSubDataMod<T>> subMods,
            Dictionary<RelativeId, List<IGirlSubDataMod<T>>> subModListsById,
            Dictionary<RelativeId, List<IGirlSubDataMod<GirlPartSubDefinition>>> partListsById)
        {
            if (subMods != null && subMods.Any())
            {
                foreach (var subMod in subMods.Where(x => x != null))
                {
                    var subModList = subModListsById.GetOrNew(subMod.Id);
                    subModList.Add(subMod);

                    var parts = subMod.GetPartDataMods();
                    if (parts != null
                        && parts.Any())
                    {
                        foreach (var part in parts.Where(x => x != null))
                        {
                            partListsById.GetOrNew(part.Id).Add(part);
                        }
                    }
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

        private static void MapRelativeIdRange(IDictionary<RelativeId, int> idToIndex, IDictionary<int, RelativeId> indexToId, int count)
        {
            for (int i = 0; i < count; i++)
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
        where T : new()
        {
            foreach (var id in ids.Where(x => !idToIndex.ContainsKey(x)))
            {
                idToIndex[id] = startingIndex;
                indexToId[startingIndex++] = id;
                gameData.Add(new T());
                ModInterface.Log.LogInfo($"New GameData for id {id}");
            }
        }
    }
}
