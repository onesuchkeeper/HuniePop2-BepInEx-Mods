using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using UnityEngine;

namespace Hp2BaseModTweaks;

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

    private static readonly int FAV_QUESTION_DISPLAY_COUNT = 3;

    private static readonly FieldInfo f_talkType = AccessTools.Field(typeof(TalkManager), "_talkType");
    private static readonly FieldInfo f_talkStepIndex = AccessTools.Field(typeof(TalkManager), "_talkStepIndex");
    private static readonly FieldInfo f_questionsPool = AccessTools.Field(typeof(TalkManager), "_questionPool");
    private static readonly FieldInfo f_fileGirlPair = AccessTools.Field(typeof(TalkManager), "_fileGirlPair");
    private static readonly FieldInfo f_girlPair = AccessTools.Field(typeof(TalkManager), "_girlPair");
    private static readonly MethodInfo m_OnDialogOptionSelected = AccessTools.Method(typeof(TalkManager), "OnDialogOptionSelected");

    protected TalkManager _core;
    private ExpandedTalkManager(TalkManager core)
    {
        _core = core;
    }

    public bool TalkStep()
    {
        var talkType = f_talkType.GetValue<TalkWithType>(_core);
        var talkStepIndex = f_talkStepIndex.GetValue<int>(_core) + 1;

        ModInterface.Log.LogInfo($"Talk Type {talkType}, Step {talkType} {talkStepIndex}");

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
                }
                break;
        }

        return true;
    }

    private void FavoriteQuestionShowOptions()
    {
        //replace fav question talk step 1 to protect against bad question indexes
        var questionPool = f_questionsPool.GetValue<List<QuestionDefinition>>(_core);
        questionPool.Clear();
        questionPool.AddRange(_core.favQuestionDefinitions);

        var girlPair = f_girlPair.GetValue<GirlPairDefinition>(_core);

        for (int j = 0; j < girlPair.favQuestions.Count; j++)
        {
            questionPool.Remove(girlPair.favQuestions[j].questionDefinition);
        }

        //remove questions from non-special chars that don't have answers
        void RemoveInvalidAnswers(GirlDefinition def)
        {
            if (!(def?.specialCharacter ?? true))
            {
                int i = 0;
                foreach (var answerIndex in def.favAnswers)
                {
                    if (answerIndex < 0)
                    {
                        questionPool.RemoveAll(x => x.id == i);
                    }

                    i++;
                }
            }
        }

        RemoveInvalidAnswers(girlPair.girlDefinitionOne);
        RemoveInvalidAnswers(girlPair.girlDefinitionTwo);

        if (questionPool.Count < 2)
        {
            var supplementPool = _core.favQuestionDefinitions.Except(questionPool).ToList();

            while (questionPool.Count < 2)
            {
                questionPool.Add(supplementPool.PopRandom());
            }
        }

        ListUtils.ShuffleList(questionPool);

        //remove all but 2
        questionPool.RemoveRange(0, questionPool.Count - 2);
        var pairFavQuestionIndexes = ListUtils.GetIndexList(girlPair.favQuestions);

        var fileGirlPair = f_fileGirlPair.GetValue<PlayerFileGirlPair>(_core);
        int maxRecentFavQuestionIndex = fileGirlPair.recentFavQuestions.Count - 1;

        while (maxRecentFavQuestionIndex >= 0 && pairFavQuestionIndexes.Count > 1)
        {
            if (pairFavQuestionIndexes.Contains(fileGirlPair.recentFavQuestions[maxRecentFavQuestionIndex]))
            {
                pairFavQuestionIndexes.Remove(fileGirlPair.recentFavQuestions[maxRecentFavQuestionIndex]);
            }
            maxRecentFavQuestionIndex--;
        }

        int num3 = pairFavQuestionIndexes[Random.Range(0, pairFavQuestionIndexes.Count)];
        fileGirlPair.AddRecentFavQuestion(num3);
        questionPool.Add(girlPair.favQuestions[num3].questionDefinition);
        var options = new List<DialogOptionInfo>();

        for (int k = 0; k < questionPool.Count; k++)
        {
            options.Add(new DialogOptionInfo(questionPool[k].questionText, _core.favQuestionDefinitions.IndexOf(questionPool[k])));
        }

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
}
