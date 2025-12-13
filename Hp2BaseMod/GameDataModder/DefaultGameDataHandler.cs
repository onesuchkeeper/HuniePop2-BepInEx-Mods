using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.ModGameData;

namespace Hp2BaseMod;

internal static class DefaultGameDataHandler
{
    // in game this info is stored in the location manager which isn't instantiated at this point
    // I don't love it but for now I'll just copy it here
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
        23, //outer space
        26, //airplane bathroom
    };

    public static void CollectDefaultData(GameData gameData,
        out Dictionary<int, AbilityDefinition> abilityDataDict,
        out Dictionary<int, AilmentDefinition> ailmentDataDict,
        out Dictionary<int, CodeDefinition> codeDataDict,
        out Dictionary<int, CutsceneDefinition> cutsceneDataDict,
        out Dictionary<int, DialogTriggerDefinition> dialogTriggerDataDict,
        out Dictionary<int, DlcDefinition> dlcDataDict,
        out Dictionary<int, EnergyDefinition> energyDataDict,
        out Dictionary<int, GirlDefinition> girlDataDict,
        out Dictionary<int, GirlPairDefinition> girlPairDataDict,
        out Dictionary<int, ItemDefinition> itemDataDict,
        out Dictionary<int, LocationDefinition> locationDataDict,
        out Dictionary<int, PhotoDefinition> photoDataDict,
        out Dictionary<int, QuestionDefinition> questionDataDict,
        out Dictionary<int, TokenDefinition> tokenDataDict
        )
    {
        //grab dicts
        abilityDataDict = GetDataDict<AbilityDefinition>(gameData, typeof(AbilityData), "_abilityData");
        ailmentDataDict = GetDataDict<AilmentDefinition>(gameData, typeof(AilmentData), "_ailmentData");
        codeDataDict = GetDataDict<CodeDefinition>(gameData, typeof(CodeData), "_codeData");
        cutsceneDataDict = GetDataDict<CutsceneDefinition>(gameData, typeof(CutsceneData), "_cutsceneData");
        dialogTriggerDataDict = GetDataDict<DialogTriggerDefinition>(gameData, typeof(DialogTriggerData), "_dialogTriggerData");
        dlcDataDict = GetDataDict<DlcDefinition>(gameData, typeof(DlcData), "_dlcData");
        energyDataDict = GetDataDict<EnergyDefinition>(gameData, typeof(EnergyData), "_energyData");
        girlDataDict = GetDataDict<GirlDefinition>(gameData, typeof(GirlData), "_girlData");
        girlPairDataDict = GetDataDict<GirlPairDefinition>(gameData, typeof(GirlPairData), "_girlPairData");
        itemDataDict = GetDataDict<ItemDefinition>(gameData, typeof(ItemData), "_itemData");
        locationDataDict = GetDataDict<LocationDefinition>(gameData, typeof(LocationData), "_locationData");
        photoDataDict = GetDataDict<PhotoDefinition>(gameData, typeof(PhotoData), "_photoData");
        questionDataDict = GetDataDict<QuestionDefinition>(gameData, typeof(QuestionData), "_questionData");
        tokenDataDict = GetDataDict<TokenDefinition>(gameData, typeof(TokenData), "_tokenData");

        // register default sub data
        using (ModInterface.Log.MakeIndent("registering default sub data"))
        {
            using (ModInterface.Log.MakeIndent("dialog triggers"))
            {
                foreach (var dt in dialogTriggerDataDict.Values)
                {
                    var expansion = ExpandedDialogTriggerDefinition.Get(dt);
                    var id = new RelativeId(-1, dt.id);

                    if (GirlSubDataModder.IsGirlDialogTrigger(dt))
                    {
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
                        MapRelativeIdRange(body.PartIdToIndex, body.PartIndexToId, girl.parts.Count);
                        MapRelativeIdRange(expansion.ExpressionIdToIndex, expansion.ExpressionIndexToId, girl.expressions.Count);
                        MapRelativeIdRange(expansion.HairstyleIdToIndex, expansion.HairstyleIndexToId, girl.hairstyles.Count);
                        MapRelativeIdRange(expansion.OutfitIdToIndex, expansion.OutfitIndexToId, girl.outfits.Count);
                        MapRelativeIdRange(body.SpecialPartIdToIndex, body.SpecialPartIndexToId, girl.specialParts.Count);

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

                        foreach (var part in girl.specialParts)
                        {
                            part.Expansion().RequiredHairstyles = hairShowingSpecials.ToList();
                        }

                        for (i = 0; i < girl.favAnswers.Count; i++)
                        {
                            expansion.FavQuestionIdToAnswerId[new RelativeId(-1, i + 1)] = new RelativeId(-1, girl.favAnswers[i]);
                        }
                    }
                }
            }

            using (ModInterface.Log.MakeIndent("pairs"))
            {
                foreach (var def in girlPairDataDict.Values) def.Expansion().PairStyle = new PairStyleInfo(def);
            }

            using (ModInterface.Log.MakeIndent("locations"))
            {
                foreach (var def in locationDataDict.Values)
                {
                    var expansion = def.Expansion();

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

            using (ModInterface.Log.MakeIndent("Questions"))
            {
                foreach (var id_def in questionDataDict)
                {
                    var expansion = id_def.Value.Expansion();
                    expansion.DialogTriggerIndex = id_def.Key - 1;
                    MapRelativeIdRange(expansion.AnswerIdToIndex, expansion.AnswerIndexToId, id_def.Value.questionAnswers.Count);
                }
            }
        }
    }

    private static Dictionary<int, T> GetDataDict<T>(GameData gameData, Type dataType, string dataName)
                => AccessTools.DeclaredField(dataType, "_definitions")
                              .GetValue(AccessTools.DeclaredField(typeof(GameData), dataName)
                              .GetValue(gameData)) as Dictionary<int, T>;

    private static void MapRelativeIdRange(IDictionary<RelativeId, int> idToIndex, IDictionary<int, RelativeId> indexToId, int count, int startingIndex = 0)
    {
        for (int i = startingIndex; i < startingIndex + count; i++)
        {
            idToIndex[new RelativeId(-1, i)] = i;
            indexToId[i] = new RelativeId(-1, i);
        }
    }
}
