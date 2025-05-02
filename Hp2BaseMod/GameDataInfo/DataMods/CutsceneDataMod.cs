// Hp2BaseMod 2021, By OneSuchKeeper

using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a CutsceneDefinition
    /// </summary>
    public class CutsceneDataMod : DataMod, IGameDataMod<CutsceneDefinition>
    {
        public CutsceneCleanUpType? CleanUpType;

        public List<IGameDefinitionInfo<CutsceneStepSubDefinition>> Steps;

        /// <inheritdoc/>
        public CutsceneDataMod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="insertStyle">The way in which mod data should be applied to the data instance.</param>
        public CutsceneDataMod(RelativeId id, InsertStyle insertStyle, int loadPriority = 0)
            : base(id, insertStyle, loadPriority)
        {
        }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assets referenced by the definition.</param>
        public CutsceneDataMod(CutsceneDefinition def, AssetProvider assetProvider)
            : base(new RelativeId(def), InsertStyle.replace, 0)
        {
            CleanUpType = def.cleanUpType;
            Steps = def.steps?.Select(x => (IGameDefinitionInfo<CutsceneStepSubDefinition>)new CutsceneStepInfo(x, assetProvider)).ToList();
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
