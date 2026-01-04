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
    private static readonly FieldInfo f_oppositeFileGirl = AccessTools.Field(typeof(TalkManager), "_oppositeFileGirl");
    private static readonly FieldInfo f_girlPair = AccessTools.Field(typeof(TalkManager), "_girlPair");
    private static readonly FieldInfo f_targetDoll = AccessTools.Field(typeof(TalkManager), "_targetDoll");
    private static readonly FieldInfo f_oppositeDoll = AccessTools.Field(typeof(TalkManager), "_oppositeDoll");

    private static readonly MethodInfo m_OnDialogOptionSelected = AccessTools.Method(typeof(TalkManager), "OnDialogOptionSelected");
    private static readonly MethodInfo m_OnDialogLineComplete = AccessTools.Method(typeof(TalkManager), "OnDialogLineComplete");
    private static readonly MethodInfo m_ReceiveFruitFromGirl = AccessTools.Method(typeof(TalkManager), "ReceiveFruitFromGirl");
    private static readonly MethodInfo m_TalkStep = AccessTools.Method(typeof(TalkManager), "TalkStep");

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
                        f_talkStepIndex.SetValue(_core, talkStepIndex);
                        FavoriteQuestionShowOptions();
                        return false;
                    case 2:
                        f_talkStepIndex.SetValue(_core, talkStepIndex);
                        FavoriteQuestionHandleSelection();
                        return false;
                    case 3:
                        f_talkStepIndex.SetValue(_core, talkStepIndex);
                        FavoriteQuestionResponse();//needs args
                        return false;
                }
                break;
        }

        return true;
    }

    private void FavoriteQuestionResponse()
    {
        var fileGirl = f_fileGirl.GetValue<PlayerFileGirl>(_core);
        var girl = fileGirl.girlDefinition;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);
        var girlPair = f_girlPair.GetValue<GirlPairDefinition>(_core);

        var oppositeDoll = f_oppositeDoll.GetValue<UiDoll>(_core);
        var oppositeDef = oppositeDoll.girlDefinition.Expansion().FavQuestionIdToAnswerId;
        var oppositeFileGirl = f_oppositeFileGirl.GetValue<PlayerFileGirl>(_core);

        var selectedQuestionId = ModInterface.Data.GetDataId(GameDataType.Question, Game.Session.Dialog.selectedDialogOptionIndex);
        var selectedQuestion = ModInterface.GameData.GetQuestion(selectedQuestionId);
        var answer = ExpandedGirlDefinition.Get(girlId).FavQuestionIdToAnswerId[selectedQuestionId];

        var args = new TalkFavQuestionResponseArgs()
        {
            OtherGirlResponds = oppositeDef.TryGetValue(selectedQuestionId, out var oppositeAnswer)
            && oppositeAnswer == answer
        };

        ModInterface.Events.NotifyFavQuestionResponse(args);

        if (args.OtherGirlResponds)
        {
            oppositeFileGirl.LearnFavAnswer(selectedQuestion);
            m_ReceiveFruitFromGirl.Invoke(_core, [oppositeDoll, false]);
            oppositeDoll.DialogLineCompleteEvent += OnDialogLineComplete;
            oppositeDoll.ReadDialogTrigger(_core.dtFavQuestionAgreement, DialogLineFormat.ACTIVE, selectedQuestion.Expansion().AnswerLookup[oppositeAnswer]);
        }
        else
        {
            m_TalkStep.Invoke(_core, []);
        }
    }

    private void FavoriteQuestionHandleSelection()
    {
        var questionPool = f_questionsPool.GetValue<List<QuestionDefinition>>(_core);
        var girlPair = f_girlPair.GetValue<GirlPairDefinition>(_core);
        var fileGirl = f_fileGirl.GetValue<PlayerFileGirl>(_core);
        var targetDoll = f_targetDoll.GetValue<UiDoll>(_core);
        var oppositeGirl = f_oppositeDoll.GetValue<UiDoll>(_core).girlDefinition;
        var girl = fileGirl.girlDefinition;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);

        var selectedQuestionId = ModInterface.Data.GetDataId(GameDataType.Question, Game.Session.Dialog.selectedDialogOptionIndex);
        var selectedQuestion = ModInterface.GameData.GetQuestion(selectedQuestionId);
        var answer = ExpandedGirlDefinition.Get(girlId).FavQuestionIdToAnswerId[selectedQuestionId];

        questionPool.Clear();

        fileGirl.LearnFavAnswer(selectedQuestion);

        if (oppositeGirl.Expansion().FavQuestionIdToAnswerId.TryGetValue(selectedQuestionId, out var oppositeAnswer)
            && oppositeAnswer == answer)
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

        var girlIndex = ExpandedGirlDefinition.DialogTriggerIndexes[ModInterface.Data.GetDataId(GameDataType.Girl, fileGirl.girlDefinition.id)];
        var lineIndex = ExpandedQuestionDefinition.DialogTriggerIndexes[ModInterface.Data.GetDataId(GameDataType.Question, selectedQuestion.id)];

        targetDoll.DialogLineCompleteEvent += OnDialogLineComplete;
        targetDoll.ReadDialogTrigger(_core.dtFavQuestionResponse, DialogLineFormat.ACTIVE, lineIndex);
    }

    private void FavoriteQuestionShowOptions()
    {
        var fileGirl = f_fileGirl.GetValue<PlayerFileGirl>(_core);
        var girl = fileGirl.girlDefinition;
        var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girl.id);
        ModInterface.Log.Message($"Target girl: {girl.girlName}");

        var questionPool = f_questionsPool.GetValue<List<QuestionDefinition>>(_core);
        var targetGirlQuestions = girl.Expansion().FavQuestionIdToAnswerId.Keys.Select(ModInterface.GameData.GetQuestion);
        questionPool.Clear();
        questionPool.AddRange(targetGirlQuestions);

        var girlPair = f_girlPair.GetValue<GirlPairDefinition>(_core);
        var girlOneFavs = girlPair.girlDefinitionOne.Expansion().FavQuestionIdToAnswerId;
        var girlTwoFavs = girlPair.girlDefinitionTwo.Expansion().FavQuestionIdToAnswerId;

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

            var favQuestionIdToAnswerId = def.Expansion().FavQuestionIdToAnswerId;

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

        Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogOptionSelected;
        ModInterface.Log.Message("final option count: " + options.Count);

        if (options.Any())
        {
            Game.Session.Dialog.ShowDialogOptions(options, true, true);
        }
        else
        {
            ModInterface.Log.Warning("No question options. Skipping to step 4");
            f_talkStepIndex.SetValue(_core, 3);
            m_TalkStep.Invoke(_core, []);
        }
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
