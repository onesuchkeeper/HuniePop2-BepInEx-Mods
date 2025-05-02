// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a GirlDefinition
    /// </summary>
    public class GirlDataMod : DataMod, IGirlDataMod
    {
        public List<IGirlSubDataMod<GirlPartSubDefinition>> parts;

        public List<IGirlSubDataMod<GirlExpressionSubDefinition>> expressions;

        public List<IGirlSubDataMod<ExpandedHairstyleDefinition>> hairstyles;

        public List<IGirlSubDataMod<ExpandedOutfitDefinition>> outfits;

        public List<(RelativeId, List<IGirlSubDataMod<DialogLine>>)> linesByDialogTriggerId;

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

        public List<ItemFoodType> BadFoodTypes;

        public List<RelativeId?> GirlPairDefIDs;

        #endregion

        #region Items

        public List<RelativeId?> BaggageItemDefIDs;

        public List<RelativeId?> UniqueItemDefIDs;

        public List<RelativeId?> ShoesItemDefIDs;

        #endregion

        #region Questions

        public List<GirlQuestionSubDefinition> HerQuestions;

        public List<int> FavAnswers;

        #endregion

        #region special

        public string SpecialEffectName;

        public IGameDefinitionInfo<Vector2> HeadPosition;
        public IGameDefinitionInfo<Vector2> BackPosition;

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

        public IGameDefinitionInfo<Vector2> BreathEmitterPos;

        public IGameDefinitionInfo<Vector2> UpsetEmitterPos;

        public RelativeId? PartIdBody;

        public RelativeId? PartIdNipples;

        public RelativeId? PartIdBlushLight;

        public RelativeId? PartIdBlushHeavy;

        public RelativeId? PartIdBlink;

        public RelativeId? PartIdMouthNeutral;

        public int? DefaultExpressionIndex;

        public int? FailureExpressionIndex;

        public RelativeId? DefaultHairstyleId;

        public RelativeId? DefaultOutfitId;

        public RelativeId? Phonemes_aeil;

        public RelativeId? Phonemes_neutral;

        public RelativeId? Phonemes_oquw;

        public RelativeId? Phonemes_fv;

        public RelativeId? Phonemes_other;

        public RelativeId? PhonemesTeeth_aeil;

        public RelativeId? PhonemesTeeth_neutral;

        public RelativeId? PhonemesTeeth_oquw;

        public RelativeId? PhonemesTeeth_fv;

        public RelativeId? PhonemesTeeth_other;

        public List<GirlSpecialPartSubDefinition> SpecialParts;

        #endregion

        /// <inheritdoc/>
        public GirlDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public GirlDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor from an unmodified definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
        internal GirlDataMod(GirlDefinition def, AssetProvider assetProvider, IEnumerable<DialogTriggerDefinition> dts)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            GirlName = def.girlName;
            GirlNickName = def.girlNickName;
            GirlAge = def.girlAge;
            SpecialCharacter = def.specialCharacter;
            BossCharacter = def.bossCharacter;
            FavoriteAffectionType = def.favoriteAffectionType;
            LeastFavoriteAffectionType = def.leastFavoriteAffectionType;
            VoiceVolume = def.voiceVolume;
            SexVoiceVolume = def.sexVoiceVolume;
            ShoesType = def.shoesType;
            ShoesAdj = def.shoesAdj;
            UniqueType = def.uniqueType;
            UniqueAdj = def.uniqueAdj;
            BadFoodTypes = def.badFoodTypes;
            HasAltStyles = def.hasAltStyles;
            AltStylesFlagName = def.altStylesFlagName;
            SpecialParts = def.specialParts;
            HerQuestions = def.herQuestions;
            FavAnswers = def.favAnswers;
            DefaultExpressionIndex = def.defaultExpressionIndex;
            FailureExpressionIndex = def.failureExpressionIndex;

            assetProvider.NameAndAddAsset(ref SpecialEffectName, def.specialEffectPrefab);

            GirlPairDefIDs = def.girlPairDefs?.Select(x => (RelativeId?)new RelativeId(x)).ToList();
            BaggageItemDefIDs = def.baggageItemDefs?.Select(x => (RelativeId?)new RelativeId(x)).ToList();
            UniqueItemDefIDs = def.uniqueItemDefs?.Select(x => (RelativeId?)new RelativeId(x)).ToList();
            ShoesItemDefIDs = def.shoesItemDefs?.Select(x => (RelativeId?)new RelativeId(x)).ToList();
            AltStylesCodeDefinitionID = new RelativeId(def);
            UnlockStyleCodeDefinitionID = new RelativeId(def);
            PartIdBody = new RelativeId(-1, def.partIndexBody);
            PartIdNipples = new RelativeId(-1, def.partIndexNipples);
            PartIdBlushLight = new RelativeId(-1, def.partIndexBlushLight);
            PartIdBlushHeavy = new RelativeId(-1, def.partIndexBlushHeavy);
            PartIdBlink = new RelativeId(-1, def.partIndexBlink);
            PartIdMouthNeutral = new RelativeId(-1, def.partIndexMouthNeutral);

            if (def.partIndexesPhonemes != null)
            {
                var it = def.partIndexesPhonemes.GetEnumerator();
                it.MoveNext();
                Phonemes_aeil = new RelativeId(-1, it.Current);
                it.MoveNext();
                Phonemes_neutral = new RelativeId(-1, it.Current);
                it.MoveNext();
                Phonemes_oquw = new RelativeId(-1, it.Current);
                it.MoveNext();
                Phonemes_fv = new RelativeId(-1, it.Current);
                it.MoveNext();
                Phonemes_other = new RelativeId(-1, it.Current);
            }

            if (def.partIndexesPhonemesTeeth != null)
            {
                var it = def.partIndexesPhonemesTeeth.GetEnumerator();
                it.MoveNext();
                PhonemesTeeth_aeil = new RelativeId(-1, it.Current);
                it.MoveNext();
                PhonemesTeeth_neutral = new RelativeId(-1, it.Current);
                it.MoveNext();
                PhonemesTeeth_oquw = new RelativeId(-1, it.Current);
                it.MoveNext();
                PhonemesTeeth_fv = new RelativeId(-1, it.Current);
                it.MoveNext();
                PhonemesTeeth_other = new RelativeId(-1, it.Current);
            }

            DefaultHairstyleId = new RelativeId(-1, def.defaultHairstyleIndex);
            DefaultOutfitId = new RelativeId(-1, def.defaultOutfitIndex);

            if (def.cellphonePortrait != null) { CellphonePortrait = new SpriteInfoPath(def.cellphonePortrait, assetProvider); }
            if (def.cellphonePortraitAlt != null) { CellphonePortraitAlt = new SpriteInfoPath(def.cellphonePortraitAlt, assetProvider); }
            if (def.cellphoneHead != null) { CellphoneHead = new SpriteInfoPath(def.cellphoneHead, assetProvider); }
            if (def.cellphoneHeadAlt != null) { CellphoneHeadAlt = new SpriteInfoPath(def.cellphoneHeadAlt, assetProvider); }
            if (def.cellphoneMiniHead) { CellphoneMiniHead = new SpriteInfoPath(def.cellphoneMiniHead, assetProvider); }
            if (def.cellphoneMiniHeadAlt) { CellphoneMiniHeadAlt = new SpriteInfoPath(def.cellphoneMiniHeadAlt, assetProvider); }
            if (def.breathEmitterPos != null) { BreathEmitterPos = new VectorInfo(def.breathEmitterPos); }
            if (def.upsetEmitterPos != null) { UpsetEmitterPos = new VectorInfo(def.upsetEmitterPos); }

            if (def.specialEffectOffset != null)
            {
                //Kyu's specialEffectOffset is for her back not her head
                if (def.id == Girls.KyuId.LocalId)
                {
                    BackPosition = new VectorInfo(def.specialEffectOffset);
                }
                else
                {
                    HeadPosition = new VectorInfo(def.specialEffectOffset);
                }
            }

            int i;
            if (def.parts != null)
            {
                i = 0;
                parts = def.parts.Select(x => (IGirlSubDataMod<GirlPartSubDefinition>)new GirlPartDataMod(i++, assetProvider, def)).ToList();
            }

            if (def.expressions != null)
            {
                i = 0;
                expressions = def.expressions.Select(x => (IGirlSubDataMod<GirlExpressionSubDefinition>)new GirlExpressionDataMod(i++, assetProvider, def)).ToList();
            }

            if (def.outfits != null)
            {
                i = 0;
                outfits = def.outfits.Select(x => (IGirlSubDataMod<ExpandedOutfitDefinition>)new OutfitDataMod(i++, def, assetProvider)).ToList();
            }

            if (def.hairstyles != null)
            {
                i = 0;
                hairstyles = def.hairstyles.Select(x => (IGirlSubDataMod<ExpandedHairstyleDefinition>)new HairstyleDataMod(i++, def, assetProvider)).ToList();
            }

            linesByDialogTriggerId = new List<(RelativeId, List<IGirlSubDataMod<DialogLine>>)>();

            foreach (var dialogTrigger in dts)
            {
                var dialogTirggerId = new RelativeId(dialogTrigger);

                var found = false;
                var lines = linesByDialogTriggerId.FirstOrDefault(x => { found = x.Item1 == dialogTirggerId; return found; });
                if (!found)
                {
                    lines = (dialogTirggerId, new List<IGirlSubDataMod<DialogLine>>());
                    linesByDialogTriggerId.Add(lines);
                }

                i = 0;
                foreach (var line in dialogTrigger.GetLineSetByGirl(def).dialogLines)
                {
                    lines.Item2.Add(new DialogLineDataMod(line, assetProvider, new RelativeId(-1, i)));
                    i++;
                }
            }
        }

        /// <inheritdoc/>
        public void SetData(GirlDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            var expansion = def.Expansion();

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

            ValidatedSet.SetValue(ref def.partIndexBody, expansion.PartIdToIndex, PartIdBody);
            ValidatedSet.SetValue(ref def.partIndexNipples, expansion.PartIdToIndex, PartIdNipples);
            ValidatedSet.SetValue(ref def.partIndexBlushLight, expansion.PartIdToIndex, PartIdBlushLight);
            ValidatedSet.SetValue(ref def.partIndexBlushHeavy, expansion.PartIdToIndex, PartIdBlushHeavy);
            ValidatedSet.SetValue(ref def.partIndexBlink, expansion.PartIdToIndex, PartIdBlink);
            ValidatedSet.SetValue(ref def.partIndexMouthNeutral, expansion.PartIdToIndex, PartIdMouthNeutral);

            ValidatedSet.SetValue(ref def.defaultExpressionIndex, DefaultExpressionIndex);
            ValidatedSet.SetValue(ref def.failureExpressionIndex, FailureExpressionIndex);

            ValidatedSet.SetValue(ref def.defaultHairstyleIndex, expansion.HairstyleIdToIndex, DefaultHairstyleId);
            ValidatedSet.SetValue(ref def.defaultOutfitIndex, expansion.OutfitIdToIndex, DefaultOutfitId);

            ValidatedSet.SetValue(ref def.altStylesCodeDefinition, gameDataProvider.GetCode(AltStylesCodeDefinitionID), InsertStyle);
            ValidatedSet.SetValue(ref def.unlockStyleCodeDefinition, gameDataProvider.GetCode(UnlockStyleCodeDefinitionID), InsertStyle);

            ValidatedSet.SetValue(ref def.girlName, GirlName, InsertStyle);
            ValidatedSet.SetValue(ref def.girlNickName, GirlNickName, InsertStyle);

            ValidatedSet.SetListValue(ref def.specialParts, SpecialParts, InsertStyle);
            ValidatedSet.SetListValue(ref def.herQuestions, HerQuestions, InsertStyle);
            ValidatedSet.SetListValue(ref def.favAnswers, FavAnswers, InsertStyle);
            ValidatedSet.SetValue(ref def.altStylesFlagName, AltStylesFlagName, InsertStyle);

            var partIdsPhonemes = new[]
            {
                Phonemes_aeil,
                Phonemes_neutral,
                Phonemes_oquw,
                Phonemes_fv,
                Phonemes_other
            };

            ValidatedSet.SetListValue(ref def.partIndexesPhonemes, partIdsPhonemes?
                .Select(x => x.HasValue ? (int?)expansion.PartIdToIndex[x.Value] : null), InsertStyle);

            var partIdsPhonemesTeeth = new[]
            {
                PhonemesTeeth_aeil,
                PhonemesTeeth_neutral,
                PhonemesTeeth_oquw,
                PhonemesTeeth_fv,
                PhonemesTeeth_other
            };

            ValidatedSet.SetListValue(ref def.partIndexesPhonemesTeeth, partIdsPhonemesTeeth
                .Select(x => x.HasValue ? (int?)expansion.PartIdToIndex[x.Value] : null), InsertStyle);

            ValidatedSet.SetListValue(ref def.badFoodTypes, BadFoodTypes, InsertStyle);
            ValidatedSet.SetValue(ref def.shoesAdj, ShoesAdj, InsertStyle);
            ValidatedSet.SetValue(ref def.uniqueAdj, UniqueAdj, InsertStyle);

            ValidatedSet.SetValue(ref def.cellphonePortrait, CellphonePortrait, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphonePortraitAlt, CellphonePortraitAlt, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphoneHead, CellphoneHead, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphoneHeadAlt, CellphoneHeadAlt, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphoneMiniHead, CellphoneMiniHead, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.cellphoneMiniHeadAlt, CellphoneMiniHeadAlt, InsertStyle, gameDataProvider, assetProvider);

            ValidatedSet.SetValue(ref def.breathEmitterPos, BreathEmitterPos, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.upsetEmitterPos, UpsetEmitterPos, InsertStyle, gameDataProvider, assetProvider);

            ValidatedSet.SetValue(ref def.specialEffectOffset, HeadPosition, InsertStyle, gameDataProvider, assetProvider);

            ValidatedSet.SetValue(ref def.specialEffectPrefab, assetProvider.GetInternalAsset<UiDollSpecialEffect>(SpecialEffectName), InsertStyle);

            ValidatedSet.SetListValue(ref def.girlPairDefs, GirlPairDefIDs?.Select(gameDataProvider.GetGirlPair), InsertStyle);
            ValidatedSet.SetListValue(ref def.baggageItemDefs, BaggageItemDefIDs?.Select(gameDataProvider.GetItem), InsertStyle);
            ValidatedSet.SetListValue(ref def.uniqueItemDefs, UniqueItemDefIDs?.Select(gameDataProvider.GetItem), InsertStyle);
            ValidatedSet.SetListValue(ref def.shoesItemDefs, ShoesItemDefIDs?.Select(gameDataProvider.GetItem), InsertStyle);

            ValidatedSet.SetValue(ref expansion.HeadPosition, HeadPosition, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref expansion.BackPosition, BackPosition, InsertStyle, gameDataProvider, assetProvider);
        }

        public IEnumerable<IGirlSubDataMod<GirlExpressionSubDefinition>> GetExpressions() => expressions;
        public IEnumerable<IGirlSubDataMod<ExpandedOutfitDefinition>> GetOutfits() => outfits;
        public IEnumerable<IGirlSubDataMod<ExpandedHairstyleDefinition>> GetHairstyles() => hairstyles;
        public IEnumerable<IGirlSubDataMod<GirlPartSubDefinition>> GetPartMods() => parts;

        public IEnumerable<Tuple<RelativeId, IEnumerable<IGirlSubDataMod<DialogLine>>>> GetLinesByDialogTriggerId()
            => (IEnumerable<Tuple<RelativeId, IEnumerable<IGirlSubDataMod<DialogLine>>>>)linesByDialogTriggerId;

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            parts?.ForEach(x => x?.RequestInternals(assetProvider));
            expressions?.ForEach(x => x?.RequestInternals(assetProvider));
            hairstyles?.ForEach(x => x?.RequestInternals(assetProvider));
            outfits?.ForEach(x => x?.RequestInternals(assetProvider));

            HeadPosition?.RequestInternals(assetProvider);
            BackPosition?.RequestInternals(assetProvider);
            CellphonePortrait?.RequestInternals(assetProvider);
            CellphonePortraitAlt?.RequestInternals(assetProvider);
            CellphoneHead?.RequestInternals(assetProvider);
            CellphoneHeadAlt?.RequestInternals(assetProvider);
            CellphoneMiniHead?.RequestInternals(assetProvider);
            CellphoneMiniHeadAlt?.RequestInternals(assetProvider);
            BreathEmitterPos?.RequestInternals(assetProvider);
            UpsetEmitterPos?.RequestInternals(assetProvider);

            linesByDialogTriggerId?.SelectManyNN(x => x.Item2).ForEach(x => x.RequestInternals(assetProvider));
        }
    }
}
