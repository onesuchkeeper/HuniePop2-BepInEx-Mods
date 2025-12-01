using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make an <see cref="GirlExpressionSubDefinition"/>.
    /// </summary>
    public class GirlExpressionDataMod : DataMod, IBodySubDataMod<GirlExpressionSubDefinition>
    {
        public GirlExpressionType? ExpressionType;

        public IBodySubDataMod<GirlPartSubDefinition> PartEyebrows;

        public IBodySubDataMod<GirlPartSubDefinition> PartEyes;

        public IBodySubDataMod<GirlPartSubDefinition> PartEyesGlow;

        public IBodySubDataMod<GirlPartSubDefinition> PartMouthClosed;

        public IBodySubDataMod<GirlPartSubDefinition> PartMouthOpen;

        public bool? EyesClosed;

        public bool? MouthOpen;

        /// <inheritdoc/>
        public GirlExpressionDataMod() { }

        public GirlExpressionDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        internal GirlExpressionDataMod(int index, AssetProvider assetProvider, GirlDefinition girlDef)
            : base(new RelativeId(-1, index), InsertStyle.replace, 0)
        {
            var expressionDef = girlDef.expressions[index];

            ExpressionType = expressionDef.expressionType;
            EyesClosed = expressionDef.eyesClosed;
            MouthOpen = expressionDef.mouthOpen;

            PartEyebrows = new GirlPartDataMod(expressionDef.partIndexEyebrows, assetProvider, girlDef);
            PartEyes = new GirlPartDataMod(expressionDef.partIndexEyes, assetProvider, girlDef);
            PartEyesGlow = new GirlPartDataMod(expressionDef.partIndexEyesGlow, assetProvider, girlDef);
            PartMouthClosed = new GirlPartDataMod(expressionDef.partIndexMouthClosed, assetProvider, girlDef);
            PartMouthOpen = new GirlPartDataMod(expressionDef.partIndexMouthOpen, assetProvider, girlDef);
        }

        /// <inheritdoc/>
        public void SetData(GirlExpressionSubDefinition def,
            GameDefinitionProvider gameData,
            AssetProvider assetProvider,
            RelativeId girlId,
            GirlBodySubDefinition bodyDef)
        {
            if (def == null) { return; }
            ModInterface.Log.LogInfo($"Expression Mod {Id} pre: Normal eye index:{def.partIndexEyes}, Glow eye index {def.partIndexEyesGlow}");

            ValidatedSet.SetValue(ref def.expressionType, ExpressionType);
            ValidatedSet.SetValue(ref def.eyesClosed, EyesClosed);
            ValidatedSet.SetValue(ref def.mouthOpen, MouthOpen);

            ValidatedSet.SetValue(ref def.partIndexEyebrows, bodyDef.PartIdToIndex, PartEyebrows?.Id);
            ValidatedSet.SetValue(ref def.partIndexEyes, bodyDef.PartIdToIndex, PartEyes?.Id);
            ValidatedSet.SetValue(ref def.partIndexEyesGlow, bodyDef.PartIdToIndex, PartEyesGlow?.Id);
            ValidatedSet.SetValue(ref def.partIndexMouthClosed, bodyDef.PartIdToIndex, PartMouthClosed?.Id);
            ValidatedSet.SetValue(ref def.partIndexMouthOpen, bodyDef.PartIdToIndex, PartMouthOpen?.Id);

            ModInterface.Log.LogInfo($"Expression Mod {Id} post: Normal eye index:{def.partIndexEyes}, Glow eye index {def.partIndexEyesGlow}");
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            PartEyebrows?.RequestInternals(assetProvider);
            PartEyes?.RequestInternals(assetProvider);
            PartEyesGlow?.RequestInternals(assetProvider);
            PartMouthClosed?.RequestInternals(assetProvider);
            PartMouthOpen?.RequestInternals(assetProvider);
        }

        /// <inheritdoc/>
        public IEnumerable<IBodySubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return PartEyebrows;
            yield return PartEyes;
            yield return PartEyesGlow;
            yield return PartMouthClosed;
            yield return PartMouthOpen;
        }
    }
}
