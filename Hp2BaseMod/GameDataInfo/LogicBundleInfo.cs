// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModLoader;
using Hp2BaseMod.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hp2BaseMod.GameDataInfo
{
    public class LogicBundleInfo : IGameDefinitionInfo<LogicBundle>
    {
        public List<IGameDefinitionInfo<LogicCondition>> Conditions;

        public List<IGameDefinitionInfo<LogicAction>> Actions;

        /// <summary>
        /// Constructor
        /// </summary>
        public LogicBundleInfo() { }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <param name="assetProvider">Asset provider containing the assest referenced by the definition.</param>
        public LogicBundleInfo(LogicBundle def, AssetProvider assetProvider)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }
            if (assetProvider == null) { throw new ArgumentNullException(nameof(assetProvider)); }

            if (def.conditions != null) { Conditions = def.conditions.Select(x => (IGameDefinitionInfo<LogicCondition>)new LogicConditionInfo(x)).ToList(); }
            if (def.actions != null) { Actions = def.actions.Select(x => (IGameDefinitionInfo<LogicAction>)new LogicActionInfo(x, assetProvider)).ToList(); }
        }

        /// <inheritdoc/>
        public void SetData(ref LogicBundle def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<LogicBundle>();
            }

            ValidatedSet.SetListValue(ref def.conditions, Conditions, insertStyle, gameDataProvider, assetProvider);
            ValidatedSet.SetListValue(ref def.actions, Actions, insertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalSpriteRequests() => Conditions.OrEmptyIfNull()
            .SelectManyNN(x => x.GetInternalSpriteRequests())
            .ConcatNN(Actions?.SelectManyNN(x => x.GetInternalSpriteRequests()));

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalAudioRequests() => Conditions.OrEmptyIfNull()
            .SelectManyNN(x => x.GetInternalAudioRequests())
            .ConcatNN(Actions?.SelectManyNN(x => x.GetInternalAudioRequests()));
    }
}
