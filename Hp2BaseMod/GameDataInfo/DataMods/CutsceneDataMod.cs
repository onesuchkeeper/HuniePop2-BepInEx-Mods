// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Information to make a <see cref="CutsceneDefinition"/>.
    /// </summary>
    public class CutsceneDataMod : DataMod, IGameDataMod<CutsceneDefinition>
    {
        public CutsceneCleanUpType? CleanUpType;

        public List<IGameDefinitionInfo<CutsceneStepSubDefinition>> Steps;

        /// <inheritdoc/>
        public CutsceneDataMod() { }

        public CutsceneDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <inheritdoc/>
        public void SetData(CutsceneDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider)
        {
            ValidatedSet.SetValue(ref def.cleanUpType, CleanUpType);
            ValidatedSet.SetListValue(ref def.steps, Steps, InsertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public void RequestInternals(AssetProvider assetProvider)
        {
            Steps?.ForEach(x => x?.RequestInternals(assetProvider));
        }
    }
}
