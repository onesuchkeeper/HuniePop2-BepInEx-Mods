using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using HarmonyLib;
using Hp2BaseMod.Extension;
using Hp2BaseMod.ModGameData.Interface;
using Hp2BaseMod.Utility;

namespace Hp2BaseMod;

[HarmonyPatch(typeof(CutsceneManager))]
internal static class CutsceneManagerPatch
{
    [HarmonyPatch("NextStep")]
    [HarmonyPrefix]
    public static bool NextStep(CutsceneManager __instance, bool resetSequence = true)
        => ExpandedCutsceneManager.Get(__instance).NextStep(resetSequence);
}

public class ExpandedCutsceneManager
{
    private static Dictionary<CutsceneManager, ExpandedCutsceneManager> _expansions
        = new Dictionary<CutsceneManager, ExpandedCutsceneManager>();

    public static ExpandedCutsceneManager Get(CutsceneManager core)
    {
        if (!_expansions.TryGetValue(core, out var expansion))
        {
            expansion = new ExpandedCutsceneManager(core);
            _expansions[core] = expansion;
        }

        return expansion;
    }

    private static FieldInfo _currentStep = AccessTools.Field(typeof(CutsceneManager), "_currentStep");
    private static FieldInfo _branchStepIndices = AccessTools.Field(typeof(CutsceneManager), "_branchStepIndices");
    private static FieldInfo _branches = AccessTools.Field(typeof(CutsceneManager), "_branches");
    private static FieldInfo _audioLink = AccessTools.Field(typeof(CutsceneManager), "_audioLink");
    private static FieldInfo _stepSequence = AccessTools.Field(typeof(CutsceneManager), "_stepSequence");
    private static MethodInfo m_nextStep = AccessTools.Method(typeof(CutsceneManager), "NextStep");

    protected CutsceneManager _core;
    private ExpandedCutsceneManager(CutsceneManager core)
    {
        _core = core;
    }

    public bool NextStep(bool resetSequence)
    {
        var branchStepIndices = _branchStepIndices.GetValue<List<int>>(_core);
        var branches = _branches.GetValue<List<List<CutsceneStepSubDefinition>>>(_core);

        //don't actually change the index yet in case we don't end up processing
        var currentBranchStepIndex = branchStepIndices[_core.currentBranchIndex] + 1;

        if (currentBranchStepIndex >= branches[_core.currentBranchIndex].Count)
        {
            return true;
        }

        var currentStep = branches[_core.currentBranchIndex][currentBranchStepIndex];
        var stepSequence = _stepSequence.GetValue<Sequence>(_core);

        if (currentStep is IFunctionalCutsceneStep functional)
        {
            branchStepIndices[_core.currentBranchIndex] = currentBranchStepIndex;
            functional.Complete += On_FunctionStep_Complete;

            if (resetSequence)
            {
                Game.Manager.Time.KillTween(stepSequence, true, true);
                stepSequence = DOTween.Sequence();
                _stepSequence.SetValue(_core, stepSequence);
            }

            functional.Act();

            return false;
        }

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

        if (resetSequence)
        {
            Game.Manager.Time.KillTween(stepSequence, true, true);
            stepSequence = DOTween.Sequence();
            _stepSequence.SetValue(_core, stepSequence);
        }

        branchStepIndices[_core.currentBranchIndex] = currentBranchStepIndex;
        _audioLink.SetValue(_core, null);
        _currentStep.SetValue(_core, currentStep);

        CutsceneManagerUtility.HandleStepType(currentStep,
            branches,
            branchStepIndices,
            uiDoll,
            stepSequence);

        return false;
    }

    private void On_FunctionStep_Complete(IFunctionalCutsceneStep sender)
    {
        sender.Complete -= On_FunctionStep_Complete;
        m_nextStep.Invoke(_core, [false]);
    }
}
