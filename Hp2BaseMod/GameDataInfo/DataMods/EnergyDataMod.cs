﻿// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;
using UnityEngine;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="EnergyDefinition"/>.
    /// </summary>
    public class EnergyDataMod : DataMod, IGameDataMod<EnergyDefinition>
    {
        public string TextMaterialName;

        public IGameDefinitionInfo<Color> TextColorInfo;

        public IGameDefinitionInfo<Color> OutlineColorInfo;

        public IGameDefinitionInfo<Color> ShadowColorInfo;

        public IGameDefinitionInfo<Color> SurgeColorInfo;

        public GirlExpressionType? SurgeExpression;

        public bool? SurgeEyesClosed;
        public GirlExpressionType? NegSurgeExpression;

        public bool? NegSurgeEyesClosed;
        public GirlExpressionType? BossSurgeExpression;

        public bool? BossSurgeEyesClosed;

        public List<IGameDefinitionInfo<Sprite>> SurgeSprites;

        public List<IGameDefinitionInfo<Sprite>> BurstSprites;

        public List<IGameDefinitionInfo<Sprite>> TrailSprites;

        public List<IGameDefinitionInfo<Sprite>> SplashSprites;

        /// <inheritdoc/>
        public EnergyDataMod() { }

        public EnergyDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal EnergyDataMod(EnergyDefinition def, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            BurstSprites = def.burstSprites?.Select(x => (IGameDefinitionInfo<Sprite>)new SpriteInfoInternal(x, assetProvider)).ToList();
            TrailSprites = def.trailSprites?.Select(x => (IGameDefinitionInfo<Sprite>)new SpriteInfoInternal(x, assetProvider)).ToList();
            SplashSprites = def.splashSprites?.Select(x => (IGameDefinitionInfo<Sprite>)new SpriteInfoInternal(x, assetProvider)).ToList();
            SurgeSprites = def.surgeSprites?.Select(x => (IGameDefinitionInfo<Sprite>)new SpriteInfoInternal(x, assetProvider)).ToList();
            assetProvider.NameAndAddAsset(ref TextMaterialName, def.textMaterial);
            SurgeExpression = def.surgeExpression;
            SurgeEyesClosed = def.surgeEyesClosed;
            NegSurgeExpression = def.negSurgeExpression;
            NegSurgeEyesClosed = def.negSurgeEyesClosed;
            BossSurgeExpression = def.bossSurgeExpression;
            BossSurgeEyesClosed = def.bossSurgeEyesClosed;

            if (def.textColor != null) { TextColorInfo = new ColorInfo(def.textColor); }
            if (def.outlineColor != null) { OutlineColorInfo = new ColorInfo(def.outlineColor); }
            if (def.shadowColor != null) { ShadowColorInfo = new ColorInfo(def.shadowColor); }
            if (def.surgeColor != null) { SurgeColorInfo = new ColorInfo(def.surgeColor); }
        }

        /// <inheritdoc/>
        public void SetData(EnergyDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            ValidatedSet.SetValue(ref def.surgeExpression, SurgeExpression);
            ValidatedSet.SetValue(ref def.surgeEyesClosed, SurgeEyesClosed);
            ValidatedSet.SetValue(ref def.negSurgeExpression, NegSurgeExpression);
            ValidatedSet.SetValue(ref def.negSurgeEyesClosed, NegSurgeEyesClosed);
            ValidatedSet.SetValue(ref def.bossSurgeExpression, BossSurgeExpression);
            ValidatedSet.SetValue(ref def.bossSurgeEyesClosed, BossSurgeEyesClosed);

            ValidatedSet.SetValue(ref def.textMaterial, assetProvider.GetInternalAsset<Material>(TextMaterialName), InsertStyle);
            ValidatedSet.SetValue(ref def.textColor, TextColorInfo, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.outlineColor, OutlineColorInfo, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.shadowColor, ShadowColorInfo, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetValue(ref def.surgeColor, SurgeColorInfo, InsertStyle, gameDataProvider, assetProvider);

            ValidatedSet.SetListValue(ref def.burstSprites, BurstSprites, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.trailSprites, TrailSprites, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.splashSprites, SplashSprites, InsertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.surgeSprites, SurgeSprites, InsertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            TextColorInfo?.RequestInternals(assetProvider);
            OutlineColorInfo?.RequestInternals(assetProvider);
            ShadowColorInfo?.RequestInternals(assetProvider);
            SurgeColorInfo?.RequestInternals(assetProvider);
            SurgeSprites?.ForEach(x => x?.RequestInternals(assetProvider));
            BurstSprites?.ForEach(x => x?.RequestInternals(assetProvider));
            TrailSprites?.ForEach(x => x?.RequestInternals(assetProvider));
            SplashSprites?.ForEach(x => x?.RequestInternals(assetProvider));
        }
    }
}
