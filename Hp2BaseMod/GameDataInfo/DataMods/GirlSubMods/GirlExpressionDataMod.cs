using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make an <see cref="GirlExpressionSubDefinition"/>.
    /// </summary>
    public class GirlExpressionDataMod : DataMod, IGirlSubDataMod<GirlExpressionSubDefinition>
    {
        public GirlExpressionType? ExpressionType;

        public IGirlSubDataMod<GirlPartSubDefinition> PartEyebrows;

        public IGirlSubDataMod<GirlPartSubDefinition> PartEyes;

        public IGirlSubDataMod<GirlPartSubDefinition> PartEyesGlow;

        public IGirlSubDataMod<GirlPartSubDefinition> PartMouthClosed;

        public IGirlSubDataMod<GirlPartSubDefinition> PartMouthOpen;

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
        public void SetData(ref GirlExpressionSubDefinition def,
            GameDefinitionProvider gameData,
            AssetProvider assetProvider,
            InsertStyle insertStyle,
            RelativeId girlId,
            GirlDefinition girlDef)
        {
            var girlExpansion = ExpandedGirlDefinition.Get(girlId);

            ValidatedSet.SetValue(ref def.expressionType, ExpressionType);
            ValidatedSet.SetValue(ref def.eyesClosed, EyesClosed);
            ValidatedSet.SetValue(ref def.mouthOpen, MouthOpen);

            ValidatedSet.SetValue(ref def.partIndexEyebrows, girlExpansion.PartIdToIndex, PartEyebrows?.Id);
            ValidatedSet.SetValue(ref def.partIndexEyes, girlExpansion.PartIdToIndex, PartEyes?.Id);
            ValidatedSet.SetValue(ref def.partIndexEyesGlow, girlExpansion.PartIdToIndex, PartEyesGlow?.Id);
            ValidatedSet.SetValue(ref def.partIndexMouthClosed, girlExpansion.PartIdToIndex, PartMouthClosed?.Id);
            ValidatedSet.SetValue(ref def.partIndexMouthOpen, girlExpansion.PartIdToIndex, PartMouthOpen?.Id);
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
        public IEnumerable<IGirlSubDataMod<GirlPartSubDefinition>> GetPartDataMods()
        {
            yield return PartEyebrows;
            yield return PartEyes;
            yield return PartEyesGlow;
            yield return PartMouthClosed;
            yield return PartMouthOpen;
        }
    }
}
