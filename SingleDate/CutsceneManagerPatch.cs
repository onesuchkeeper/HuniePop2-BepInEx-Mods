using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Utility;

namespace SingleDate;

[HarmonyPatch(typeof(CutsceneManager))]
public static class CutsceneManagerPatch
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
        if (!(State.IsLocationPairSingle()
                && _branchStepIndices.GetValue(__instance) is List<int> branchStepIndices
                && _branches.GetValue(__instance) is List<List<CutsceneStepSubDefinition>> branches))
        {
            return true;
        }

        var stepSequence = _stepSequence.GetValue(__instance) as Sequence;

        //don't actually change the index in case we don't end up processing
        var currentBranchStepIndex = branchStepIndices[__instance.currentBranchIndex] + 1;

        if (currentBranchStepIndex >= branches[__instance.currentBranchIndex].Count)
        {
            return true;
        }

        var currentStep = branches[__instance.currentBranchIndex][currentBranchStepIndex];

        if (currentStep.skipStep
            || (currentStep.stepType.ToString().ToUpper().Contains("PUZZLE")
                && !Game.Session.Location.AtLocationType([LocationType.DATE])))
        {
            return true;
        }

        if (!CutsceneManagerUtility.WillDollBeRandomized(currentStep))
        {
            return true;
        }

        //we're committed, no more return trues, apply changes to the actual branchStepIndices and use that from now on
        //and set the other values
        ModInterface.Log.LogInfo("Forcing cutscene random girl selection to the right for single date");

        if (resetSequence)
        {
            Game.Manager.Time.KillTween(stepSequence, true, true);
        }

        branchStepIndices[__instance.currentBranchIndex] = currentBranchStepIndex;
        _audioLink.SetValue(__instance, null);
        _currentStep.SetValue(__instance, currentStep);

        if (resetSequence)
        {
            stepSequence = DOTween.Sequence();
            _stepSequence.SetValue(__instance, stepSequence);
        }

        CutsceneManagerUtility.HandleStepType(currentStep,
            branches,
            branchStepIndices,
            Game.Session.gameCanvas.dollRight,
            stepSequence);

        return false;
    }
}