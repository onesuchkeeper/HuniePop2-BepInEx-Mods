using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(TalkManager))]
public static class TalkManagerPatch
{
    [HarmonyPatch("TalkStep")]
    [HarmonyPrefix]
    public static bool TalkStep(TalkManager __instance)
        => ExpandedTalkManager.Get(__instance).TalkStep_Prefix();
}

[Expansion(typeof(TalkManager), 
    Fields = new[]{ "_talkType", "_talkStepIndex", "_questionPool", "_fileGirlPair", "_fileGirl", 
        "_oppositeFileGirl", "_girlPair", "_targetDoll", "_oppositeDoll"},
    Methods = new[]{"OnDialogOptionSelected", "OnDialogLineComplete", "ReceiveFruitFromGirl", "TalkStep"})]
public partial class ExpandedTalkManager
{
    public bool TalkStep_Prefix()
    {
        var nextIndex = _talkStepIndex + 1;
        switch (_talkType)
        {
            case TalkWithType.HER_QUESTION:
                ModInterface.Log.Message($"HER_QUESTION {nextIndex}");
                switch (nextIndex)
                {
                    // case 1:
                    //     HerQuestionAskOptions();
                    //     _talkStepIndex++;
                    //     return false;
                }
                break;
            case TalkWithType.FAVORITE_QUESTION:
                ModInterface.Log.Message($"FAVORITE_QUESTION {nextIndex}");
                switch (nextIndex)
                {
                    case 1:
                        _talkStepIndex++;
                        FavoriteQuestionShowOptions();
                        return false;
                    case 2:
                        _talkStepIndex++;
                        FavoriteQuestionHandleSelection();
                        return false;
                    case 3:
                        _talkStepIndex++;
                        FavoriteQuestionResponse();
                        return false;
                }
                break;
        }

        return true;
    }

    private void FavoriteQuestionResponse()
    {
        var girl = _fileGirl.girlDefinition;

        var oppositeDoll = _oppositeDoll;
        var oppositeDef = oppositeDoll.girlDefinition.GetExpansion().FavQuestionIdToAnswerId;

        var selectedQuestionId = ModInterface.Data.GetDataId(GameDataType.Question, Game.Session.Dialog.selectedDialogOptionIndex);
        var selectedQuestion = ModInterface.GameData.GetQuestion(selectedQuestionId);

        var args = new TalkFavQuestionResponseArgs()
        {
            OtherGirlResponds = oppositeDef.TryGetValue(selectedQuestionId, out var oppositeAnswer)
                && oppositeAnswer == girl.GetExpansion().FavQuestionIdToAnswerId[selectedQuestionId]
        };

        ModInterface.Events.NotifyFavQuestionResponse(args);

        if (args.OtherGirlResponds)
        {
            _oppositeFileGirl.LearnFavAnswer(selectedQuestion);
            m_ReceiveFruitFromGirl.Invoke(_core, [oppositeDoll, false]);
            oppositeDoll.DialogLineCompleteEvent += OnDialogLineComplete_Hook;
            oppositeDoll.ReadDialogTrigger(_core.dtFavQuestionAgreement, DialogLineFormat.ACTIVE, selectedQuestion.GetExpansion().AnswerLookup[oppositeAnswer]);
        }
        else
        {
            TalkStep();
        }
    }

    private void FavoriteQuestionHandleSelection()
    {
        var fileGirl = _fileGirl;
        var targetDoll = _targetDoll;
        var girl = fileGirl.girlDefinition;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);

        var selectedQuestionId = ModInterface.Data.GetDataId(GameDataType.Question, Game.Session.Dialog.selectedDialogOptionIndex);
        var selectedQuestion = ModInterface.GameData.GetQuestion(selectedQuestionId);

        _questionPool.Clear();

        fileGirl.LearnFavAnswer(selectedQuestion);

        if (_oppositeDoll.girlDefinition.GetExpansion().FavQuestionIdToAnswerId.TryGetValue(selectedQuestionId, out var oppositeAnswer)
            && oppositeAnswer == girl.GetExpansion().FavQuestionIdToAnswerId[selectedQuestionId])
        {
            m_ReceiveFruitFromGirl.Invoke(_core, [targetDoll, false]);
        }
        else
        {
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxReject, targetDoll.pauseDefinition);
        }

        if (!_core.dtFavQuestionResponse.GetExpansion().TryGetLineSet(_core.dtFavQuestionResponse, girlId, out var lineSet))
        {
            ModInterface.Log.Error($"Failed to find favorite question response DT set for girl {girlId}");
            m_OnDialogLineComplete.Invoke(_core, [targetDoll]);
            return;
        }

        var girlIndex = ExpandedGirlDefinition.DialogTriggerIndexes[ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id)];
        var lineIndex = ExpandedQuestionDefinition.DialogTriggerIndexes[ModInterface.Data.GetDataId(GameDataType.Question, selectedQuestion.id)];

        targetDoll.DialogLineCompleteEvent += OnDialogLineComplete_Hook;
        targetDoll.ReadDialogTrigger(_core.dtFavQuestionResponse, DialogLineFormat.ACTIVE, lineIndex);
    }

    private void FavoriteQuestionShowOptions()
    {
        ModInterface.Log.Message();
        var fileGirl = f_fileGirl.GetValue<PlayerFileGirl>(_core);
        var girl = fileGirl.girlDefinition;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);
        ModInterface.Log.Message($"Target girl: {girl.girlName}");

        var questionPool = f_questionPool.GetValue<List<QuestionDefinition>>(_core);
        var targetGirlQuestions = girl.GetExpansion().FavQuestionIdToAnswerId.Keys.Select(ModInterface.GameData.GetQuestion);
        questionPool.Clear();
        questionPool.AddRange(targetGirlQuestions);

        var girlPair = f_girlPair.GetValue<GirlPairDefinition>(_core);
        var girlOneFavs = girlPair.girlDefinitionOne.GetExpansion().FavQuestionIdToAnswerId;
        var girlTwoFavs = girlPair.girlDefinitionTwo.GetExpansion().FavQuestionIdToAnswerId;

        var commonQuestions = new HashSet<int>();

        foreach (var entry in girlOneFavs)
        {
            if (girlTwoFavs.TryGetValue(entry.Key, out var value) && value == entry.Value)
            {
                commonQuestions.Add(ModInterface.Data.GetRuntimeDataId(GameDataType.Question, entry.Key));
            }
        }
        ModInterface.Log.Message("common questions: " + string.Join(", ", commonQuestions));
        questionPool.RemoveAll(x => commonQuestions.Contains(x.id));

        // remove questions from non-special chars that don't have answers
        void RemoveInvalidAnswers(GirlDefinition def)
        {
            if (def.specialCharacter) return;

            var favQuestionIdToAnswerId = def.GetExpansion().FavQuestionIdToAnswerId;

            questionPool.RemoveAll(x =>
            {
                var questionId = ModInterface.Data.GetDataId(GameDataType.Question, x.id);
                return !favQuestionIdToAnswerId.ContainsKey(questionId);
            });
        }

        RemoveInvalidAnswers(girlPair.girlDefinitionOne);
        RemoveInvalidAnswers(girlPair.girlDefinitionTwo);

        // if there aren't enough valid questions ask a 
        if (questionPool.Count < ModInterface.State.FavQuestionOptionCount)
        {
            questionPool.Clear();
            questionPool.AddRange(targetGirlQuestions);
            ModInterface.Log.Message($"Target girl questions: {string.Join(", ", targetGirlQuestions.Select(x => x.questionName))}");
        }

        ListUtils.ShuffleList(questionPool);
        ModInterface.Log.Message("final question pool: " + string.Join(", ", questionPool.Select(x => x.id)));

        if (!commonQuestions.Any())
        {
            // if the pair doesn't have common questions, just take random ones
            if (questionPool.Count > ModInterface.State.FavQuestionOptionCount)
            {
                questionPool.RemoveRange(0, questionPool.Count - ModInterface.State.FavQuestionOptionCount);
            }
        }
        else if (questionPool.Count > ModInterface.State.FavQuestionOptionCount)
        {
            // prune recent questions to ensure choice count
            var fileGirlPair = f_fileGirlPair.GetValue<PlayerFileGirlPair>(_core);
            var delta = questionPool.Count - fileGirlPair.recentFavQuestions.Count - ModInterface.State.FavQuestionOptionCount;

            if (delta < 0)
            {
                fileGirlPair.recentFavQuestions.RemoveRange(0, fileGirlPair.recentFavQuestions.Count + delta);
            }

            // remove recent questions
            if (fileGirlPair.recentFavQuestions.Any())
            {
                questionPool.RemoveAll(x => fileGirlPair.recentFavQuestions.Contains(x.id));
            }

            // see if we can add a guaranteed common answer
            var validCommons = commonQuestions.Except(fileGirlPair.recentFavQuestions).ToArray();
            if (validCommons.Any())
            {
                questionPool.RemoveRange(0, questionPool.Count - (ModInterface.State.FavQuestionOptionCount - 1));
                var randomQuestion = ModInterface.GameData.GetQuestion(validCommons.GetRandom());
                questionPool.Add(ModInterface.GameData.GetQuestion(validCommons.GetRandom()));
                fileGirlPair.AddRecentFavQuestion(randomQuestion.id);
            }
            else
            {
                questionPool.RemoveRange(0, questionPool.Count - ModInterface.State.FavQuestionOptionCount);
            }
        }

        var options = questionPool.Select(x => new DialogOptionInfo(x.questionText, x.id)).ToList();

        Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogOptionSelected_Hook;
        ModInterface.Log.Message("final option count: " + options.Count);

        if (options.Any())
        {
            Game.Session.Dialog.ShowDialogOptions(options, true, true);
        }
        else
        {
            ModInterface.Log.Warning("No question options. Skipping to step 4");
            _talkStepIndex = 3;
            TalkStep();
        }
    }

    private void OnDialogOptionSelected_Hook()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogOptionSelected_Hook;
        m_OnDialogOptionSelected.Invoke(_core);
    }

    private void OnDialogLineComplete_Hook(UiDoll doll)
    {
        ModInterface.Log.Message();
        doll.DialogLineCompleteEvent -= OnDialogLineComplete_Hook;
        m_OnDialogLineComplete.Invoke(_core, [doll]);
    }
}
