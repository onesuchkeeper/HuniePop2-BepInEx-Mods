// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a CutsceneBranchSubDefinition
    /// </summary>
    public class CutsceneBranchInfo : IGameDefinitionInfo<CutsceneBranchSubDefinition>
    {
        public RelativeId? CutsceneDefinitionID;

        public List<IGameDefinitionInfo<LogicCondition>> Conditions;

        public List<IGameDefinitionInfo<CutsceneStepSubDefinition>> Steps;

        /// <inheritdoc/>
        public void SetData(ref CutsceneBranchSubDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<CutsceneBranchSubDefinition>();
            }

            ValidatedSet.SetValue(ref def.cutsceneDefinition, gameDataProvider.GetCutscene(CutsceneDefinitionID), insertStyle);

            ValidatedSet.SetListValue(ref def.conditions, Conditions, insertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.steps, Steps, insertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Conditions?.ForEach(x => x?.RequestInternals(assetProvider));
            Steps?.ForEach(x => x?.RequestInternals(assetProvider));
        }
    }
}
