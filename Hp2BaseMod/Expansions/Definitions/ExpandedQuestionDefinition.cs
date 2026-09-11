using Hp2BaseMod.GameDataInfo;

namespace Hp2BaseMod;

[Expansion(typeof(QuestionDefinition), HasModId = true)]
public partial class ExpandedQuestionDefinition
{
    public static IdIndexMap DialogTriggerIndexes => _dialogTriggerIndexes;
    private static IdIndexMap _dialogTriggerIndexes = new();

    public IdIndexMap AnswerLookup => _answerLookup;
    private IdIndexMap _answerLookup = new();

    public string GetAnswer(QuestionDefinition def, RelativeId answerId) => def.questionAnswers[_answerLookup[answerId]];
}
