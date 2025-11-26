// Hp2BaseMod 2025, By OneSuchKeeper

using System;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Generically creates a <see cref="FunctionalCutsceneStep"/> using the provided action.
    /// The action's parameter must be invoked to signal that the step has completed.
    /// </summary>
    public class FunctionalCutsceneStepInfo : IGameDefinitionInfo<CutsceneStepSubDefinition>
    {
        private readonly Action<Action> _action;

        public FunctionalCutsceneStepInfo(Action<Action> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void SetData(ref CutsceneStepSubDefinition def, GameDefinitionProvider gameDefinitionProvider, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            _ = gameDefinitionProvider;
            _ = assetProvider;
            _ = insertStyle;

            def = new FunctionalCutsceneStep(_action);
        }

        public void RequestInternals(AssetProvider assetProvider)
        {
            // No internals
        }
    }
}
