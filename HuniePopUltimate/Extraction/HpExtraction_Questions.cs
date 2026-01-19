using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AssetStudio;
using AssetStudio.Extractor;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;

namespace HuniePopUltimate;

public partial class HpExtraction
{
    private static readonly Dictionary<RelativeId, List<RelativeId>> FavoriteQuestionOrder = new()
    {
        {Girls.Aiko,
            new()
            {
                Questions.LastName,
                Questions.Height,
                Questions.Birthday,
                Questions.FavColour,
                Questions.Age,
                Questions.Weight,
                Questions.FavSeason,
                Questions.Education,
                Questions.Occupation,
                Questions.CupSize,
                Questions.Hobby,
                Questions.FavHangout,
            }
        },
        {Girls.Audrey,
            new()
            {
                Questions.LastName,
                Questions.Education,
                Questions.Weight,
                Questions.CupSize,
                Questions.FavColour,
                Questions.FavHangout,
                Questions.Age,
                Questions.Height,
                Questions.Occupation,
                Questions.Birthday,
                Questions.Hobby,
                Questions.FavSeason,
            }
        },
        {Girls.Beli,
            new()
            {
                Questions.FavSeason,
                Questions.Weight,
                Questions.Age,
                Questions.CupSize,
                Questions.FavHangout,
                Questions.FavColour,
                Questions.Height,
                Questions.Hobby,
                Questions.Occupation,
                Questions.Education,
                Questions.LastName,
                Questions.Birthday,
            }
        },
        {Girls.Celeste,
            new()
            {
                Questions.Hobby,
                Questions.FavHangout,
                Questions.Birthday,
                Questions.FavSeason,
                Questions.CupSize,
                Questions.Height,
                Questions.Occupation,
                Questions.Weight,
                Questions.FavColour,
                Questions.HomeWorld,
                Questions.LastName,
                Questions.Birthday,
            }
        },
        {Hp2BaseMod.Girls.Jessie,
            new()
            {
                Questions.Weight,
                Questions.FavColour,
                Questions.Education,
                Questions.Birthday,
                Questions.Occupation,
                Questions.CupSize,
                Questions.FavHangout,
                Questions.Age,
                Questions.Height,
                Questions.FavSeason,
                Questions.Hobby,
                Questions.LastName,
            }
        },
        {Girls.Kyanna,
            new()
            {
                Questions.LastName,
                Questions.Age,
                Questions.Education,
                Questions.Weight,
                Questions.CupSize,
                Questions.Occupation,
                Questions.Hobby,
                Questions.FavColour,
                Questions.FavSeason,
                Questions.FavHangout,
                Questions.Height,
                Questions.Birthday,
            }
        },
        {Hp2BaseMod.Girls.Kyu,
            new()
            {
                Questions.FavHangout,
                Questions.Height,
                Questions.FavColour,
                Questions.LastName,
                Questions.HomeWorld,
                Questions.FavSeason,
                Questions.Weight,
                Questions.Birthday,
                Questions.Occupation,
                Questions.Age,
                Questions.CupSize,
                Questions.Hobby,
            }
        },
        {Hp2BaseMod.Girls.Lola,
            new()
            {
                Questions.LastName,
                Questions.Birthday,
                Questions.Age,
                Questions.Education,
                Questions.FavColour,
                Questions.Hobby,
                Questions.FavHangout,
                Questions.CupSize,
                Questions.FavSeason,
                Questions.Weight,
                Questions.Height,
                Questions.Occupation,
            }
        },
        {Girls.Momo,
            new()
            {
                Questions.HomeWorld,
                Questions.CupSize,
                Questions.FavColour,
                Questions.Occupation,
                Questions.Hobby,
                Questions.Weight,
                Questions.Height,
                Questions.Birthday,
                Questions.FavHangout,
                Questions.LastName,
                Questions.FavSeason,
                Questions.Age,
            }
        },
        {Girls.Nikki,
            new()
            {
                Questions.Age,
                Questions.FavSeason,
                Questions.Birthday,
                Questions.Height,
                Questions.Education,
                Questions.Weight,
                Questions.Hobby,
                Questions.FavHangout,
                Questions.Occupation,
                Questions.FavColour,
                Questions.CupSize,
                Questions.LastName,
            }
        },
        {Girls.Tiffany,
            new()
            {
                Questions.LastName,
                Questions.Education,
                Questions.Birthday,
                Questions.FavHangout,
                Questions.Education,
                Questions.Weight,
                Questions.CupSize,
                Questions.FavSeason,
                Questions.Age,
                Questions.Height,
                Questions.Hobby,
                Questions.FavColour,
            }
        },
        {Girls.Venus,
            new()
            {
                Questions.LastName,
                Questions.Height,
                Questions.FavSeason,
                Questions.Occupation,
                Questions.FavHangout,
                Questions.Weight,
                Questions.HomeWorld,
                Questions.Hobby,
                Questions.CupSize,
                Questions.Age,
                Questions.Birthday,
                Questions.FavColour,
            }
        },
    };

    private void ExtractQuestions(OrderedDictionary girlDef, SerializedFile file, Dictionary<RelativeId, IHerQuestionDataInfo> questions)
    {
        var questionCount = 0;
        if (girlDef.TryGetValue("talkQuestions", out List<object> talkQuestions))
        {
            foreach (var talkQ in talkQuestions.OfType<OrderedDictionary>())
            {
                var questionId = new RelativeId(Plugin.ModId, questionCount++);
                var question = new HerQuestionInfo();
                question.IncorrectAnswers = new();
                questions[questionId] = question;

                if (UnityAssetPath.TryExtract(talkQ, out var talkQPath)
                    && _extractor.TryExtractMonoBehavior(file, talkQPath, out var talkQuery)
                    && talkQuery.TryGetValue("steps", out List<object> steps))
                {
                    OrderedDictionary dialogStep = null;
                    OrderedDictionary answersStep = null;

                    foreach (var step in steps.OfType<OrderedDictionary>())
                    {
                        if (step.TryGetValue("type", out int type))
                        {
                            if (type == 0)// dialog line
                            {
                                dialogStep = step;
                            }
                            else if (type == 1)// response options
                            {
                                answersStep = step;
                            }
                        }
                    }

                    if (dialogStep == null || answersStep == null)
                    {
                        ModInterface.Log.Warning("Failed to parse questions steps");
                    }

                    if (dialogStep != null
                        && dialogStep.TryGetValue("sceneLine", out OrderedDictionary sceneLine)
                        && sceneLine.TryGetValue("dialogLine", out OrderedDictionary dialogLine)
                        && TryExtractDialogLine(dialogLine, file, new RelativeId(Plugin.ModId, _dialogLineCount++), out var dialogLineMod))
                    {
                        question.DialogLine = dialogLineMod;
                    }
                    else
                    {
                        ModInterface.Log.Error("Failed to find dialog step");
                        continue;
                    }

                    if (answersStep != null
                            && answersStep.TryGetValue("responseOptions", out List<object> responseOptions))
                    {
                        var answerCount = 0;

                        foreach (var option in responseOptions.OfType<OrderedDictionary>())
                        {
                            if (TryExtractAnswer(option, file, out var answerMod))
                            {
                                if (answerCount++ == 0)
                                {
                                    question.CorrectAnswer = answerMod;
                                }
                                else
                                {
                                    question.IncorrectAnswers[new RelativeId(Plugin.ModId, answerCount)] = answerMod;
                                }
                            }
                        }
                    }
                    else
                    {
                        ModInterface.Log.Error($"Failed to find answer step");
                    }
                }
            }
        }
    }

    private bool TryExtractAnswer(OrderedDictionary answerDef, SerializedFile file, out HerQuestionAnswerInfo answer)
    {
        if (answerDef.TryGetValue("text", out string text)
            && answerDef.TryGetValue("secondary", out bool secondary)
            && answerDef.TryGetValue("secondaryText", out string secondaryText)
            && answerDef.TryGetValue("steps", out List<object> optionSteps)
            && optionSteps.Any()
            && optionSteps[0] is OrderedDictionary step
            && step.TryGetValue("type", out int type))
        {
            answer = new()
            {
                text = text
            };

            if (secondary) answer.altText = secondaryText;

            if (type == 11
                )//&& step.TryGetValue("dialogTrigger", out OrderedDictionary dialogTrigger)
                 //&& UnityAssetPath.TryExtract(dialogTrigger, out var atAssetPath))
            {
                return true;//for now just assuming we added the dts already and ignore
            }
            else if (type == 0
                && step.TryGetValue("sceneLine", out OrderedDictionary answerSceneLine)
                && answerSceneLine.TryGetValue("dialogLine", out OrderedDictionary answerDialogLine)
                && TryExtractDialogLine(answerDialogLine, file, new RelativeId(Plugin.ModId, _dialogLineCount++), out answer.Response))
            {
                return true;
            }
            else
            {
                ModInterface.Log.Error($"Unhandled step type {type}");
                return false;
            }
        }

        answer = null;
        return false;
    }

    private void ExtractQueries(RelativeId girlId, OrderedDictionary girlDef, SerializedFile file, Dictionary<RelativeId, IDialogLineDataMod> favoriteDialogLines)
    {
        if (girlDef.TryGetValue("talkQueries", out List<object> talkQueries))
        {
            if (!FavoriteQuestionOrder.TryGetValue(girlId, out var questionOrder))
            {
                ModInterface.Log.Warning($"Failed to find query order for girl {girlId}");
                return;
            }

            var questionOrderEnum = questionOrder.GetEnumerator();
            foreach (var talkQ in talkQueries.OfType<OrderedDictionary>())
            {
                if (UnityAssetPath.TryExtract(talkQ, out var talkQPath)
                    && _extractor.TryExtractMonoBehavior(file, talkQPath, out var talkQuery)
                    && talkQuery.TryGetValue<List<object>>("steps", out var steps))
                {
                    foreach (var step in steps.OfType<OrderedDictionary>().Skip(1))//skip the initial dt, move to the options
                    {
                        foreach (var line in ExtractQueryLines(step, file))
                        {
                            if (!questionOrderEnum.MoveNext()) break;
                            favoriteDialogLines[questionOrderEnum.Current] = line;
                        }
                    }
                }
            }
        }
    }

    private IEnumerable<DialogLineDataMod> ExtractQueryLines(OrderedDictionary dialogSceneStep, SerializedFile file)
    {
        if (dialogSceneStep.TryGetValue("responseOptions", out List<object> responseOptions))
        {
            foreach (var branch in responseOptions.OfType<OrderedDictionary>())
            {
                if (branch.TryGetValue("steps", out List<object> steps))
                {
                    foreach (var step in steps.OfType<OrderedDictionary>())
                    {
                        if (step.TryGetValue("conditionalBranchs", out List<object> conditionalBranches))
                        {
                            foreach (var conditionalBranch in conditionalBranches.OfType<OrderedDictionary>())
                            {
                                if (conditionalBranch.TryGetValue("steps", out List<object> conditionalBranchSteps))
                                {
                                    foreach (var conditionalBranchStep in conditionalBranchSteps.OfType<OrderedDictionary>())
                                    {
                                        if (conditionalBranchStep.TryGetValue("type", out int type)
                                            && type == 0
                                            && conditionalBranchStep.TryGetValue("sceneLine", out OrderedDictionary sceneLine)
                                            && sceneLine.TryGetValue("dialogLine", out OrderedDictionary dialogLine)
                                            && TryExtractDialogLine(dialogLine, file, new RelativeId(Plugin.ModId, _dialogLineCount++), out var dialogLineMod))
                                        {
                                            yield return dialogLineMod;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}