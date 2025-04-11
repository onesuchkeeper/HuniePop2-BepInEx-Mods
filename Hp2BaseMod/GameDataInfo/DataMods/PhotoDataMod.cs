﻿// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModLoader;
using Hp2BaseMod.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a PhotoDefinition
    /// </summary>
    public class PhotoDataMod : DataMod, IGameDataMod<PhotoDefinition>
    {
        public RelativeId? GirlPairDefinitionID;

        public bool? HasAlts;

        public string AltFlagName;

        public RelativeId? AltCodeDefinitionID;

        IGameDefinitionInfo<Sprite> BigPhotoCensored;

        IGameDefinitionInfo<Sprite> BigPhotoUncensored;

        IGameDefinitionInfo<Sprite> BigPhotoWet;

        IGameDefinitionInfo<Sprite> ThumbnailCensored;

        IGameDefinitionInfo<Sprite> ThumbnailUncensored;

        IGameDefinitionInfo<Sprite> ThumbnailWet;

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
        /// <param name="assetProvider">Asset provider containing the assest referenced by the definition.</param>
        internal PhotoDataMod(PhotoDefinition def, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            HasAlts = def.hasAlts;
            AltFlagName = def.altFlagName;

            if (def.bigPhotoImages != null)
            {
                var it = def.bigPhotoImages.GetEnumerator();
                it.MoveNext();
                BigPhotoCensored = new SpriteInfoPath(it.Current, assetProvider);
                it.MoveNext();
                BigPhotoUncensored = new SpriteInfoPath(it.Current, assetProvider);
                it.MoveNext();
                BigPhotoWet = new SpriteInfoPath(it.Current, assetProvider);
            }

            if (def.thumbnailImages != null)
            {
                var it = def.thumbnailImages.GetEnumerator();
                it.MoveNext();
                ThumbnailCensored = new SpriteInfoPath(it.Current, assetProvider);
                it.MoveNext();
                ThumbnailUncensored = new SpriteInfoPath(it.Current, assetProvider);
                it.MoveNext();
                ThumbnailWet = new SpriteInfoPath(it.Current, assetProvider);
            }

            AltCodeDefinitionID = new RelativeId(def.altCodeDefinition);
            GirlPairDefinitionID = new RelativeId(def.girlPairDefinition);
        }

        /// <inheritdoc/>
        public void SetData(PhotoDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            ValidatedSet.SetValue(ref def.hasAlts, HasAlts);

            ValidatedSet.SetValue(ref def.girlPairDefinition, (GirlPairDefinition)gameDataProvider.GetDefinition(GameDataType.GirlPair, GirlPairDefinitionID), InsertStyle);
            ValidatedSet.SetValue(ref def.altCodeDefinition, (CodeDefinition)gameDataProvider.GetDefinition(GameDataType.Code, AltCodeDefinitionID), InsertStyle);

            ValidatedSet.SetValue(ref def.altFlagName, AltFlagName, InsertStyle);

            ValidatedSet.SetListValue(ref def.bigPhotoImages, new[] { BigPhotoCensored, BigPhotoUncensored, BigPhotoWet }, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.thumbnailImages, new[] { ThumbnailCensored, ThumbnailUncensored, ThumbnailWet }, InsertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalSpriteRequests() => IEnumerableExtension.OrEmptyIfNull(BigPhotoCensored?.GetInternalSpriteRequests())
            .ConcatNN(BigPhotoUncensored?.GetInternalSpriteRequests())
            .ConcatNN(BigPhotoWet?.GetInternalSpriteRequests())
            .ConcatNN(ThumbnailCensored?.GetInternalSpriteRequests())
            .ConcatNN(ThumbnailUncensored?.GetInternalSpriteRequests())
            .ConcatNN(ThumbnailWet?.GetInternalSpriteRequests());

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalAudioRequests() => IEnumerableExtension.OrEmptyIfNull(BigPhotoCensored?.GetInternalAudioRequests())
            .ConcatNN(BigPhotoUncensored?.GetInternalAudioRequests())
            .ConcatNN(BigPhotoWet?.GetInternalAudioRequests())
            .ConcatNN(ThumbnailCensored?.GetInternalAudioRequests())
            .ConcatNN(ThumbnailUncensored?.GetInternalAudioRequests())
            .ConcatNN(ThumbnailWet?.GetInternalAudioRequests());
    }
}
