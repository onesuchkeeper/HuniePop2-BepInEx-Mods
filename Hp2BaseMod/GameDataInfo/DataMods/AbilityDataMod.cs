// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make or modify an <see cref="AbilityDefinition"/>.
    /// Set <see cref="ScriptedAbilityFactory"/> to attach code-driven behaviour that wraps
    /// or replaces the data-driven step pipeline. Leave it null for purely data-driven abilities.
    ///
    /// Scripted behaviour executes in three stages around the data-driven pipeline:
    ///   1. <see cref="IScriptedAbility.PrePerform"/> may abort before any steps run
    ///   2. <see cref="IScriptedAbility.ReplacePerform"/> when non-null, replaces all steps
    ///   3. <see cref="IScriptedAbility.PostPerform"/> may override the final result
    ///
    /// Note: abilities are already the preferred way to implement enable/disable effects on
    /// scripted ailments via <see cref="AilmentDefinition.enableAbilityDef"/> and
    /// <see cref="AilmentDefinition.disableAbilityDef"/>. Use <see cref="IScriptedAilment"/>
    /// only for behaviour that requires direct access to trigger context or move/match/gift
    /// modifiers, which abilities cannot reach.
    /// </summary>
    public class AbilityDataMod : DataMod, IGameDataMod<AbilityDefinition>
    {
        public bool? SelectableTarget;

        public int? TargetMinimumCount;

        public IGameDefinitionInfo<TokenConditionSet> TargetConditionSetInfo;

        public List<IGameDefinitionInfo<AbilityStepSubDefinition>> Steps;

        /// <summary>
        /// Factory invoked once per <see cref="Ability"/> construction to produce scripted behaviour.
        /// Receives the newly constructed Ability so the factory can capture instance-specific context.
        /// Null if this is a purely data-driven ability.
        /// </summary>
        public Func<Ability, IScriptedAbility> ScriptedAbilityFactory;

        /// <inheritdoc/>
        public AbilityDataMod() { }

        public AbilityDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

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

            // Always write the factory null clears scripted behaviour, non-null sets it.
            ValidatedSet.SetValue(ref def.Expansion().ScriptedAbilityFactory, ScriptedAbilityFactory, InsertStyle);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Steps?.ForEach(x => x?.RequestInternals(assetProvider));
        }
    }
}