using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using UnityEngine;

namespace SingleDate;

[HarmonyPatch(typeof(TalkManager))]
public static class TalkManagerPatch
{
    [HarmonyPatch("TalkStep")]
    [HarmonyPrefix]
    public static bool TalkStep(TalkManager __instance) => ExpandedTalkManager.Get(__instance).TalkStep();
}

public class ExpandedTalkManager
{
    private static Dictionary<TalkManager, ExpandedTalkManager> _expansions
        = new Dictionary<TalkManager, ExpandedTalkManager>();

    public static ExpandedTalkManager Get(TalkManager talkManager)
    {
        if (!_expansions.TryGetValue(talkManager, out var expansion))
        {
            expansion = new ExpandedTalkManager(talkManager);
            _expansions[talkManager] = expansion;
        }

        return expansion;
    }

    private static FieldInfo _talkStepIndex = AccessTools.Field(typeof(TalkManager), "_talkStepIndex");
    private static FieldInfo _girlPair = AccessTools.Field(typeof(TalkManager), "_girlPair");
    private static FieldInfo _fileGirlPair = AccessTools.Field(typeof(TalkManager), "_fileGirlPair");
    private static FieldInfo _questionPool = AccessTools.Field(typeof(TalkManager), "_questionPool");
    private static MethodInfo _talkStep = AccessTools.Method(typeof(TalkManager), "TalkStep");

    private TalkManager _talkManager;

    public ExpandedTalkManager(TalkManager talkManager)
    {
        _talkManager = talkManager;
    }

    public bool TalkStep()
    {
        var talkStepIndex = (int)_talkStepIndex.GetValue(_talkManager);

        if (_talkManager.talkType != TalkWithType.FAVORITE_QUESTION
            || !(talkStepIndex == 0 || talkStepIndex == 2)
            || !State.IsSingle(_girlPair.GetValue(_talkManager) as GirlPairDefinition))
        {
            return true;
        }

        //replace step 1 and 3 for single date pairs
        talkStepIndex++;
        _talkStepIndex.SetValue(_talkManager, talkStepIndex);

        var filePair = (PlayerFileGirlPair)_fileGirlPair.GetValue(_talkManager);

        switch (talkStepIndex)
        {
            case 1:
                //3 random questions
                var questionPool = (List<QuestionDefinition>)_questionPool.GetValue(_talkManager);
                questionPool.Clear();

                var pair = (GirlPairDefinition)_girlPair.GetValue(_talkManager);
                var favQuestionIndexes = ListUtils.GetIndexList(pair.favQuestions);
                favQuestionIndexes.RemoveAll(filePair.recentFavQuestions.Contains);
                ListUtils.ShuffleList(favQuestionIndexes);
                favQuestionIndexes.RemoveRange(0, favQuestionIndexes.Count - 3);

                foreach (var index in favQuestionIndexes)
                {
                    filePair.AddRecentFavQuestion(index);
                    questionPool.Add(pair.favQuestions[index].questionDefinition);
                }

                Game.Session.Dialog.ShowDialogOptions(
                    questionPool.Select(x => new DialogOptionInfo(x.questionText, _talkManager.favQuestionDefinitions.IndexOf(x))).ToList(),
                    true,
                    true);

                Game.Session.Dialog.DialogOptionSelectedEvent += OnDialogOptionSelected;
                break;
            case 3:
                //ignore response from other girl, cuz there is no other girl
                filePair.LearnFavAnswer(_talkManager.favQuestionDefinitions[Game.Session.Dialog.selectedDialogOptionIndex]);
                _talkStep.Invoke(_talkManager, null);
                break;
        }

        return false;
    }

    private void OnDialogOptionSelected()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogOptionSelected;
        _talkStep.Invoke(_talkManager, null);
    }
}