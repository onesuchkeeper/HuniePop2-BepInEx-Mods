// Hp2BaseMod 2021, By OneSuchKeeper

using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModLoader;
using Hp2BaseMod.Utility;
using System;
using System.Collections.Generic;

namespace Hp2BaseMod.GameDataInfo
{
    /// <summary>
    /// Serializable information to make a GirlPairFavQuestionSubDefinition
    /// </summary>
    public class GirlPairFavQuestionInfo : IGameDefinitionInfo<GirlPairFavQuestionSubDefinition>
    {
        public RelativeId? QuestionDefinitionID;

        public int? GirlResponseIndexOne;

        public int? GirlResponseIndexTwo;

        /// <summary>
        /// Constructor
        /// </summary>
        public GirlPairFavQuestionInfo() { }

        /// <summary>
        /// Constructor from a definition instance.
        /// </summary>
        /// <param name="def">The definition.</param>
        public GirlPairFavQuestionInfo(GirlPairFavQuestionSubDefinition def)
        {
            if (def == null) { throw new ArgumentNullException(nameof(def)); }

            GirlResponseIndexOne = def.girlResponseIndexOne;
            GirlResponseIndexTwo = def.girlResponseIndexTwo;

            QuestionDefinitionID = new RelativeId(def.questionDefinition);
        }

        /// <inheritdoc/>
        public void SetData(ref GirlPairFavQuestionSubDefinition def, GameDefinitionProvider gameDataProvider, AssetProvider assetProvider, InsertStyle insertStyle)
        {
            if (def == null)
            {
                def = Activator.CreateInstance<GirlPairFavQuestionSubDefinition>();
            }

            ValidatedSet.SetValue(ref def.girlResponseIndexOne, GirlResponseIndexOne);
            ValidatedSet.SetValue(ref def.girlResponseIndexTwo, GirlResponseIndexTwo);

            ValidatedSet.SetValue(ref def.questionDefinition, gameDataProvider.GetQuestion(QuestionDefinitionID), insertStyle);
        }

        public IEnumerable<string> GetInternalSpriteRequests() => null;

        public IEnumerable<string> GetInternalAudioRequests() => null;
    }
}
