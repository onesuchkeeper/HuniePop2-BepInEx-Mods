using System.Collections.Generic;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface IFavQuestionDataMod : IGameDataMod<QuestionDefinition>
{
    /// <summary>
    /// The answer ids used by this mod. Exposed so they can be pre-allocated and given indexes.
    /// </summary>
    public IEnumerable<RelativeId> GetAnswerIds();
}
