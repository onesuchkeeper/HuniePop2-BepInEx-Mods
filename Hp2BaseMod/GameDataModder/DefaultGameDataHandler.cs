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
        out Dictionary<int, TokenDefinition> tokenDataDict)
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
            using (ModInterface.Log.MakeIndent("girls"))
            {
                var herQuestionsDt = dialogTriggerDataDict[DialogTriggers.HerQuestion.LocalId];
                var herQuestionsDtExp = herQuestionsDt.Expansion();
                var herQuestionsGoodRespDt = dialogTriggerDataDict[DialogTriggers.HerQuestionGoodResponse.LocalId];
                var herQuestionsGoodRespDtExp = herQuestionsGoodRespDt.Expansion();
                var herQuestionsBadRespDt = dialogTriggerDataDict[DialogTriggers.HerQuestionBadResponse.LocalId];
                var herQuestionsBadRespDtExp = herQuestionsBadRespDt.Expansion();

                var dateGreetingDt = dialogTriggerDataDict[DialogTriggers.DateGreeting.LocalId];
                var dateGreetingDtExp = dateGreetingDt.Expansion();

                ExpandedGirlDefinition.DialogTriggerIndexes.MapRelativeIdRange(girlDataDict.Count, 1);

                foreach (var girl in girlDataDict.Values)
                {
                    using (ModInterface.Log.MakeIndent($"runtime id: {girl.id}, Name: {girl.girlName}, DialogTriggerTab: {girl.dialogTriggerTab}"))
                    {
                        void defaultStyleExpansion(ExpandedStyleDefinition expansion, int index)
                        {
                            expansion.IsNSFW = false;
                            expansion.IsPurchased = index > 6;
                            expansion.IsCodeUnlocked = index == 6;
                        }

                        var id = new RelativeId(-1, girl.id);
                        var expansion = ExpandedGirlDefinition.Get(id);

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

                        body.PartLookup.MapRelativeIdRange(girl.parts.Count);
                        body.SpecialPartLookup.MapRelativeIdRange(girl.specialParts.Count);

                        expansion.ExpressionLookup.MapRelativeIdRange(girl.expressions.Count);
                        expansion.HairstyleLookup.MapRelativeIdRange(girl.hairstyles.Count);
                        expansion.OutfitLookup.MapRelativeIdRange(girl.outfits.Count);

                        int i = 0;
                        var hairShowingSpecials = new List<RelativeId>();
                        foreach (var hairstyle in girl.hairstyles)
                        {
                            var hairstyleId = expansion.HairstyleLookup[i];
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
                            ModInterface.GameData._specialEffects[SpecialParts.KyuWingId] = girl.specialEffectPrefab;
                        }
                        else
                        {
                            if (id == Girls.MoxieId)
                            {
                                ModInterface.GameData._specialEffects[SpecialParts.MoxieWingId] = girl.specialEffectPrefab;
                            }
                            else if (id == Girls.JewnId)
                            {
                                ModInterface.GameData._specialEffects[SpecialParts.JewnWingId] = girl.specialEffectPrefab;
                            }

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

                        var herQuestionsSet = herQuestionsDtExp.GetLineSetOrNew(herQuestionsDt, id);
                        var herQuestionsGoodRespSet = herQuestionsGoodRespDtExp.GetLineSetOrNew(herQuestionsGoodRespDt, id);
                        var herQuestionsBadRespSet = herQuestionsBadRespDtExp.GetLineSetOrNew(herQuestionsBadRespDt, id);
                        var dateGreetingSet = dateGreetingDtExp.GetLineSetOrNew(dateGreetingDt, id);

                        expansion.HerQuestionIdToIndex.MapRelativeIdRange(herQuestionsSet.dialogLines.Count);
                        expansion.HerQuestionGoodResponseIdToDtIndex.MapRelativeIdRange(herQuestionsGoodRespSet.dialogLines.Count);
                        expansion.HerQuestionBadResponseIdToDtIndex.MapRelativeIdRange(herQuestionsBadRespSet.dialogLines.Count);
                        expansion.DateGreetingLocIdToDtIndex.MapRelativeIdRange(dateGreetingSet.dialogLines.Count, 0, 9);
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

                    expansion.DefaultStyle = new RelativeId(-1, (int)def.dateGirlStyleType);
                }
            }

            using (ModInterface.Log.MakeIndent("Questions"))
            {
                ExpandedQuestionDefinition.DialogTriggerIndexes.MapRelativeIdRange(questionDataDict.Count, 0, 1);

                foreach (var id_def in questionDataDict)
                {
                    var expansion = id_def.Value.Expansion();
                    expansion.AnswerLookup.MapRelativeIdRange(id_def.Value.questionAnswers.Count);
                }
            }

            using (ModInterface.Log.MakeIndent("Items"))
            {
                foreach (var item in itemDataDict.Values)
                {
                    switch (item.itemType)
                    {
                        case ItemType.DATE_GIFT:
                            item.storeCost = 6;
                            item.storeSectionPreference = true;
                            break;
                        case ItemType.SMOOTHIE:
                            item.storeCost = 5;
                            item.storeSectionPreference = true;
                            break;
                        case ItemType.SHOES:
                        case ItemType.UNIQUE_GIFT:
                            item.storeCost = 4;
                            item.storeSectionPreference = true;
                            break;
                    }
                }
            }
        }
    }

    private static Dictionary<int, T> GetDataDict<T>(GameData gameData, Type dataType, string dataName)
                => AccessTools.DeclaredField(dataType, "_definitions")
                              .GetValue(AccessTools.DeclaredField(typeof(GameData), dataName)
                              .GetValue(gameData)) as Dictionary<int, T>;
}
