// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="GirlDefinition"/>.
    /// </summary>
    public class GirlDataMod : DataMod, IGirlDataMod
    {
        public List<(RelativeId, List<IDialogLineDataMod>)> LinesByDialogTriggerId;

        public Dictionary<RelativeId, IDialogLineDataMod> FavoriteDialogLines;

        public Dictionary<RelativeId, IDialogLineDataMod> LocationGreetingDialogLines;

        #region Girl Info

        public string GirlName;

        public string GirlNickName;

        public int? GirlAge;

        public bool? SpecialCharacter;

        public bool? BossCharacter;

        public PuzzleAffectionType? FavoriteAffectionType;

        public PuzzleAffectionType? LeastFavoriteAffectionType;

        public ItemShoesType? ShoesType;

        public string ShoesAdj;

        public ItemUniqueType? UniqueType;

        public string UniqueAdj;

        public float? VoiceVolume;

        public float? SexVoiceVolume;

        public ItemFoodType? BadFoodType;//in game it's a list, but the code literally only checks the first index...

        public List<RelativeId?> GirlPairDefIDs;

        #endregion

        #region Items

        public List<RelativeId?> BaggageItemDefIDs;

        public List<RelativeId?> UniqueItemDefIDs;

        public List<RelativeId?> ShoesItemDefIDs;

        #endregion

        #region Questions

        public Dictionary<RelativeId, IHerQuestionDataInfo> HerQuestions;

        public Dictionary<RelativeId, RelativeId> FavAnswers;

        #endregion

        #region special

        public bool? HasAltStyles;

        public string AltStylesFlagName;

        public RelativeId? AltStylesCodeDefinitionID;

        public RelativeId? UnlockStyleCodeDefinitionID;

        #endregion

        #region Sprites

        public IGameDefinitionInfo<Sprite> CellphonePortrait;

        public IGameDefinitionInfo<Sprite> CellphonePortraitAlt;

        public IGameDefinitionInfo<Sprite> CellphoneHead;

        public IGameDefinitionInfo<Sprite> CellphoneHeadAlt;

        public IGameDefinitionInfo<Sprite> CellphoneMiniHead;

        public IGameDefinitionInfo<Sprite> CellphoneMiniHeadAlt;

        #endregion

        public List<IGirlBodyDataMod> bodies;

        /// <inheritdoc/>
        public GirlDataMod() { }

        public GirlDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <inheritdoc/>
        public void SetData(GirlDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            var girlExp = ExpandedGirlDefinition.Get(Id);

            ValidatedSet.SetValue(ref def.girlAge, GirlAge);
            ValidatedSet.SetValue(ref def.specialCharacter, SpecialCharacter);
            ValidatedSet.SetValue(ref def.bossCharacter, BossCharacter);
            ValidatedSet.SetValue(ref def.favoriteAffectionType, FavoriteAffectionType);
            ValidatedSet.SetValue(ref def.leastFavoriteAffectionType, LeastFavoriteAffectionType);
            ValidatedSet.SetValue(ref def.voiceVolume, VoiceVolume);
            ValidatedSet.SetValue(ref def.sexVoiceVolume, SexVoiceVolume);
            ValidatedSet.SetValue(ref def.shoesType, ShoesType);
            ValidatedSet.SetValue(ref def.uniqueType, UniqueType);
            ValidatedSet.SetValue(ref def.hasAltStyles, HasAltStyles);

            ValidatedSet.SetValue(ref def.altStylesCodeDefinition, gameDataProvider.GetCode(AltStylesCodeDefinitionID), InsertStyle);
            ValidatedSet.SetValue(ref def.unlockStyleCodeDefinition, gameDataProvider.GetCode(UnlockStyleCodeDefinitionID), InsertStyle);

            ValidatedSet.SetValue(ref def.girlName, GirlName, InsertStyle);
            ValidatedSet.SetValue(ref def.girlNickName, GirlNickName, InsertStyle);

            if (HerQuestions != null)
            {
                var herQuestionDt = gameDataProvider.GetDialogTrigger(DialogTriggers.HerQuestion);
                var herQuestionGoodResponseDt = gameDataProvider.GetDialogTrigger(DialogTriggers.HerQuestionGoodResponse);
                var herQuestionBadResponseDt = gameDataProvider.GetDialogTrigger(DialogTriggers.HerQuestionBadResponse);

                var herQuestionDtSet = herQuestionDt.Expansion().GetLineSetOrNew(herQuestionDt, Id);
                var herQuestionGoodResponseDtSet = herQuestionGoodResponseDt.Expansion().GetLineSetOrNew(herQuestionGoodResponseDt, Id);
                var herQuestionBadResponseDtSet = herQuestionBadResponseDt.Expansion().GetLineSetOrNew(herQuestionBadResponseDt, Id);

                foreach (var id_questionMod in HerQuestions)
                {
                    var questionIndex = girlExp.HerQuestionIdToIndex[id_questionMod.Key];
                    var question = def.herQuestions.GetOrNew(questionIndex);
                    id_questionMod.Value.SetData(question,
                        herQuestionDtSet.dialogLines.GetOrNew(questionIndex),
                        girlExp.GetQuestionAnswerIndexMap(id_questionMod.Key),
                        () =>
                        {
                            var index = girlExp.HerQuestionGoodResponseIdToDtIndex[id_questionMod.Key];
                            return (herQuestionGoodResponseDtSet.dialogLines.GetOrNew(index), index);
                        },
                        (id) =>
                        {
                            var index = girlExp.HerQuestionBadResponseIdToDtIndex[id];
                            return (herQuestionBadResponseDtSet.dialogLines.GetOrNew(index), index);
                        },
                        gameDataProvider,
                        assetProvider);
                }
            }

            ValidatedSet.SetDictValues(ref girlExp.FavQuestionIdToAnswerId, FavAnswers, InsertStyle);
            ValidatedSet.SetValue(ref def.altStylesFlagName, AltStylesFlagName, InsertStyle);
            if (BadFoodType.HasValue)
            {
                //the code only looks at the 0th index for some reason...
                if (def.badFoodTypes == null)
                {
                    def.badFoodTypes = new() { BadFoodType.Value };
                }
                else if (!def.badFoodTypes.Any())
                {
                    def.badFoodTypes.Add(BadFoodType.Value);
                }
                else
                {
                    def.badFoodTypes[0] = BadFoodType.Value;
                }
            }

            ValidatedSet.SetValue(ref def.shoesAdj, ShoesAdj, InsertStyle);
            ValidatedSet.SetValue(ref def.uniqueAdj, UniqueAdj, InsertStyle);

            ValidatedSet.SetValue(ref def.cellphonePortrait, CellphonePortrait, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphonePortraitAlt, CellphonePortraitAlt, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphoneHead, CellphoneHead, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphoneHeadAlt, CellphoneHeadAlt, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphoneMiniHead, CellphoneMiniHead, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphoneMiniHeadAlt, CellphoneMiniHeadAlt, InsertStyle, gameDataProvider, assetProvider);

            ValidatedSet.SetListValue(ref def.girlPairDefs, GirlPairDefIDs?.Select(gameDataProvider.GetGirlPair), InsertStyle);
            ValidatedSet.SetListValue(ref def.baggageItemDefs, BaggageItemDefIDs?.Select(gameDataProvider.GetItem), InsertStyle);
            ValidatedSet.SetListValue(ref def.uniqueItemDefs, UniqueItemDefIDs?.Select(gameDataProvider.GetItem), InsertStyle);
            ValidatedSet.SetListValue(ref def.shoesItemDefs, ShoesItemDefIDs?.Select(gameDataProvider.GetItem), InsertStyle);

            if (FavoriteDialogLines != null)
            {
                var favQuestionResponse = gameDataProvider.GetDialogTrigger(DialogTriggers.FavQuestionResponse);

                if (favQuestionResponse == null)
                {
                    ModInterface.Log.Error("Failed to find FavQuestionResponse dialog trigger.");
                }
                else
                {
                    var lineSet = favQuestionResponse.dialogLineSets.GetOrNew(ExpandedGirlDefinition.DialogTriggerIndexes[Id]);
                    foreach (var id_line in FavoriteDialogLines)
                    {
                        var index = ExpandedQuestionDefinition.DialogTriggerIndexes[id_line.Key];
                        var line = lineSet.dialogLines.GetOrNew(index);
                        id_line.Value.SetData(line, gameDataProvider, assetProvider);
                    }
                }
            }

            if (LocationGreetingDialogLines != null)
            {
                var dateGreeting = gameDataProvider.GetDialogTrigger(DialogTriggers.DateGreeting);

                if (dateGreeting == null)
                {
                    ModInterface.Log.Error("Failed to find DateGreeting dialog trigger.");
                }
                else
                {
                    var lineSet = dateGreeting.dialogLineSets.GetOrNew(ExpandedGirlDefinition.DialogTriggerIndexes[Id]);
                    foreach (var id_line in LocationGreetingDialogLines)
                    {
                        var index = girlExp.DateGreetingLocIdToDtIndex[id_line.Key];
                        var line = lineSet.dialogLines.GetOrNew(index);
                        id_line.Value.SetData(line, gameDataProvider, assetProvider);
                    }
                }
            }

            if (LinesByDialogTriggerId != null)
            {
                foreach ((RelativeId dtId, var mods) in LinesByDialogTriggerId)
                {
                    var dt = gameDataProvider.GetDialogTrigger(dtId);
                    var dtExp = dt.Expansion();

                    foreach (var mod in mods)
                    {
                        var line = dtExp.GetLineOrNew(dt, Id, mod.Id);
                        mod.SetData(line, gameDataProvider, assetProvider);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IGirlBodyDataMod> GetBodyMods() => bodies;

        // /// <inheritdoc/>
        // public IEnumerable<(RelativeId, IEnumerable<IDialogLineDataMod>)> GetLinesByDialogTriggerId()
        //     => LinesByDialogTriggerId?.Select(x => (x.Item1, x.Item2.Select(x => x)));

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            CellphonePortrait?.RequestInternals(assetProvider);
            CellphonePortraitAlt?.RequestInternals(assetProvider);
            CellphoneHead?.RequestInternals(assetProvider);
            CellphoneHeadAlt?.RequestInternals(assetProvider);
            CellphoneMiniHead?.RequestInternals(assetProvider);
            CellphoneMiniHeadAlt?.RequestInternals(assetProvider);

            LinesByDialogTriggerId?.SelectManyNN(x => x.Item2).ForEach(x => x.RequestInternals(assetProvider));

            bodies?.ForEach(x => x.RequestInternals(assetProvider));
        }
    }
}
