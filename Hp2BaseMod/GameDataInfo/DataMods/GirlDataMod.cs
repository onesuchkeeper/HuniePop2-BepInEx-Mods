// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Extension.IEnumerableExtension;
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
        public List<(RelativeId, List<IDialogLineDataMod>)> linesByDialogTriggerId;

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
            HerQuestions = def.herQuestions;
            FavAnswers = def.favAnswers;

            GirlPairDefIDs = def.girlPairDefs?.Select(x => (RelativeId?)new RelativeId(x)).ToList();
            BaggageItemDefIDs = def.baggageItemDefs?.Select(x => (RelativeId?)new RelativeId(x)).ToList();
            UniqueItemDefIDs = def.uniqueItemDefs?.Select(x => (RelativeId?)new RelativeId(x)).ToList();
            ShoesItemDefIDs = def.shoesItemDefs?.Select(x => (RelativeId?)new RelativeId(x)).ToList();
            AltStylesCodeDefinitionID = new RelativeId(def);
            UnlockStyleCodeDefinitionID = new RelativeId(def);

            if (def.cellphonePortrait != null) { CellphonePortrait = new SpriteInfoInternal(def.cellphonePortrait, assetProvider); }
            if (def.cellphonePortraitAlt != null) { CellphonePortraitAlt = new SpriteInfoInternal(def.cellphonePortraitAlt, assetProvider); }
            if (def.cellphoneHead != null) { CellphoneHead = new SpriteInfoInternal(def.cellphoneHead, assetProvider); }
            if (def.cellphoneHeadAlt != null) { CellphoneHeadAlt = new SpriteInfoInternal(def.cellphoneHeadAlt, assetProvider); }
            if (def.cellphoneMiniHead) { CellphoneMiniHead = new SpriteInfoInternal(def.cellphoneMiniHead, assetProvider); }
            if (def.cellphoneMiniHeadAlt) { CellphoneMiniHeadAlt = new SpriteInfoInternal(def.cellphoneMiniHeadAlt, assetProvider); }

            linesByDialogTriggerId = new();

            int i;
            foreach (var dialogTrigger in dts)
            {
                var dialogTriggerId = new RelativeId(dialogTrigger);

                var found = false;
                var lines = linesByDialogTriggerId.FirstOrDefault(x => { found = x.Item1 == dialogTriggerId; return found; });
                if (!found)
                {
                    lines = (dialogTriggerId, new());
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

            ValidatedSet.SetListValue(ref def.herQuestions, HerQuestions, InsertStyle);
            ValidatedSet.SetListValue(ref def.favAnswers, FavAnswers, InsertStyle);
            ValidatedSet.SetValue(ref def.altStylesFlagName, AltStylesFlagName, InsertStyle);

            ValidatedSet.SetListValue(ref def.badFoodTypes, BadFoodTypes, InsertStyle);
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
        }

        /// <inheritdoc/>
        public IEnumerable<IGirlBodyDataMod> GetBodyMods() => bodies;

        /// <inheritdoc/>
        public IEnumerable<(RelativeId, IEnumerable<IDialogLineDataMod>)> GetLinesByDialogTriggerId()
            => linesByDialogTriggerId?.Select(x => (x.Item1, x.Item2.Select(x => x)));

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            CellphonePortrait?.RequestInternals(assetProvider);
            CellphonePortraitAlt?.RequestInternals(assetProvider);
            CellphoneHead?.RequestInternals(assetProvider);
            CellphoneHeadAlt?.RequestInternals(assetProvider);
            CellphoneMiniHead?.RequestInternals(assetProvider);
            CellphoneMiniHeadAlt?.RequestInternals(assetProvider);

            linesByDialogTriggerId?.SelectManyNN(x => x.Item2).ForEach(x => x.RequestInternals(assetProvider));

            bodies?.ForEach(x => x.RequestInternals(assetProvider));
        }
    }
}
