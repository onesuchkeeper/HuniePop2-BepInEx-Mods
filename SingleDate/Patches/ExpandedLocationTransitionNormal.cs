using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using SingleDate;
using UnityEngine;

[HarmonyPatch(typeof(LocationTransitionNormal))]
internal static class LocationTransitionNormalPatch
{
    [HarmonyPatch("ArriveStep")]
    [HarmonyPostfix]
    public static void ArriveStep(LocationTransitionNormal __instance)
        => ExpandedLocationTransitionNormal.Get(__instance).ArriveStep();

    [HarmonyPatch("DepartStep")]
    [HarmonyPrefix]
    public static bool DepartStep(LocationTransitionNormal __instance)
        => ExpandedLocationTransitionNormal.Get(__instance).DepartStep();
}

internal class ExpandedLocationTransitionNormal
{
    private static Dictionary<LocationTransitionNormal, ExpandedLocationTransitionNormal> _extendedTransitions
        = new Dictionary<LocationTransitionNormal, ExpandedLocationTransitionNormal>();

    public static ExpandedLocationTransitionNormal Get(LocationTransitionNormal locationTransitionNormal)
    {
        if (!_extendedTransitions.TryGetValue(locationTransitionNormal, out var extended))
        {
            extended = new ExpandedLocationTransitionNormal(locationTransitionNormal);
            _extendedTransitions[locationTransitionNormal] = extended;
        }

        return extended;
    }

    private static readonly FieldInfo _stepIndex = AccessTools.Field(typeof(LocationTransitionNormal), "_stepIndex");
    private static readonly MethodInfo _departStep = AccessTools.Method(typeof(LocationTransitionNormal), "DepartStep");

    private static readonly FieldInfo _altGirlFocused = AccessTools.Field(typeof(PuzzleStatus), "_altGirlFocused");

    private readonly LocationTransitionNormal _core;

    public ExpandedLocationTransitionNormal(LocationTransitionNormal core)
    {
        _core = core;
    }

    public void ArriveStep()
    {
        if (_stepIndex.GetValue<int>(_core) != 0)
        {
            return;
        }

        if (!State.IsSingleDate)
        {

            Game.Session.Puzzle.puzzleGrid.transform.position = State.DefaultPuzzleGridPosition;
            return;
        }

        var delta = Game.Session.gameCanvas.header.xValues.y - Game.Session.gameCanvas.header.xValues.x;

        Game.Session.Puzzle.puzzleGrid.transform.position = new Vector3(State.DefaultPuzzleGridPosition.x + delta,
            State.DefaultPuzzleGridPosition.y);
    }

    public bool DepartStep()
    {
        var stepIndex = _stepIndex.GetValue<int>(_core);

        if (stepIndex != 0
            || !Game.Session.Location.AtLocationType([LocationType.SIM])
            || !State.IsSingleDate)
        {
            return true;
        }

        stepIndex++;
        _stepIndex.SetValue(_core, stepIndex);

        //case stepIndex 1:
        if (Game.Manager.Windows.IsWindowActive(null, true, true))
        {
            Game.Manager.Windows.WindowHiddenEvent += OnWindowHidden;
            Game.Manager.Windows.HideWindow();
        }
        else
        {
            _departStep.Invoke(_core, null);
        }

        var uiDoll = Game.Session.gameCanvas.dollRight;

        DialogTriggerDefinition dialogTriggerDefinition;
        if (Game.Persistence.playerFile.locationDefinition.locationType == LocationType.DATE)
        {
            //here we force the focus of the puzzle grid because the initial puzzle 'NextRound' occurs before the current pair is set
            ModInterface.Log.LogInfo("LocationTransitionNormal-Forcing puzzle focus to alt girl for single date");
            _altGirlFocused.SetValue(Game.Session.Puzzle.puzzleStatus, true);

            dialogTriggerDefinition = Game.Session.Location.dtAskDate;
        }
        else
        {
            dialogTriggerDefinition = Game.Session.Location.dtValediction;
        }

        if (dialogTriggerDefinition == null)
        {
            _departStep.Invoke(_core, null);
        }
        else
        {
            uiDoll.DialogBoxHiddenEvent += OnValedictionDialogRead;
            uiDoll.ReadDialogTrigger(dialogTriggerDefinition, DialogLineFormat.ACTIVE, -1);
        }

        return false;
    }

    private void OnWindowHidden()
    {
        Game.Manager.Windows.WindowHiddenEvent -= OnWindowHidden;
        _departStep.Invoke(_core, null);
    }

    private void OnValedictionDialogRead(UiDoll doll)
    {
        doll.DialogBoxHiddenEvent -= OnValedictionDialogRead;
        _departStep.Invoke(_core, null);
    }
}
