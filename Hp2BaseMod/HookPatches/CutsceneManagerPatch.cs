using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(CutsceneManager))]
internal static class CutsceneManagerPatch
{
    private static FieldInfo _currentStep = AccessTools.Field(typeof(CutsceneManager), "_currentStep");
    private static FieldInfo _branchStepIndices = AccessTools.Field(typeof(CutsceneManager), "_branchStepIndices");
    private static FieldInfo _branches = AccessTools.Field(typeof(CutsceneManager), "_branches");
    private static FieldInfo _audioLink = AccessTools.Field(typeof(CutsceneManager), "_audioLink");
    private static FieldInfo _stepSequence = AccessTools.Field(typeof(CutsceneManager), "_stepSequence");

    [HarmonyPatch("NextStep")]
    [HarmonyPrefix]
    public static bool NextStep(CutsceneManager __instance, bool resetSequence = true)
    {
        //Some cutscene steps select a girl at random

        var branchStepIndices = _branchStepIndices.GetValue<List<int>>(__instance);
        var branches = _branches.GetValue<List<List<CutsceneStepSubDefinition>>>(__instance);

        //don't actually change the index yet in case we don't end up processing
        var currentBranchStepIndex = branchStepIndices[__instance.currentBranchIndex] + 1;

        if (currentBranchStepIndex >= branches[__instance.currentBranchIndex].Count)
        {
            return true;
        }

        var currentStep = branches[__instance.currentBranchIndex][currentBranchStepIndex];

        if (currentStep.skipStep
            || (currentStep.stepType.ToString().ToUpper().Contains("PUZZLE")
                && !Game.Session.Location.AtLocationType(LocationType.DATE)))
        {
            return true;
        }

        if (!CutsceneManagerUtility.WillDollBeRandomized(currentStep))
        {
            return true;
        }

        var args = new RandomDollSelectedArgs();
        ModInterface.Events.NotifyRandomDollSelected(args);
        var uiDoll = args.SelectedDoll ?? Game.Session.gameCanvas.GetDoll(MathUtils.RandomBool());

        //we're committed, no more return trues, apply changes to the actual branchStepIndices and use that from now on
        //and set the other values
        var stepSequence = _stepSequence.GetValue<Sequence>(__instance);

        if (resetSequence)
        {
            Game.Manager.Time.KillTween(stepSequence, true, true);
            stepSequence = DOTween.Sequence();
            _stepSequence.SetValue(__instance, stepSequence);
        }

        branchStepIndices[__instance.currentBranchIndex] = currentBranchStepIndex;
        _audioLink.SetValue(__instance, null);
        _currentStep.SetValue(__instance, currentStep);

        CutsceneManagerUtility.HandleStepType(currentStep,
            branches,
            branchStepIndices,
            uiDoll,
            stepSequence);

        return false;
    }
}
