using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.ModGameData;

namespace Hp2BaseMod;
#pragma warning disable HP001 // Deprecated member usage
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

    public static GameDataContext CollectDefaultData(GameData gameData, GameDefinitionProvider gameDefinitionProvider)
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
        var puzzleResources = gameDefinitionProvider._puzzleResources ?? throw new Exception("Puzzle Resources");
        var dollSpecialEffects = gameDefinitionProvider._dollSpecialEffects;
        var affections = gameDefinitionProvider._affections;
        var itemGiftHandlers = gameDefinitionProvider._itemGiftHandlers;
        var itemStoreHandlers = gameDefinitionProvider._itemStoreHandlers;

        // register default sub data
        using (ModInterface.Log.MakeIndent("registering default sub data"))
        {
            using (ModInterface.Log.MakeIndent("Affection"))
            {
                affections[PuzzleAffectionId.Talent] = new Affection(PuzzleAffectionId.Talent, "TALENT", [
                    new RelativeId(-1, 1),
                    new RelativeId(-1, 2),
                    new RelativeId(-1, 3),
                    new RelativeId(-1, 4),
                    new RelativeId(-1, 5),
                ]);
                affections[PuzzleAffectionId.Flirtation] = new Affection(PuzzleAffectionId.Flirtation, "FLIRTATION", [
                    new RelativeId(-1, 6),
                    new RelativeId(-1, 7),
                    new RelativeId(-1, 8),
                    new RelativeId(-1, 9),
                    new RelativeId(-1, 10),
                ]);
                affections[PuzzleAffectionId.Romance] = new Affection(PuzzleAffectionId.Romance, "ROMANCE", [
                    new RelativeId(-1, 11),
                    new RelativeId(-1, 12),
                    new RelativeId(-1, 13),
                    new RelativeId(-1, 14),
                    new RelativeId(-1, 15),
                ]);
                affections[PuzzleAffectionId.Sexuality] = new Affection(PuzzleAffectionId.Sexuality, "SEXUALITY", [
                    new RelativeId(-1, 16),
                    new RelativeId(-1, 17),
                    new RelativeId(-1, 18),
                    new RelativeId(-1, 19),
                    new RelativeId(-1, 20),
                ]);
            }

            using (ModInterface.Log.MakeIndent("Item Handlers"))
            {
                itemGiftHandlers[ItemGiftHandlerId.SmoothieFlirtation] = new SmoothieItemHandler(PuzzleAffectionId.Flirtation);
                itemGiftHandlers[ItemGiftHandlerId.SmoothieRomance] = new SmoothieItemHandler(PuzzleAffectionId.Romance);
                itemGiftHandlers[ItemGiftHandlerId.SmoothieSexuality] = new SmoothieItemHandler(PuzzleAffectionId.Sexuality);
                itemGiftHandlers[ItemGiftHandlerId.SmoothieTalent] = new SmoothieItemHandler(PuzzleAffectionId.Talent);
                itemGiftHandlers[ItemGiftHandlerId.DateGift] = new DateGiftItemHandler();
                itemGiftHandlers[ItemGiftHandlerId.Food] = new FoodItemHandler();
                itemGiftHandlers[ItemGiftHandlerId.StaminaFood] = new StaminaFoodItemHandler();
                itemGiftHandlers[ItemGiftHandlerId.Shoes] = new ShoesItemHandler();
                itemGiftHandlers[ItemGiftHandlerId.Uniques] = new UniqueItemHandler();
                itemGiftHandlers[ItemGiftHandlerId.Misc] = new NonGiftItemHandler();

                itemStoreHandlers[ItemTypes.DateGift] = new DateGiftStoreHandler();
                itemStoreHandlers[ItemTypes.Food] = new FoodStoreHandler();
                itemStoreHandlers[ItemTypes.Shoe] = new ShoeItemStoreHandler();
                itemStoreHandlers[ItemTypes.Smoothie] = new SmoothieStoreHandler();
                itemStoreHandlers[ItemTypes.Unique] = new UniqueItemStoreHandler();
            }

            using (ModInterface.Log.MakeIndent("girls"))
            {
                var herQuestionsDt = dialogTriggerDataDict[DialogTriggers.HerQuestion.LocalId];
                var herQuestionsDtExp = herQuestionsDt.GetExpansion();
                var herQuestionsGoodRespDt = dialogTriggerDataDict[DialogTriggers.HerQuestionGoodResponse.LocalId];
                var herQuestionsGoodRespDtExp = herQuestionsGoodRespDt.GetExpansion();
                var herQuestionsBadRespDt = dialogTriggerDataDict[DialogTriggers.HerQuestionBadResponse.LocalId];
                var herQuestionsBadRespDtExp = herQuestionsBadRespDt.GetExpansion();

                var dateGreetingDt = dialogTriggerDataDict[DialogTriggers.DateGreeting.LocalId];
                var dateGreetingDtExp = dateGreetingDt.GetExpansion();

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
                        var expansion = girl.GetExpansion();

                        switch (girl.favoriteAffectionType)
                        {
                            case PuzzleAffectionType.TALENT:
                                expansion.FavAffection = affections[PuzzleAffectionId.Talent];
                                break;
                            case PuzzleAffectionType.FLIRTATION:
                                expansion.FavAffection = affections[PuzzleAffectionId.Flirtation];
                                break;
                            case PuzzleAffectionType.ROMANCE:
                                expansion.FavAffection = affections[PuzzleAffectionId.Romance];
                                break;
                            case PuzzleAffectionType.SEXUALITY:
                                expansion.FavAffection = affections[PuzzleAffectionId.Sexuality];
                                break;
                        }

                        switch (girl.leastFavoriteAffectionType)
                        {
                            case PuzzleAffectionType.TALENT:
                                expansion.LeastFavAffection = affections[PuzzleAffectionId.Talent];
                                break;
                            case PuzzleAffectionType.FLIRTATION:
                                expansion.LeastFavAffection = affections[PuzzleAffectionId.Flirtation];
                                break;
                            case PuzzleAffectionType.ROMANCE:
                                expansion.LeastFavAffection = affections[PuzzleAffectionId.Romance];
                                break;
                            case PuzzleAffectionType.SEXUALITY:
                                expansion.LeastFavAffection = affections[PuzzleAffectionId.Sexuality];
                                break;
                        }

                        if (id == Girls.Zoey)
                        {
                            expansion.TalkHandler = new TelepathGirlTalkHandler();
                        }
                        else
                        {
                            expansion.TalkHandler = new GirlTalkHandler();
                        }

                        var body = new GirlBodySubDefinition(girl)
                        {
                            BodyName = "HuniePop 2",
                            LocationIdToOutfitId = new() {
                                {Locations.MassageSpa, new GirlStyleInfo(Styles.Relaxing)},
                                {Locations.Aquarium, new GirlStyleInfo(Styles.Activity)},
                                {Locations.SecludedCabana, new GirlStyleInfo(Styles.Relaxing)},
                                {Locations.PoolsideBar, new GirlStyleInfo(Styles.Water)},
                                {Locations.GolfCourse, new GirlStyleInfo(Styles.Activity)},
                                {Locations.CruiseShip, new GirlStyleInfo(Styles.Water)},
                                {Locations.RooftopLounge, new GirlStyleInfo(Styles.Romantic)},
                                {Locations.Casino, new GirlStyleInfo(Styles.Party)},
                                {Locations.PrivateTable, new GirlStyleInfo(Styles.Romantic)},
                                {Locations.SecretGrotto, new GirlStyleInfo(Styles.Water)},
                                {Locations.RoyalSuite, new GirlStyleInfo(Styles.Sexy)},
                                {Locations.AirplaneBathroom, new GirlStyleInfo(Styles.Activity)},
                                {Locations.OuterSpace, new GirlStyleInfo(Styles.Sexy)},
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
                            var hairstyleExpansion = hairstyle.GetExpansion();
                            defaultStyleExpansion(hairstyleExpansion, i++);

                            if (!hairstyle.hideSpecials)
                            {
                                hairShowingSpecials.Add(hairstyleId);
                            }
                        }

                        i = 0;
                        girl.outfits.ForEach(x => defaultStyleExpansion(x.GetExpansion(), i++));

                        if (id == Girls.Kyu)
                        {
                            body.BackPos = girl.specialEffectOffset;
                            dollSpecialEffects[SpecialParts.KyuWingId] = girl.specialEffectPrefab;
                        }
                        else
                        {
                            if (id == Girls.Moxie)
                            {
                                dollSpecialEffects[SpecialParts.MoxieWingId] = girl.specialEffectPrefab;
                            }
                            else if (id == Girls.Jewn)
                            {
                                dollSpecialEffects[SpecialParts.JewnWingId] = girl.specialEffectPrefab;
                            }

                            body.HeadPos = girl.specialEffectOffset;
                        }

                        foreach (var part in girl.specialParts)
                        {
                            part.GetExpansion().RequiredHairstyles = hairShowingSpecials.ToList();
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
                foreach (var def in girlPairDataDict.Values) def.GetExpansion().PairStyle = new PairStyleInfo(def);
            }

            using (ModInterface.Log.MakeIndent("locations"))
            {
                foreach (var def in locationDataDict.Values)
                {
                    var expansion = def.GetExpansion();

                    if (def.locationType == LocationType.DATE)
                    {
                        expansion.DefaultStyle = new RelativeId(-1, (int)def.dateGirlStyleType);

                        if (!_specialDateLocationIds.Contains(def.id))
                        {
                            expansion.AllowNormal = true;
                            expansion.PostBoss = false;
                            expansion.AllowNonStop = true;
                        }

                        expansion.AllowNoPair = false;
                    }
                    else if (def.locationType == LocationType.HUB)
                    {
                        expansion.AllowNoPair = true;
                    }
                    else
                    {
                        expansion.AllowNoPair = false;
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
                ExpandedQuestionDefinition.DialogTriggerIndexes.MapRelativeIdRange(questionDataDict.Count, 0, 1);

                foreach (var id_def in questionDataDict)
                {
                    var expansion = id_def.Value.GetExpansion();
                    expansion.AnswerLookup.MapRelativeIdRange(id_def.Value.questionAnswers.Count);
                }
            }

            using (ModInterface.Log.MakeIndent("Items"))
            {
                foreach (var item in itemDataDict.Values)
                {
                    var expansion = item.GetExpansion();

                    if (item.storeSectionPreference || item.itemType == ItemType.FRUIT)
                    {
                        switch (item.affectionType)
                        {
                            case PuzzleAffectionType.TALENT:
                                expansion.Affection = affections[PuzzleAffectionId.Talent];
                                break;
                            case PuzzleAffectionType.FLIRTATION:
                                expansion.Affection = affections[PuzzleAffectionId.Flirtation];
                                break;
                            case PuzzleAffectionType.ROMANCE:
                                expansion.Affection = affections[PuzzleAffectionId.Romance];
                                break;
                            case PuzzleAffectionType.SEXUALITY:
                                expansion.Affection = affections[PuzzleAffectionId.Sexuality];
                                break;
                        }
                    }

                    switch (item.itemType)
                    {
                        case ItemType.DATE_GIFT:
                            expansion.StoreHandler = itemStoreHandlers[ItemTypes.DateGift];
                            expansion.GiftHandler = itemGiftHandlers[ItemGiftHandlerId.DateGift];
                            item.storeCost = 6;
                            break;
                        case ItemType.SMOOTHIE:
                            expansion.StoreHandler = itemStoreHandlers[ItemTypes.Smoothie];
                            switch (item.affectionType)
                            {
                                case PuzzleAffectionType.TALENT:
                                    expansion.Affection = affections[PuzzleAffectionId.Talent];
                                    expansion.GiftHandler = itemGiftHandlers[ItemGiftHandlerId.SmoothieTalent];
                                    break;
                                case PuzzleAffectionType.FLIRTATION:
                                    expansion.Affection = affections[PuzzleAffectionId.Flirtation];
                                    expansion.GiftHandler = itemGiftHandlers[ItemGiftHandlerId.SmoothieFlirtation];
                                    break;
                                case PuzzleAffectionType.ROMANCE:
                                    expansion.Affection = affections[PuzzleAffectionId.Romance];
                                    expansion.GiftHandler = itemGiftHandlers[ItemGiftHandlerId.SmoothieRomance];
                                    break;
                                case PuzzleAffectionType.SEXUALITY:
                                    expansion.Affection = affections[PuzzleAffectionId.Sexuality];
                                    expansion.GiftHandler = itemGiftHandlers[ItemGiftHandlerId.SmoothieSexuality];
                                    break;
                            }
                            
                            item.storeCost = 5;
                            item.storeSectionPreference = true;
                            break;
                        case ItemType.SHOES:
                            expansion.StoreHandler = itemStoreHandlers[ItemTypes.Shoe];
                            expansion.GiftHandler = itemGiftHandlers[ItemGiftHandlerId.Shoes];
                            item.storeCost = 4;
                            item.storeSectionPreference = true;
                            break;
                        case ItemType.UNIQUE_GIFT:
                            expansion.StoreHandler = itemStoreHandlers[ItemTypes.Unique];
                            expansion.GiftHandler = itemGiftHandlers[ItemGiftHandlerId.Uniques];
                            item.storeCost = 4;
                            item.storeSectionPreference = true;
                            break;
                        case ItemType.FOOD:
                            expansion.StoreHandler = itemStoreHandlers[ItemTypes.Food];
                            expansion.GiftHandler = item.noStaminaCost 
                                ? itemGiftHandlers[ItemGiftHandlerId.Food]
                                : itemGiftHandlers[ItemGiftHandlerId.StaminaFood];
                            break;
                    }
                }
            }

            using (ModInterface.Log.MakeIndent("Puzzle Resources"))
            {
                var flirtation = gameDefinitionProvider.GetToken(TokenTypes.AffectionFlirtation);
                puzzleResources[PuzzleResourceId.AffectionFlirtation] = new PuzzleResourceAffection(flirtation.resourceName, flirtation.resourceSign, PuzzleResourceId.AffectionFlirtation, PuzzleAffectionId.Flirtation);

                var romance = gameDefinitionProvider.GetToken(TokenTypes.AffectionRomance);
                puzzleResources[PuzzleResourceId.AffectionRomance] = new PuzzleResourceAffection(romance.resourceName, romance.resourceSign, PuzzleResourceId.AffectionRomance, PuzzleAffectionId.Romance);

                var sexuality = gameDefinitionProvider.GetToken(TokenTypes.AffectionSexuality);
                puzzleResources[PuzzleResourceId.AffectionSexuality] = new PuzzleResourceAffection(sexuality.resourceName, sexuality.resourceSign, PuzzleResourceId.AffectionSexuality, PuzzleAffectionId.Sexuality);

                var talent = gameDefinitionProvider.GetToken(TokenTypes.AffectionTalent);
                puzzleResources[PuzzleResourceId.AffectionTalent] = new PuzzleResourceAffection(talent.resourceName, talent.resourceSign, PuzzleResourceId.AffectionTalent, PuzzleAffectionId.Talent);
                
                var joy = gameDefinitionProvider.GetToken(TokenTypes.Joy);
                puzzleResources[PuzzleResourceId.Moves] = new PuzzleResourceMoves(joy.resourceName, joy.resourceSign);

                var stamina = gameDefinitionProvider.GetToken(TokenTypes.Stamina);
                puzzleResources[PuzzleResourceId.Stamina] = new PuzzleResourceStamina(stamina.resourceName, stamina.resourceSign);

                var passion = gameDefinitionProvider.GetToken(TokenTypes.Passion);
                puzzleResources[PuzzleResourceId.Passion] = new PuzzleResourcePassion(passion.resourceName, passion.resourceSign);

                var sentiment = gameDefinitionProvider.GetToken(TokenTypes.Sentiment);
                puzzleResources[PuzzleResourceId.Sentiment] = new PuzzleResourceSentiment(sentiment.resourceName, sentiment.resourceSign);

                var broken = gameDefinitionProvider.GetToken(TokenTypes.Broken);
                puzzleResources[PuzzleResourceId.Broken] = new PuzzleResourceBroken(broken.resourceName, broken.resourceSign);
            }

            using (ModInterface.Log.MakeIndent("Tokens"))
            {
                foreach (var token in tokenDataDict.Values)
                {
                    token.GetExpansion().InitInternalDefinition(puzzleResources);
                }
            }
        }
    
        return new GameDataContext()
        {
            abilityDataDict = abilityDataDict,
            ailmentDataDict = ailmentDataDict,
            codeDataDict = codeDataDict,
            cutsceneDataDict = cutsceneDataDict,
            dialogTriggerDataDict = dialogTriggerDataDict,
            dlcDataDict = dlcDataDict,
            energyDataDict = energyDataDict,
            girlDataDict = girlDataDict,
            girlPairDataDict = girlPairDataDict,
            itemDataDict = itemDataDict,
            locationDataDict = locationDataDict,
            photoDataDict = photoDataDict,
            questionDataDict = questionDataDict,
            tokenDataDict = tokenDataDict,
            puzzleResources = puzzleResources,
            dollSpecialEffects = dollSpecialEffects
        };
    }

    private static Dictionary<int, T> GetDataDict<T>(GameData gameData, Type dataType, string dataName)
                => AccessTools.DeclaredField(dataType, "_definitions")
                              .GetValue(AccessTools.DeclaredField(typeof(GameData), dataName)
                              .GetValue(gameData)) as Dictionary<int, T>;
}