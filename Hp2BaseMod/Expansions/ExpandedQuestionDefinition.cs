using System.Collections.Generic;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

public static class QuestionDefinition_Ext
{
    public static ExpandedQuestionDefinition Expansion(this QuestionDefinition def)
        => ExpandedQuestionDefinition.Get(def);
}

public class ExpandedQuestionDefinition
{
    private static Dictionary<RelativeId, ExpandedQuestionDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedQuestionDefinition>();

    public static ExpandedQuestionDefinition Get(QuestionDefinition def)
        => Get(def.id);

    public static ExpandedQuestionDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.Question, runtimeId));

    public static ExpandedQuestionDefinition Get(RelativeId id) => _expansions.GetOrNew(id);

    public Dictionary<RelativeId, int> AnswerIdToIndex = new Dictionary<RelativeId, int>()
    {
        {RelativeId.Default, -1}
    };

    public Dictionary<int, RelativeId> AnswerIndexToId = new Dictionary<int, RelativeId>()
    {
        {-1, RelativeId.Default}
    };

    public string GetAnswer(QuestionDefinition def, RelativeId answerId) => def.questionAnswers[AnswerIdToIndex[answerId]];
}
