// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make an AbilityDefinition
    /// </summary>
    public class AbilityDataMod : DataMod, IGameDataMod<AbilityDefinition>
    {
        public bool? SelectableTarget;

        public int? TargetMinimumCount;

        public IGameDefinitionInfo<TokenConditionSet> TargetConditionSetInfo;

        public List<IGameDefinitionInfo<AbilityStepSubDefinition>> Steps;

        /// <inheritdoc/>
        public AbilityDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public AbilityDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assest referenced by the definition.</param>
        internal AbilityDataMod(AbilityDefinition def, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            SelectableTarget = def.selectableTarget;
            TargetMinimumCount = def.targetMinimumCount;

            if (def.targetConditionSet != null) { TargetConditionSetInfo = new TokenConditionSetInfo(def.targetConditionSet); }
            Steps = def.steps?.Select(x => (IGameDefinitionInfo<AbilityStepSubDefinition>)new AbilityStepInfo(x, assetProvider)).ToList();
        }

        /// <inheritdoc/>
        public void SetData(AbilityDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            ValidatedSet.SetValue(ref def.selectableTarget, SelectableTarget);
            ValidatedSet.SetValue(ref def.targetMinimumCount, TargetMinimumCount);

            ValidatedSet.SetValue(ref def.targetConditionSet, TargetConditionSetInfo, InsertStyle, gameDataProvider, assetProvider);

            ValidatedSet.SetListValue(ref def.steps, Steps, InsertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Steps?.ForEach(x => x?.RequestInternals(assetProvider));
        }
    }
}
