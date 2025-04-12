// Hp2BaseMod 2021, By OneSuchKeeper

using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod.GameDataInfo
{
    public class TokenConditionSetInfo : IGameDefinitionInfo<TokenConditionSet>
    {
        public List<IGameDefinitionInfo<TokenCondition>> Conditions;

        /// <summary>
        /// Constructor
        /// </summary>
        public TokenConditionSetInfo() { }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        public TokenConditionSetInfo(TokenConditionSet def)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }

            if (def.conditions != null)
            {
                Conditions = def.conditions
                    .Select(x => (IGameDefinitionInfo<TokenCondition>)new TokenConditionInfo(x))
                    .ToList();
            }
        }

        /// <inheritdoc/>
        public void SetData(ref TokenConditionSet def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<TokenConditionSet>();
            }

            ValidatedSet.SetListValue(ref def.conditions, Conditions, insertStyle, gameDataProvider, assetProvider);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalSpriteRequests() => Conditions.OrEmptyIfNull()
            .SelectManyNN(x => x.GetInternalSpriteRequests());

        /// <inheritdoc/>
        public IEnumerable<string> GetInternalAudioRequests() => Conditions.OrEmptyIfNull()
            .SelectManyNN(x => x.GetInternalAudioRequests());
    }
}
