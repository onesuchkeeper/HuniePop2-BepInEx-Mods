// Hp2BaseModdedLoader 2021, by OneSuchKeeper

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
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

                ModInterface.Log.LogTitle("Modifying GameData");
                using (ModInterface.Log.MakeIndent())
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

                    //replace outfits/hairstyles with expansions
                    foreach (var girl in girlDataDict)
                    {
                        var i = 0;
                        girl.Value.outfits = girl.Value.outfits.Select(x => (GirlOutfitSubDefinition)new ExpandedOutfitDefinition(x, i++)).ToList();
                        i = 0;
                        girl.Value.hairstyles = girl.Value.hairstyles.Select(x => (GirlHairstyleSubDefinition)new ExpandedHairstyleDefinition(x, i++)).ToList();
                    }

                    // register default sub data
                    ModInterface.Log.LogInfo("registering default sub data");
                    using (ModInterface.Log.MakeIndent())
                    {
                        ModInterface.Log.LogInfo("dialog triggers");
                        foreach (var dt in dialogTriggerDataDict.Values)
                        {
                            var expansion = ExpandedDialogTriggerDefinition.Get(dt);
                            var id = new RelativeId(-1, dt.id);

                            // lines are looked up by trigger id and girl id.
                            var girlIndex = 0;
                            foreach (var lineSet in dt.dialogLineSets)
                            {
                                var girlId = new RelativeId(-1, girlIndex);
                                var lineIndex = 0;
                                foreach (var line in lineSet.dialogLines)
                                {
                                    var lineId = new RelativeId(-1, lineIndex);

                                    if (!expansion.GirlIdToLineIdToLineIndex.TryGetValue(girlId, out var lineIndexLookup))
                                    {
                                        lineIndexLookup = new Dictionary<RelativeId, int>()
                                        {
                                            {RelativeId.Default, -1}
                                        };
                                        expansion.GirlIdToLineIdToLineIndex.Add(girlId, lineIndexLookup);
                                    }

                                    if (!expansion.GirlIdToLineIndexToLineId.TryGetValue(girlId, out var lineIdLookup))
                                    {
                                        lineIdLookup = new Dictionary<int, RelativeId>();
                                        expansion.GirlIdToLineIndexToLineId.Add(girlId, lineIdLookup);
                                    }

                                    lineIndexLookup[lineId] = lineIndex;
                                    lineIdLookup[lineIndex] = lineId;

                                    lineIndex++;
                                }

                                girlIndex++;
                            }
                        }

                        ModInterface.Log.LogInfo("girls");
                        foreach (var girl in girlDataDict.Values)
                        {
                            var expansion = ExpandedGirlDefinition.Get(girl);
                            var id = new RelativeId(-1, girl.id);

                            expansion.DialogTriggerIndex = girl.id;

                            MapRelativeIdRange(expansion.PartIdToIndex, expansion.PartIndexToId, girl.parts.Count);
                            MapRelativeIdRange(expansion.ExpressionIdToIndex, expansion.ExpressionIndexToId, girl.expressions.Count);
                            MapRelativeIdRange(expansion.HairstyleIdToIndex, expansion.HairstyleIndexToId, girl.hairstyles.Count);
                            MapRelativeIdRange(expansion.OutfitIdToIndex, expansion.OutfitIndexToId, girl.outfits.Count);

                            if (id == Girls.KyuId)
                            {
                                expansion.BackPosition = girl.specialEffectOffset;
                            }
                            else
                            {
                                expansion.HeadPosition = girl.specialEffectOffset;
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
                    var outfitModsByIdByGirl = new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<ExpandedOutfitDefinition>>>>();
                    var hairstyleModsByIdByGirl = new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<ExpandedHairstyleDefinition>>>>();
                    var dialogLineModsByIdByDialogTriggerByGirlId = new Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<DialogLine>>>>>();

                    foreach (var girlMod in girlDataMods)
                    {
                        AddGirlSubMods(girlMod.Id, girlMod.GetPartMods(), partModsByIdByGirl);
                        AddGirlSubMods(girlMod.Id, girlMod.GetExpressions(), expressionModsByIdByGirl);
                        AddGirlSubMods(girlMod.Id, girlMod.GetOutfits(), outfitModsByIdByGirl);
                        AddGirlSubMods(girlMod.Id, girlMod.GetHairstyles(), hairstyleModsByIdByGirl);

                        var linesByDialogTriggerId = girlMod.GetLinesByDialogTriggerId();

                        if (linesByDialogTriggerId != null)
                        {
                            if (!dialogLineModsByIdByDialogTriggerByGirlId.ContainsKey(girlMod.Id))
                            {
                                dialogLineModsByIdByDialogTriggerByGirlId.Add(girlMod.Id, new Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<DialogLine>>>>());
                            }

                            foreach (var dtIdToLines in linesByDialogTriggerId)
                            {
                                if (!dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id].ContainsKey(dtIdToLines.Item1))
                                {
                                    dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id].Add(dtIdToLines.Item1, new Dictionary<RelativeId, List<IGirlSubDataMod<DialogLine>>>());
                                }

                                foreach (var lineMod in dtIdToLines.Item2)
                                {
                                    if (!dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtIdToLines.Item1].ContainsKey(lineMod.Id))
                                    {
                                        dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtIdToLines.Item1].Add(lineMod.Id, new List<IGirlSubDataMod<DialogLine>>());
                                    }

                                    dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtIdToLines.Item1][lineMod.Id].Add(lineMod);
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
                    ModInterface.Log.LogInfo("creating and registering data for new ids for sub mods");
                    using (ModInterface.Log.MakeIndent())
                    {
                        ModInterface.Log.LogInfo("dialog trigger indexes");
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
                                }
                                nextIndex++;
                            }
                        }

                        ModInterface.Log.LogInfo("girl parts");
                        foreach (var girlIdToPartModsById in partModsByIdByGirl)
                        {
                            var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlIdToPartModsById.Key)];
                            var expansion = ExpandedGirlDefinition.Get(girlDef);

                            RegisterUnregisteredIds(expansion.PartIdToIndex,
                                expansion.PartIndexToId,
                                girlDef.parts.Count,
                                girlIdToPartModsById.Value.Select(x => x.Key).Distinct(),
                                () => girlDef.parts.Add(new GirlPartSubDefinition()));
                        }

                        ModInterface.Log.LogInfo("girl expressions");
                        foreach (var girlIdToExpressionModsById in expressionModsByIdByGirl)
                        {
                            var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlIdToExpressionModsById.Key)];
                            var expansion = ExpandedGirlDefinition.Get(girlDef);

                            RegisterUnregisteredIds(expansion.ExpressionIdToIndex,
                                expansion.ExpressionIndexToId,
                                girlDef.expressions.Count,
                                girlIdToExpressionModsById.Value.Select(x => x.Key).Distinct(),
                                () => girlDef.expressions.Add(new GirlExpressionSubDefinition()));
                        }

                        ModInterface.Log.LogInfo("girl outfits");
                        foreach (var girlIdToOutfitModsById in outfitModsByIdByGirl)
                        {
                            var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlIdToOutfitModsById.Key)];
                            var expansion = ExpandedGirlDefinition.Get(girlDef);

                            RegisterUnregisteredIds(expansion.OutfitIdToIndex,
                                expansion.OutfitIndexToId,
                                girlDef.outfits.Count,
                                girlIdToOutfitModsById.Value.Select(x => x.Key).Distinct(),
                                () => girlDef.outfits.Add(new ExpandedOutfitDefinition()));
                        }

                        ModInterface.Log.LogInfo("girl hairstyles");
                        foreach (var girlIdToHairstyleModsById in hairstyleModsByIdByGirl)
                        {
                            var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlIdToHairstyleModsById.Key)];
                            var expansion = ExpandedGirlDefinition.Get(girlDef);

                            RegisterUnregisteredIds(expansion.HairstyleIdToIndex,
                                expansion.HairstyleIndexToId,
                                girlDef.hairstyles.Count,
                                girlIdToHairstyleModsById.Value.Select(x => x.Key).Distinct(),
                                () => girlDef.hairstyles.Add(new ExpandedHairstyleDefinition()));
                        }

                        ModInterface.Log.LogInfo("girl lines");
                        foreach (var girlId_DialogLineModsByIdByDialogTrigger in dialogLineModsByIdByDialogTriggerByGirlId)
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
                                    () => dialogTriggerSet.dialogLines.Add(new DialogLine()));
                            }
                        }

                        ModInterface.Log.LogInfo("location slots");
                        foreach (var location in ModInterface.Data.GetIds(GameDataType.Location).Where(x => x.SourceId != -1))
                        {
                            // do good stuff here pls
                        }
                    }

                    ModInterface.Log.DecreaseIndent();

                    //Requesting internal data
                    ModInterface.Log.LogInfo("cataloging internal assets");
                    using (ModInterface.Log.MakeIndent())
                    {
                        assetProvider.FulfilInternalAssetRequests();
                    }

                    //setup defs
                    ModInterface.Log.LogInfo("applying mod data to game data");
                    using (ModInterface.Log.MakeIndent())
                    {
                        ModInterface.Log.LogInfo("abilities");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(abilityDataDict, abilityDataMods, gameDataProvider, assetProvider, GameDataType.Ability);
                        }

                        ModInterface.Log.LogInfo("ailments");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(ailmentDataDict, ailmentDataMods, gameDataProvider, assetProvider, GameDataType.Ailment);
                        }

                        ModInterface.Log.LogInfo("codes");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(codeDataDict, codeDataMods, gameDataProvider, assetProvider, GameDataType.Code);
                        }

                        ModInterface.Log.LogInfo("cutscenes");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(cutsceneDataDict, cutsceneDataMods, gameDataProvider, assetProvider, GameDataType.Cutscene);
                        }

                        ModInterface.Log.LogInfo("dialog triggers");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(dialogTriggerDataDict, dialogTriggerDataMods, gameDataProvider, assetProvider, GameDataType.DialogTrigger);
                        }

                        ModInterface.Log.LogInfo("dlcs");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(dlcDataDict, dlcDataMods, gameDataProvider, assetProvider, GameDataType.Dlc);
                        }

                        ModInterface.Log.LogInfo("energy");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(energyDataDict, energyDataMods, gameDataProvider, assetProvider, GameDataType.Energy);
                        }

                        ModInterface.Log.LogInfo("girls");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(girlDataDict, girlDataMods, gameDataProvider, assetProvider, GameDataType.Girl);

                            ModInterface.Log.LogInfo("parts");
                            using (ModInterface.Log.MakeIndent())
                            {
                                foreach (var girlId_ModsByPartId in partModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByPartId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsByPartId.Key);

                                    foreach (var partId_Mods in girlId_ModsByPartId.Value)
                                    {
                                        var part = expansion.GetPart(girl, partId_Mods.Key);

                                        foreach (var mod in partId_Mods.Value.OrderBy(x => x.LoadPriority))
                                        {
                                            mod.SetData(ref part, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByPartId.Key);
                                        }
                                    }
                                }
                            }

                            ModInterface.Log.LogInfo("expressions");
                            using (ModInterface.Log.MakeIndent())
                            {
                                foreach (var girlId_ModsByExpressionId in expressionModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByExpressionId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsByExpressionId.Key);

                                    foreach (var expressionId_Mods in girlId_ModsByExpressionId.Value)
                                    {
                                        var expression = expansion.GetExpression(girl, expressionId_Mods.Key);

                                        foreach (var mod in expressionId_Mods.Value.OrderBy(x => x.LoadPriority))
                                        {
                                            mod.SetData(ref expression, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByExpressionId.Key);
                                        }
                                    }
                                }
                            }

                            ModInterface.Log.LogInfo("outfits");
                            using (ModInterface.Log.MakeIndent())
                            {
                                foreach (var girlId_ModsByOutfitId in outfitModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByOutfitId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsByOutfitId.Key);

                                    foreach (var outfitId_Mods in girlId_ModsByOutfitId.Value)
                                    {
                                        var outfit = expansion.GetOutfit(girl, outfitId_Mods.Key);
                                        if (!(outfit is ExpandedOutfitDefinition expandedOutfit))
                                        {
                                            throw new Exception();
                                        }

                                        foreach (var mod in outfitId_Mods.Value.OrderBy(x => x.LoadPriority))
                                        {
                                            mod.SetData(ref expandedOutfit, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByOutfitId.Key);
                                        }
                                    }
                                }
                            }

                            ModInterface.Log.LogInfo("hairstyles");
                            using (ModInterface.Log.MakeIndent())
                            {
                                foreach (var girlId_ModsByHairstyleId in hairstyleModsByIdByGirl)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_ModsByHairstyleId.Key)];
                                    var expansion = ExpandedGirlDefinition.Get(girlId_ModsByHairstyleId.Key);

                                    foreach (var hairstyleId_Mods in girlId_ModsByHairstyleId.Value)
                                    {
                                        var hairstyle = expansion.GetHairstyle(girl, hairstyleId_Mods.Key);

                                        if (!(hairstyle is ExpandedHairstyleDefinition expandedHairstye))
                                        {
                                            throw new Exception();
                                        }

                                        foreach (var mod in hairstyleId_Mods.Value.OrderBy(x => x.LoadPriority))
                                        {
                                            mod.SetData(ref expandedHairstye, gameDataProvider, assetProvider, InsertStyle.replace, girlId_ModsByHairstyleId.Key);
                                        }
                                    }
                                }
                            }

                            ModInterface.Log.LogInfo("lines");
                            using (ModInterface.Log.MakeIndent())
                            {
                                foreach (var girl_ToModsByIdByDialogTrigger in dialogLineModsByIdByDialogTriggerByGirlId)
                                {
                                    var girl = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girl_ToModsByIdByDialogTrigger.Key)];

                                    foreach (var dialogTriggerIdToModsById in girl_ToModsByIdByDialogTrigger.Value)
                                    {
                                        var dialogTrigger = dialogTriggerDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.DialogTrigger, dialogTriggerIdToModsById.Key)];
                                        var expansion = ExpandedDialogTriggerDefinition.Get(dialogTriggerIdToModsById.Key);

                                        foreach (var lineId_Mods in dialogTriggerIdToModsById.Value)
                                        {
                                            if (expansion.TryGetLine(dialogTrigger, girl_ToModsByIdByDialogTrigger.Key, lineId_Mods.Key, out var line))
                                            {
                                                foreach (var mod in lineId_Mods.Value.OrderBy(x => x.LoadPriority))
                                                {
                                                    mod.SetData(ref line, gameDataProvider, assetProvider, InsertStyle.replace, girl_ToModsByIdByDialogTrigger.Key);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        ModInterface.Log.LogInfo("girl pairs");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(girlPairDataDict, girlPairDataMods, gameDataProvider, assetProvider, GameDataType.GirlPair);

                            ModInterface.Log.LogInfo("styles");
                            using (ModInterface.Log.MakeIndent())
                            {
                                foreach (var pairMod in girlPairDataMods)
                                {
                                    var expansion = ExpandedGirlPairDefinition.Get(pairMod.Id);
                                    expansion.PairStyle = pairMod.GetStyles();
                                }
                            }
                        }

                        ModInterface.Log.LogInfo("items");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(itemDataDict, itemDataMods, gameDataProvider, assetProvider, GameDataType.Item);
                        }


                        ModInterface.Log.LogInfo("locations");
                        using (ModInterface.Log.MakeIndent())
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

                        ModInterface.Log.LogInfo("photos");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(photoDataDict, photoDataMods, gameDataProvider, assetProvider, GameDataType.Photo);
                        }

                        ModInterface.Log.LogInfo("questions");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(questionDataDict, questionDataMods, gameDataProvider, assetProvider, GameDataType.Question);
                        }

                        ModInterface.Log.LogInfo("tokens");
                        using (ModInterface.Log.MakeIndent())
                        {
                            SetData(tokenDataDict, tokenDataMods, gameDataProvider, assetProvider, GameDataType.Token);
                        }
                    }

                    ModInterface.Log.LogInfo("registering functional mods");
                    using (ModInterface.Log.MakeIndent())
                    {
                        ModInterface.Log.LogInfo("ailments");
                        ModInterface.Data.RegisterFunctionalAilments(ailmentDataMods.OfType<IFunctionalAilmentDataMod>());
                    }
                }
            }
            catch (Exception e)
            {
                ModInterface.Log.LogInfo($"{e}");
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

        private static void AddGirlSubMods<T>(RelativeId girlId,
                                              IEnumerable<IGirlSubDataMod<T>> subMods,
                                              Dictionary<RelativeId, Dictionary<RelativeId, List<IGirlSubDataMod<T>>>> collection)
        {
            if (subMods != null)
            {
                if (!collection.ContainsKey(girlId))
                {
                    collection.Add(girlId, new Dictionary<RelativeId, List<IGirlSubDataMod<T>>>());
                }

                foreach (var subMod in subMods)
                {
                    if (!collection[girlId].ContainsKey(subMod.Id))
                    {
                        collection[girlId].Add(subMod.Id, new List<IGirlSubDataMod<T>>());
                    }

                    collection[girlId][subMod.Id].Add(subMod);
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

        private static void RegisterUnregisteredIds(IDictionary<RelativeId, int> idToIndex,
            IDictionary<int, RelativeId> indexToId,
            int startingIndex,
            IEnumerable<RelativeId> ids,
            Action onRegistered)
        {
            foreach (var partId in ids.Where(x => !idToIndex.ContainsKey(x)))
            {
                idToIndex[partId] = startingIndex;
                indexToId[startingIndex++] = partId;
                onRegistered.Invoke();
            }
        }
    }
}
