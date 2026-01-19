using System.Collections.Generic;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

public static class QuestionDefinition_Ext
{
    public static ExpandedQuestionDefinition Expansion(this QuestionDefinition def)
        => ExpandedQuestionDefinition.Get(def);

    public static RelativeId ModId(this QuestionDefinition def)
        => ModInterface.Data.GetDataId(GameDataType.Question, def.id);
}

public class ExpandedQuestionDefinition
{
    public static IdIndexMap DialogTriggerIndexes => _dialogTriggerIndexes;
    private static IdIndexMap _dialogTriggerIndexes = new();

    private static Dictionary<RelativeId, ExpandedQuestionDefinition> _expansions
        = new Dictionary<RelativeId, ExpandedQuestionDefinition>();

    public static ExpandedQuestionDefinition Get(QuestionDefinition def)
        => Get(def.id);

    public static ExpandedQuestionDefinition Get(int runtimeId)
        => Get(ModInterface.Data.GetDataId(GameDataType.Question, runtimeId));

    public static ExpandedQuestionDefinition Get(RelativeId id) => _expansions.GetOrNew(id);

    public IdIndexMap AnswerLookup => _answerLookup;
    private IdIndexMap _answerLookup = new();

    public string GetAnswer(QuestionDefinition def, RelativeId answerId) => def.questionAnswers[_answerLookup[answerId]];
}
