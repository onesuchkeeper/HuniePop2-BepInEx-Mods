using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using SingleDate;
using UnityEngine;

[HarmonyPatch(typeof(LocationTransitionNormal))]
public static class LocationTransitionNormalPatch
{
    [HarmonyPatch("ArriveStep")]
    [HarmonyPostfix]
    public static void ArriveStep(LocationTransitionNormal __instance)
        => ExtendedLocationTransitionNormal.Get(__instance).ArriveStep();

    [HarmonyPatch("DepartStep")]
    [HarmonyPrefix]
    public static bool DepartStep(LocationTransitionNormal __instance)
        => ExtendedLocationTransitionNormal.Get(__instance).DepartStep();
}

public class ExtendedLocationTransitionNormal
{
    private static Dictionary<LocationTransitionNormal, ExtendedLocationTransitionNormal> _extendedTransitions
        = new Dictionary<LocationTransitionNormal, ExtendedLocationTransitionNormal>();

    public static ExtendedLocationTransitionNormal Get(LocationTransitionNormal locationTransitionNormal)
    {
        if (!_extendedTransitions.TryGetValue(locationTransitionNormal, out var extended))
        {
            extended = new ExtendedLocationTransitionNormal(locationTransitionNormal);
            _extendedTransitions[locationTransitionNormal] = extended;
        }

        return extended;
    }

    private static readonly FieldInfo _stepIndex = AccessTools.Field(typeof(LocationTransitionNormal), "_stepIndex");
    private static readonly MethodInfo _departStep = AccessTools.Method(typeof(LocationTransitionNormal), "DepartStep");

    private readonly LocationTransitionNormal _core;

    public ExtendedLocationTransitionNormal(LocationTransitionNormal core)
    {
        _core = core;
    }

    public void ArriveStep()
    {
        var stepIndex = (int)_stepIndex.GetValue(_core);

        if (stepIndex != 0)
        {
            return;
        }

        if (!State.IsLocationPairSingle())
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
        var stepIndex = (int)_stepIndex.GetValue(_core);

        if (stepIndex != 0
            || !Game.Session.Location.AtLocationType([LocationType.SIM])
            //the location's currentGirlPair is preserved while the player file current girl is changed, so use that
            || !State.IsLocationPairSingle())
        {
            return true;
        }

        ModInterface.Log.LogInfo("Forcing LocationTransitionNormal to girl on right for single date");

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

        var dialogTriggerDefinition = Game.Persistence.playerFile.locationDefinition.locationType == LocationType.DATE
            ? Game.Session.Location.dtAskDate
            : Game.Session.Location.dtValediction;

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
