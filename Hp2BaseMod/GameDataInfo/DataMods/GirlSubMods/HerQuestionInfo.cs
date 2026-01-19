using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseMod.GameDataInfo;

/// <summary>
/// Information to make a <see cref="GirlPartSubDefinition"/>.
/// </summary>
public class HerQuestionInfo : IHerQuestionDataInfo
{
    public DialogLineDataMod DialogLine;
    public HerQuestionAnswerInfo CorrectAnswer;
    public Dictionary<RelativeId, HerQuestionAnswerInfo> IncorrectAnswers;

    public void SetData(GirlQuestionSubDefinition def,
        DialogLine questionLine,
        IdIndexMap answerIdToIndex,
        Func<(DialogLine responseLine, int responseIndex)> getGoodResponse,
        Func<RelativeId, (DialogLine responseLine, int responseIndex)> getBadResponse,
        GameDefinitionProvider gameDefinitionProvider,
        AssetProvider assetProvider)
    {
        if (DialogLine != null)
        {
            DialogLine.SetData(questionLine, gameDefinitionProvider, assetProvider);
        }

        // The correct answer is hard coded as the first answer
        if (CorrectAnswer != null)
        {
            var correctAnswer = def.answers.FirstOrDefault();
            if (correctAnswer == null)
            {
                correctAnswer = new() { responseIndex = -1 };
                def.answers.Add(correctAnswer);
            }

            CorrectAnswer.SetData(correctAnswer, getGoodResponse, gameDefinitionProvider, assetProvider);
        }

        if (IncorrectAnswers != null)
        {
            foreach (var id_answerMod in IncorrectAnswers)
            {
                var answerDef = def.answers.GetOrNew(answerIdToIndex[id_answerMod.Key], () => new() { responseIndex = -1 });

                id_answerMod.Value.SetData(answerDef, () => getBadResponse(id_answerMod.Key), gameDefinitionProvider, assetProvider);
            }
        }
    }
}
