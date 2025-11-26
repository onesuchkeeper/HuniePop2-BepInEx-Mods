using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod.Extension;

namespace SingleDate;

[HarmonyPatch(typeof(TalkManager))]
internal static class TalkManagerPatch
{
    [HarmonyPatch("TalkStep")]
    [HarmonyPrefix]
    public static bool TalkStep(TalkManager __instance) => ExpandedTalkManager.Get(__instance).TalkStep();
}

internal class ExpandedTalkManager
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
        var talkStepIndex = _talkStepIndex.GetValue<int>(_talkManager);

        if (_talkManager.talkType != TalkWithType.FAVORITE_QUESTION
            || talkStepIndex != 2
            || !State.IsSingle(_girlPair.GetValue<GirlPairDefinition>(_talkManager)))
        {
            return true;
        }

        //replace step 3 for single date pairs
        talkStepIndex++;
        _talkStepIndex.SetValue(_talkManager, talkStepIndex);

        var filePair = _fileGirlPair.GetValue<PlayerFileGirlPair>(_talkManager);

        //ignore response from other girl, cuz there is no other girl
        filePair.LearnFavAnswer(_talkManager.favQuestionDefinitions[Game.Session.Dialog.selectedDialogOptionIndex]);
        _talkStep.Invoke(_talkManager, null);

        return false;
    }

    private void OnDialogOptionSelected()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogOptionSelected;
        _talkStep.Invoke(_talkManager, null);
    }
}