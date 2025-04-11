﻿using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModLoader;
using Hp2BaseMod.Utility;
using System;
using System.Collections.Generic;

namespace Hp2BaseMod.GameDataInfo
{
    public class GirlExpressionDataMod : DataMod, IGirlSubDataMod<GirlExpressionSubDefinition>
    {
        public GirlExpressionType? ExpressionType;

        public RelativeId? PartIdEyebrows;

        public RelativeId? PartIdEyes;

        public RelativeId? PartIdEyesGlow;

        public RelativeId? PartIdMouthClosed;

        public RelativeId? PartIdMouthOpen;

        public bool? EyesClosed;

        public bool? MouthOpen;

        /// <inheritdoc/>
        public GirlExpressionDataMod() { }

        public GirlExpressionDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GirlExpressionDataMod(int index, AssetProvider assetProvider, GirlDefinition girlDef)
            : base(new RelativeId() { SourceId = -1, LocalId = index }, InsertStyle.replace, 0)
        {
            var expressionDef = girlDef.expressions[index];

            ExpressionType = expressionDef.expressionType;
            EyesClosed = expressionDef.eyesClosed;
            MouthOpen = expressionDef.mouthOpen;

            PartIdEyebrows = new RelativeId(-1, expressionDef.partIndexEyebrows);
            PartIdEyes = new RelativeId(-1, expressionDef.partIndexEyes);
            PartIdEyesGlow = new RelativeId(-1, expressionDef.partIndexEyesGlow);
            PartIdMouthClosed = new RelativeId(-1, expressionDef.partIndexMouthClosed);
            PartIdMouthOpen = new RelativeId(-1, expressionDef.partIndexMouthOpen);
        }

        /// <inheritdoc/>
        public void SetData(ref GirlExpressionSubDefinition def, GameDefinitionProvider gameData, AssetProvider assetProvider, InsertStyle insertStyle, RelativeId girlId)
        {
            ValidatedSet.SetValue(ref def.expressionType, ExpressionType);
            ValidatedSet.SetValue(ref def.eyesClosed, EyesClosed);
            ValidatedSet.SetValue(ref def.mouthOpen, MouthOpen);

            ValidatedSet.SetValue(ref def.partIndexEyebrows, ModInterface.Data.GetPartIndex(girlId, PartIdEyebrows));
            ValidatedSet.SetValue(ref def.partIndexEyes, ModInterface.Data.GetPartIndex(girlId, PartIdEyes));
            ValidatedSet.SetValue(ref def.partIndexEyesGlow, ModInterface.Data.GetPartIndex(girlId, PartIdEyesGlow));
            ValidatedSet.SetValue(ref def.partIndexMouthClosed, ModInterface.Data.GetPartIndex(girlId, PartIdMouthClosed));
            ValidatedSet.SetValue(ref def.partIndexMouthOpen, ModInterface.Data.GetPartIndex(girlId, PartIdMouthOpen));
        }

        public IEnumerable<string> GetInternalSpriteRequests() => null;

        public IEnumerable<string> GetInternalAudioRequests() => null;
    }
}
