// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a TokenDefinition
    /// </summary>
    public class TokenDataMod : DataMod, IGameDataMod<TokenDefinition>
    {
        public string TokenName;

        public RelativeId? PuzzleResourceID;

        public RelativeId? EnergyDefinitionID;

        public int? Weight;

        public int[] DifficultyWeightOffset;

        public int? BonusWeight;

        public bool? Negative;

        public IGameDefinitionInfo<AudioKlip> SfxMatchInfo;

        public bool? AltBonusSprite;

        public IGameDefinitionInfo<Sprite> TokenSpriteInfo;

        public IGameDefinitionInfo<Sprite> OverSpriteInfo;

        public IGameDefinitionInfo<Sprite> AltTokenSpriteInfo;

        public IGameDefinitionInfo<Sprite> AltOverSpriteInfo;

        /// <inheritdoc/>
        public TokenDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public TokenDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
        internal TokenDataMod(TokenDefinition def, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            TokenName = def.tokenName;

#pragma warning disable HP001 // Deprecated member usage
            switch (def.resourceType)
            {
                case PuzzleResourceType.AFFECTION:
                    switch (def.affectionType)
                    {
                        case PuzzleAffectionType.TALENT:
                            PuzzleResourceID = PuzzleResourceId.AffectionTalent;
                            break;
                        case PuzzleAffectionType.FLIRTATION:
                            PuzzleResourceID = PuzzleResourceId.AffectionFlirtation;
                            break;
                        case PuzzleAffectionType.ROMANCE:
                            PuzzleResourceID = PuzzleResourceId.AffectionRomance;
                            break;
                        case PuzzleAffectionType.SEXUALITY:
                            PuzzleResourceID = PuzzleResourceId.AffectionSexuality;
                            break;
                    }
                    break;
                case PuzzleResourceType.BROKEN:
                    PuzzleResourceID = PuzzleResourceId.Broken;
                    break;
                case PuzzleResourceType.MOVES:
                    PuzzleResourceID = PuzzleResourceId.Moves;
                    break;
                case PuzzleResourceType.PASSION:
                    PuzzleResourceID = PuzzleResourceId.Passion;
                    break;
                case PuzzleResourceType.SENTIMENT:
                    PuzzleResourceID = PuzzleResourceId.Sentiment;
                    break;
                case PuzzleResourceType.STAMINA:
                    PuzzleResourceID = PuzzleResourceId.Stamina;
                    break;
            }
#pragma warning restore HP001 // Deprecated member usage

            Weight = def.weight;
            DifficultyWeightOffset = def.difficultyWeightOffset;
            BonusWeight = def.bonusWeight;
            Negative = def.negative;
            AltBonusSprite = def.altBonusSprite;

            if (def.tokenSprite != null) TokenSpriteInfo = new SpriteInfoInternal(def.tokenSprite, assetProvider);
            if (def.overSprite != null) OverSpriteInfo = new SpriteInfoInternal(def.overSprite, assetProvider);
            if (def.altTokenSprite != null) AltTokenSpriteInfo = new SpriteInfoInternal(def.altTokenSprite, assetProvider);
            if (def.altOverSprite != null) AltOverSpriteInfo = new SpriteInfoInternal(def.altOverSprite, assetProvider);
            if (def.sfxMatch != null) SfxMatchInfo = new AudioKlipInfo(def.sfxMatch, assetProvider);

            EnergyDefinitionID = new RelativeId(def.energyDefinition);
        }

        /// <inheritdoc/>
        public void SetData(TokenDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            var exp = def.GetExpansion();
            if (PuzzleResourceID.HasValue) exp.PuzzleResource = gameDataProvider.GetPuzzleResource(PuzzleResourceID.Value);

            ValidatedSet.SetValue(ref def.weight, Weight);
            ValidatedSet.SetValue(ref def.bonusWeight, BonusWeight);
            ValidatedSet.SetValue(ref def.negative, Negative);
            ValidatedSet.SetValue(ref def.altBonusSprite, AltBonusSprite);
            ValidatedSet.SetValue(ref def.energyDefinition, gameDataProvider.GetEnergy(EnergyDefinitionID), InsertStyle);

            ValidatedSet.SetValue(ref def.tokenName, TokenName, InsertStyle);
            ValidatedSet.SetValue(ref def.difficultyWeightOffset, DifficultyWeightOffset, InsertStyle);

            ValidatedSet.SetValue(ref def.tokenSprite, TokenSpriteInfo, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.overSprite, OverSpriteInfo, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.altTokenSprite, AltTokenSpriteInfo, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.altOverSprite, AltOverSpriteInfo, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.sfxMatch, SfxMatchInfo, InsertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            SfxMatchInfo?.RequestInternals(assetProvider);
            TokenSpriteInfo?.RequestInternals(assetProvider);
            OverSpriteInfo?.RequestInternals(assetProvider);
            AltTokenSpriteInfo?.RequestInternals(assetProvider);
            AltOverSpriteInfo?.RequestInternals(assetProvider);
        }
    }
}
