using System.Collections.Generic;

namespace Hp2BaseMod.GameDataInfo.Interface;

public interface IQuestionDataMod : IGameDataMod<QuestionDefinition>
{
    public IEnumerable<RelativeId> GetAnswerIds();
}