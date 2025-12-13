using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(TalkManager))]
public static class TalkManagerPatch
{
    [HarmonyPatch("TalkStep")]
    [HarmonyPrefix]
    public static bool TalkStep(TalkManager __instance)
        => ExpandedTalkManager.Get(__instance).TalkStep();
}

public class ExpandedTalkManager
{
    private static Dictionary<TalkManager, ExpandedTalkManager> _expansions
        = new Dictionary<TalkManager, ExpandedTalkManager>();

    public static ExpandedTalkManager Get(TalkManager core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedTalkManager(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static readonly FieldInfo f_talkType = AccessTools.Field(typeof(TalkManager), "_talkType");
    private static readonly FieldInfo f_talkStepIndex = AccessTools.Field(typeof(TalkManager), "_talkStepIndex");
    private static readonly FieldInfo f_questionsPool = AccessTools.Field(typeof(TalkManager), "_questionPool");
    private static readonly FieldInfo f_fileGirlPair = AccessTools.Field(typeof(TalkManager), "_fileGirlPair");
    private static readonly FieldInfo f_fileGirl = AccessTools.Field(typeof(TalkManager), "_fileGirl");
    private static readonly FieldInfo f_girlPair = AccessTools.Field(typeof(TalkManager), "_girlPair");
    private static readonly FieldInfo f_targetDoll = AccessTools.Field(typeof(TalkManager), "_targetDoll");
    private static readonly MethodInfo m_OnDialogOptionSelected = AccessTools.Method(typeof(TalkManager), "OnDialogOptionSelected");
    private static readonly MethodInfo m_OnDialogLineComplete = AccessTools.Method(typeof(TalkManager), "OnDialogLineComplete");
    private static readonly MethodInfo m_ReceiveFruitFromGirl = AccessTools.Method(typeof(TalkManager), "ReceiveFruitFromGirl");

    protected TalkManager _core;
    private ExpandedTalkManager(TalkManager core)
    {
        _core = core;
    }

    public bool TalkStep()
    {
        var talkType = f_talkType.GetValue<TalkWithType>(_core);
        var talkStepIndex = f_talkStepIndex.GetValue<int>(_core) + 1;

        switch (talkType)
        {
            case TalkWithType.HER_QUESTION:
                switch (talkStepIndex)
                {
                    // case 1:
                    //     HerQuestionAskOptions();
                    //     f_talkStepIndex.SetValue(_core, talkStepIndex);
                    //     return false;
                }
                break;
            case TalkWithType.FAVORITE_QUESTION:
                switch (talkStepIndex)
                {
                    case 1:
                        FavoriteQuestionShowOptions();
                        f_talkStepIndex.SetValue(_core, talkStepIndex);
                        return false;
                    case 2:
                        ModInterface.Log.Message($"Selected {Game.Session.Dialog.selectedDialogOptionIndex}");
                        FavoriteQuestionHandleSelection();
                        f_talkStepIndex.SetValue(_core, talkStepIndex);
                        return false;
                }
                break;
        }

        return true;
    }

    private void FavoriteQuestionHandleSelection()
    {
        var questionPool = f_questionsPool.GetValue<List<QuestionDefinition>>(_core);
        var girlPair = f_girlPair.GetValue<GirlPairDefinition>(_core);
        var fileGirl = f_fileGirl.GetValue<PlayerFileGirl>(_core);
        var targetDoll = f_targetDoll.GetValue<UiDoll>(_core);
        var girl = fileGirl.girlDefinition;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);

        var selectedQuestion = ModInterface.GameData.GetQuestion(Game.Session.Dialog.selectedDialogOptionIndex);
        questionPool.Clear();

        fileGirl.LearnFavAnswer(selectedQuestion);
        var pairFavQuestion = girlPair.GetMatchingFavQuestion(selectedQuestion);

        if (pairFavQuestion != null)
        {
            m_ReceiveFruitFromGirl.Invoke(_core, [targetDoll, false]);
        }
        else
        {
            Game.Manager.Audio.Play(AudioCategory.SOUND, Game.Manager.Ui.sfxReject, targetDoll.pauseDefinition);
        }

        if (!_core.dtFavQuestionResponse.Expansion().TryGetLineSet(_core.dtFavQuestionResponse, girlId, out var lineSet))
        {
            ModInterface.Log.Error($"Failed to find favorite question response DT set for girl {girlId}");
            m_OnDialogLineComplete.Invoke(_core, [targetDoll]);
            return;
        }

        var girlIndex = fileGirl.girlDefinition.Expansion().DialogTriggerIndex;
        var lineIndex = selectedQuestion.Expansion().DialogTriggerIndex;

        targetDoll.ReadDialogTrigger(_core.dtFavQuestionResponse, DialogLineFormat.ACTIVE, lineIndex);
        targetDoll.DialogLineCompleteEvent += OnDialogLineComplete;
    }

    private void FavoriteQuestionShowOptions()
    {
        var questionPool = f_questionsPool.GetValue<List<QuestionDefinition>>(_core);
        questionPool.Clear();
        questionPool.AddRange(Game.Data.Questions.GetAll());

        var girlPair = f_girlPair.GetValue<GirlPairDefinition>(_core);

        foreach (var question in girlPair.favQuestions)
        {
            questionPool.Remove(question.questionDefinition);
        }

        // remove questions from non-special chars that don't have answers
        void RemoveInvalidAnswers(GirlDefinition def)
        {
            if (def.specialCharacter) return;

            var favQuestionIdToAnswerId = def.Expansion().FavQuestionIdToAnswerId;

            questionPool.RemoveAll(x =>
            {
                var questionId = ModInterface.Data.GetDataId(GameDataType.Question, x.id);
                return !favQuestionIdToAnswerId.ContainsKey(questionId);
            });
        }

        RemoveInvalidAnswers(girlPair.girlDefinitionOne);
        RemoveInvalidAnswers(girlPair.girlDefinitionTwo);

        ListUtils.ShuffleList(questionPool);

        if (!girlPair.favQuestions.Any())
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
            var delta = (questionPool.Count - fileGirlPair.recentFavQuestions.Count) - ModInterface.State.FavQuestionOptionCount;

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
            var validCommons = girlPair.favQuestions.Select(x => x.questionDefinition.id).Except(fileGirlPair.recentFavQuestions).ToArray();
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
        Game.Session.Dialog.ShowDialogOptions(options, true, true);
        Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogOptionSelected;
    }

    private void HerQuestionAskOptions()
    {
        // List<int> indexList = ListUtils.GetIndexList<GirlQuestionSubDefinition>(this._targetDoll.girlDefinition.herQuestions);
        // if (this._targetDoll.girlDefinition == this.telepathGirlDefinition)
        // {
        //     indexList.Remove(this.telepathQuestionsGirlDefinitions.IndexOf(this._oppositeDoll.girlDefinition));
        // }
        // int num = this._fileGirl.recentHerQuestions.Count - 1;
        // while (num >= 0 && indexList.Count > 1)
        // {
        //     if (indexList.Contains(this._fileGirl.recentHerQuestions[num]))
        //     {
        //         indexList.Remove(this._fileGirl.recentHerQuestions[num]);
        //     }
        //     num--;
        // }
        // this._herQuestionIndex = indexList[Random.Range(0, indexList.Count)];
        // this._herQuestion = this._targetDoll.girlDefinition.herQuestions[this._herQuestionIndex];
        // this._fileGirl.AddRecentHerQuestion(this._herQuestionIndex);
        // this._targetDoll.ReadDialogTrigger(this.dtHerQuestion, DialogLineFormat.ACTIVE, this._herQuestionIndex);
        // this._targetDoll.isDialogBoxLocked = true;
        // this._targetDoll.DialogLineCompleteEvent += this.OnDialogLineComplete;
    }

    private void OnDialogOptionSelected()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogOptionSelected;
        m_OnDialogOptionSelected.Invoke(_core, []);
    }

    private void OnDialogLineComplete(UiDoll doll)
    {
        doll.DialogLineCompleteEvent -= OnDialogLineComplete;
        m_OnDialogLineComplete.Invoke(_core, [doll]);
    }
}
