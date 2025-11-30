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

    private static FieldInfo f_talkStepIndex = AccessTools.Field(typeof(TalkManager), "_talkStepIndex");
    private static FieldInfo f_girlPair = AccessTools.Field(typeof(TalkManager), "_girlPair");
    private static FieldInfo f_fileGirlPair = AccessTools.Field(typeof(TalkManager), "_fileGirlPair");
    private static FieldInfo f_questionPool = AccessTools.Field(typeof(TalkManager), "_questionPool");
    private static MethodInfo m_talkStep = AccessTools.Method(typeof(TalkManager), "TalkStep");

    private TalkManager _talkManager;

    public ExpandedTalkManager(TalkManager talkManager)
    {
        _talkManager = talkManager;
    }

    public bool TalkStep()
    {
        var talkStepIndex = f_talkStepIndex.GetValue<int>(_talkManager);

        if (_talkManager.talkType != TalkWithType.FAVORITE_QUESTION
            || talkStepIndex != 2
            || !State.IsSingle(f_girlPair.GetValue<GirlPairDefinition>(_talkManager)))
        {
            return true;
        }

        //replace step 3 for single date pairs
        talkStepIndex++;
        f_talkStepIndex.SetValue(_talkManager, talkStepIndex);

        var filePair = f_fileGirlPair.GetValue<PlayerFileGirlPair>(_talkManager);

        //ignore response from other girl, cuz there is no other girl
        filePair.LearnFavAnswer(_talkManager.favQuestionDefinitions[Game.Session.Dialog.selectedDialogOptionIndex]);
        m_talkStep.Invoke(_talkManager, null);

        return false;
    }

    private void OnDialogOptionSelected()
    {
        Game.Session.Dialog.DialogOptionSelectedEvent -= OnDialogOptionSelected;
        m_talkStep.Invoke(_talkManager, null);
    }
}
