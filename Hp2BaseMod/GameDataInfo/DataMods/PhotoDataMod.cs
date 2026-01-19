// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a PhotoDefinition
    /// </summary>
    public class PhotoDataMod : DataMod, IGameDataMod<PhotoDefinition>
    {
        public bool? HasAlts;

        public string AltFlagName;

        public RelativeId? AltCodeDefinitionID;

        public IGameDefinitionInfo<Sprite> BigPhotoCensored;
        public IGameDefinitionInfo<Sprite> BigPhotoCensoredAlt;

        public IGameDefinitionInfo<Sprite> BigPhotoUncensored;
        public IGameDefinitionInfo<Sprite> BigPhotoUncensoredAlt;

        public IGameDefinitionInfo<Sprite> BigPhotoWet;
        public IGameDefinitionInfo<Sprite> BigPhotoWetAlt;

        public IGameDefinitionInfo<Sprite> ThumbnailCensored;
        public IGameDefinitionInfo<Sprite> ThumbnailCensoredAlt;

        public IGameDefinitionInfo<Sprite> ThumbnailUncensored;
        public IGameDefinitionInfo<Sprite> ThumbnailUncensoredAlt;

        public IGameDefinitionInfo<Sprite> ThumbnailWet;
        public IGameDefinitionInfo<Sprite> ThumbnailWetAlt;

        /// <inheritdoc/>
        public PhotoDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public PhotoDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
        internal PhotoDataMod(PhotoDefinition def, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            HasAlts = def.hasAlts;
            AltFlagName = def.altFlagName;

            if (def.bigPhotoImages != null)
            {
                var it = def.bigPhotoImages.GetEnumerator();
                it.MoveNext();
                BigPhotoCensored = new SpriteInfoInternal(it.Current, assetProvider);
                it.MoveNext();
                BigPhotoUncensored = new SpriteInfoInternal(it.Current, assetProvider);
                it.MoveNext();
                BigPhotoWet = new SpriteInfoInternal(it.Current, assetProvider);
            }

            if (def.thumbnailImages != null)
            {
                var it = def.thumbnailImages.GetEnumerator();
                it.MoveNext();
                ThumbnailCensored = new SpriteInfoInternal(it.Current, assetProvider);
                it.MoveNext();
                ThumbnailUncensored = new SpriteInfoInternal(it.Current, assetProvider);
                it.MoveNext();
                ThumbnailWet = new SpriteInfoInternal(it.Current, assetProvider);
            }

            AltCodeDefinitionID = new RelativeId(def.altCodeDefinition);
        }

        /// <inheritdoc/>
        public void SetData(PhotoDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            ValidatedSet.SetValue(ref def.hasAlts, HasAlts);

            ValidatedSet.SetValue(ref def.altCodeDefinition, (CodeDefinition)gameDataProvider.GetDefinition(GameDataType.Code, AltCodeDefinitionID), InsertStyle);

            ValidatedSet.SetValue(ref def.altFlagName, AltFlagName, InsertStyle);

            ValidatedSet.SetListValue(ref def.bigPhotoImages,
                [BigPhotoCensored, BigPhotoUncensored, BigPhotoWet, BigPhotoCensoredAlt, BigPhotoUncensoredAlt, BigPhotoWetAlt],
                InsertStyle,
                gameDataProvider,
                assetProvider);

            ValidatedSet.SetListValue(ref def.thumbnailImages,
                [ThumbnailCensored, ThumbnailUncensored, ThumbnailWet, ThumbnailCensoredAlt, ThumbnailUncensoredAlt, ThumbnailWetAlt],
                InsertStyle,
                gameDataProvider,
                assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            BigPhotoCensored?.RequestInternals(assetProvider);
            BigPhotoUncensored?.RequestInternals(assetProvider);
            BigPhotoWet?.RequestInternals(assetProvider);
            ThumbnailCensored?.RequestInternals(assetProvider);
            ThumbnailUncensored?.RequestInternals(assetProvider);
            ThumbnailWet?.RequestInternals(assetProvider);
        }
    }
}
