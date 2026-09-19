using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.ModGameData;
using UnityEngine;

namespace Hp2BaseMod.Utility;

/// <summary>
/// Factories for every CutsceneStepSubDefinition/CutsceneStepInfo variant.
///
/// Most factories default ProceedType to AUTOMATIC (by far the common case) and, for steps that carry
/// a doll target, default it to FOCUSED. Use the fluent extensions in CutsceneStepFluentExtensions.cs
/// (.Proceed(...), .TargetFocused(), .TargetRandom(), .TargetGirl(...), .TargetOrientation(...)) to
/// override either when the default doesn't fit, e.g.:
///
///   CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.LEFT).TargetGirl(someGirlId)
/// </summary>
public static class CutsceneStepUtility
{
    public static CutsceneStepSubDefinition MakeNothing(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.NOTHING,
            proceedType = proceedType,
        };
    }

    public static CutsceneStepInfo MakeNothingInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.NOTHING,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeWait(float duration)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.NOTHING,
            proceedType = CutsceneStepProceedType.WAIT,
            proceedFloat = duration
        };
    }

    public static CutsceneStepInfo MakeWaitInfo(float duration)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.NOTHING,
            ProceedType = CutsceneStepProceedType.WAIT,
            ProceedFloat = duration
        };
    }

    public static CutsceneStepSubDefinition MakeBranch(IEnumerable<CutsceneBranchSubDefinition> branches, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.BRANCH,
            branches = branches.ToList(),
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeBranchInfo(IEnumerable<IGameDefinitionInfo<CutsceneBranchSubDefinition>> branches, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.BRANCH,
            BranchInfos = branches.ToList(),
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeGameAction(LogicAction logicAction, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.GAME_ACTION,
            logicAction = logicAction,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeGameActionInfo(LogicActionInfo logicAction, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.GAME_ACTION,
            LogicActionInfo = logicAction,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeSpecialStep(CutsceneStepSpecial cutsceneStepSpecial, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SPECIAL_STEP,
            specialStepPrefab = cutsceneStepSpecial,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeSpecialStepInfo(string cutsceneStepSpecialName, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SPECIAL_STEP,
            SpecialStepPrefabName = cutsceneStepSpecialName,
            ProceedType = proceedType
        };
    }

    /// <summary>Change the doll's expression. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeChangeExpression(GirlExpressionType girlExpressionType, float breathSpeed, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.CHANGE_EXPRESSION,
            floatValue = breathSpeed,
            expressionType = girlExpressionType,
            setMood = false,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED,
            proceedType = proceedType
        };
    }

    /// <summary>Change the doll's expression. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeChangeExpressionInfo(GirlExpressionType girlExpressionType, float breathSpeed, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.CHANGE_EXPRESSION,
            FloatValue = breathSpeed,
            ExpressionType = girlExpressionType,
            SetMood = false,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED,
            ProceedType = proceedType
        };
    }

    /// <summary>Set the doll's mood. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeSetMood(GirlExpressionType girlExpressionType, bool eyesClosed, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.CHANGE_EXPRESSION,
            boolValue = eyesClosed,
            expressionType = girlExpressionType,
            setMood = true,
            proceedType = proceedType,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED
        };
    }

    /// <summary>Set the doll's mood. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeSetMoodInfo(GirlExpressionType girlExpressionType, bool eyesClosed, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.CHANGE_EXPRESSION,
            BoolValue = eyesClosed,
            ExpressionType = girlExpressionType,
            SetMood = true,
            ProceedType = proceedType,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED
        };
    }

    /// <summary>Show a dialog line. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeDialogLine(DialogLine dialogLine, bool isDialogBoxLocked, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DIALOG_LINE,
            boolValue = isDialogBoxLocked,
            proceedType = proceedType,
            dialogLine = dialogLine,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED
        };
    }

    /// <summary>
    /// Show a dialog line. Target defaults to FOCUSED; chain .TargetGirl(...), .TargetOrientation(...),
    /// or .TargetRandom() to override (this replaces the old per-target-type overloads).
    /// </summary>
    public static CutsceneStepInfo MakeDialogLineInfo(IDialogLineDataMod dialogLine, bool isDialogBoxLocked = false, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.DIALOG_LINE,
            BoolValue = isDialogBoxLocked,
            ProceedType = proceedType,
            DialogLine = dialogLine,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED
        };
    }

    /// <summary>Trigger dialog. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeDialogTrigger(DialogTriggerDefinition dialogTriggerDefinition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DIALOG_TRIGGER,
            dialogTriggerDefinition = dialogTriggerDefinition,
            proceedType = proceedType,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED
        };
    }

    /// <summary>Trigger dialog. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeDialogTriggerInfo(RelativeId dialogTriggerDefinitionId, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.DIALOG_TRIGGER,
            DialogTriggerDefinitionId = dialogTriggerDefinitionId,
            ProceedType = proceedType,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED
        };
    }

    // MakeDoubleTrigger doesn't expose a doll target concept in the base game data, so it isn't part
    // of the fluent-target group; only ProceedType gets the default treatment here.
    public static CutsceneStepSubDefinition MakeDoubleTrigger(bool usePuzzleFocus, DialogTriggerDefinition dialogTrigger, bool toHidden, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DIALOG_TRIGGER,
            dialogTriggerDefinition = dialogTrigger,
            boolValue = usePuzzleFocus,
            proceedBool = toHidden,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeDoubleTriggerInfo(bool usePuzzleFocus, RelativeId dialogTriggerId, bool toHidden, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.DIALOG_TRIGGER,
            DialogTriggerDefinitionId = dialogTriggerId,
            BoolValue = usePuzzleFocus,
            ProceedBool = toHidden,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeDialogOptions(IEnumerable<CutsceneDialogOptionSubDefinition> options, bool shuffle, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DIALOG_OPTIONS,
            dialogOptions = options.ToList(),
            boolValue = shuffle,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeDialogOptionsInfo(IEnumerable<IGameDefinitionInfo<CutsceneDialogOptionSubDefinition>> options, bool shuffle, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.DIALOG_OPTIONS,
            DialogOptionInfos = options.ToList(),
            BoolValue = shuffle,
            ProceedType = proceedType
        };
    }

    /// <summary>
    /// Move a doll. Target defaults to FOCUSED; chain .TargetRandom(), .TargetGirl(...), or
    /// .TargetOrientation(...) to override (this replaces the old MakeRandomDollMove/MakeFocusedDollMove/
    /// per-target-type overloads).
    /// </summary>
    public static CutsceneStepSubDefinition MakeDollMove(DollPositionType positionType, float duration = 1f, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DOLL_MOVE,
            dollPositionType = positionType,
            floatValue = duration,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED,
            proceedType = proceedType
        };
    }

    /// <summary>
    /// Move a doll. Target defaults to FOCUSED; chain .TargetRandom(), .TargetGirl(...), or
    /// .TargetOrientation(...) to override (this replaces the old MakeRandomDollMoveInfo/
    /// MakeFocusedDollMoveInfo/per-target-type overloads).
    /// </summary>
    public static CutsceneStepInfo MakeDollMoveInfo(DollPositionType positionType, float duration = 1f, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.DOLL_MOVE,
            DollPositionType = positionType,
            FloatValue = duration,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED,
            ProceedType = proceedType
        };
    }

    /// <summary>Load a girl into a doll slot. Slot defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeLoadGirl(GirlDefinition girlDefinition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.LOAD_GIRL,
            girlDefinition = girlDefinition,
            boolValue = true,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED,
            proceedType = proceedType
        };
    }

    /// <summary>Load a girl into a doll slot. Slot defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeLoadGirlInfo(RelativeId girlDefinitionId, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.LOAD_GIRL,
            GirlDefinitionId = girlDefinitionId,
            BoolValue = true,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED,
            ProceedType = proceedType
        };
    }

    /// <summary>Load a girl with a specific look into a doll slot. Slot defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeLoadGirl(GirlDefinition girlDefinition, int expression, int hairstyle, int outfit, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.LOAD_GIRL,
            girlDefinition = girlDefinition,
            boolValue = false,
            expressionIndex = expression,
            hairstyleIndex = hairstyle,
            outfitIndex = outfit,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED,
            proceedType = proceedType
        };
    }

    /// <summary>Load a girl with a specific look into a doll slot. Slot defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeLoadGirlInfo(RelativeId girlDefinitionId, RelativeId expression, RelativeId hairstyle, RelativeId outfit, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.LOAD_GIRL,
            GirlDefinitionId = girlDefinitionId,
            BoolValue = false,
            ExpressionId = expression,
            HairstyleId = hairstyle,
            OutfitId = outfit,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED,
            ProceedType = proceedType
        };
    }

    /// <summary>Unload a girl from a doll slot. Slot defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeUnloadGirl(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.LOAD_GIRL,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED,
            proceedType = proceedType
        };
    }

    /// <summary>Unload a girl from a doll slot. Slot defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeUnloadGirlInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.LOAD_GIRL,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeTogglePhone(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.TOGGLE_PHONE,
            intValue = -1,
            boolValue = leftPosition_centerPosition,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeTogglePhoneInfo(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.TOGGLE_PHONE,
            IntValue = -1,
            BoolValue = leftPosition_centerPosition,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeToggleHeader(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.TOGGLE_PHONE,
            intValue = 1,
            boolValue = leftPosition_centerPosition,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeToggleHeaderInfo(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.TOGGLE_PHONE,
            IntValue = 1,
            BoolValue = leftPosition_centerPosition,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeTogglePhoneAndHeader(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.TOGGLE_PHONE,
            intValue = 0,
            boolValue = leftPosition_centerPosition,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeTogglePhoneAndHeaderInfo(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.TOGGLE_PHONE,
            IntValue = 0,
            BoolValue = leftPosition_centerPosition,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeRewind(int steps, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.REWIND,
            intValue = steps,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeRewindInfo(int steps, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.REWIND,
            IntValue = steps,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeShowPuzzleGrid(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.PUZZLE_GRID,
            boolValue = true,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeShowPuzzleGridInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.PUZZLE_GRID,
            BoolValue = true,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeHidePuzzleGrid(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.PUZZLE_GRID,
            boolValue = false,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeHidePuzzleGridInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.PUZZLE_GRID,
            BoolValue = false,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakePuzzleRefocus(bool expressFailure, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.PUZZLE_REFOCUS,
            boolValue = expressFailure,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakePuzzleRefocusInfo(bool expressFailure, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.PUZZLE_REFOCUS,
            BoolValue = expressFailure,
            ProceedType = proceedType
        };
    }

    /// <summary>Set doll exhaustion. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeSetExhaustion(bool exhausted, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SET_EXHAUSTION,
            boolValue = exhausted,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED,
            proceedType = proceedType
        };
    }

    /// <summary>Set doll exhaustion. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeSetExhaustionInfo(bool exhausted, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SET_EXHAUSTION,
            BoolValue = exhausted,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeSubCutsceneStraight(CutsceneDefinition cutsceneDefinition, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SUB_CUTSCENE,
            subCutsceneType = CutsceneStepSubCutsceneType.STRAIGHT,
            subCutsceneDefinition = cutsceneDefinition,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeSubCutsceneStraight(RelativeId cutsceneDefinitionId, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SUB_CUTSCENE,
            SubCutsceneType = CutsceneStepSubCutsceneType.STRAIGHT,
            SubCutsceneDefinitionId = cutsceneDefinitionId,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeSubCutsceneGirlPair(GirlPairRelationshipType relationshipType, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SUB_CUTSCENE,
            subCutsceneType = CutsceneStepSubCutsceneType.GIRL_PAIR,
            girlPairRelationshipType = relationshipType,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeSubCutsceneGirlPairInfo(GirlPairRelationshipType relationshipType, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SUB_CUTSCENE,
            SubCutsceneType = CutsceneStepSubCutsceneType.GIRL_PAIR,
            GirlPairRelationshipType = relationshipType,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeSubCutsceneGirlPairRound(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SUB_CUTSCENE,
            subCutsceneType = CutsceneStepSubCutsceneType.GIRL_PAIR_ROUND,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeSubCutsceneGirlPairRoundInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SUB_CUTSCENE,
            SubCutsceneType = CutsceneStepSubCutsceneType.GIRL_PAIR_ROUND,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeSubCutsceneInner(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SUB_CUTSCENE,
            subCutsceneType = CutsceneStepSubCutsceneType.INNER,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeSubCutsceneInnerInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SUB_CUTSCENE,
            SubCutsceneType = CutsceneStepSubCutsceneType.INNER,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeShowWindow(bool queue, UiWindow windowPrefab, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SHOW_WINDOW,
            boolValue = !queue,
            windowPrefab = windowPrefab,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeShowWindowInfo(bool queue, string windowPrefabName, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SHOW_WINDOW,
            BoolValue = !queue,
            WindowPrefabName = windowPrefabName,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeHideWindow(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SHOW_WINDOW,
            boolValue = false,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeHideWindowInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SHOW_WINDOW,
            BoolValue = false,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeUseCellphone(IEnumerable<int> freezeButtonIndexes, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.USE_CELLPHONE,
            stringValue = freezeButtonIndexes == null
                ? null
                : string.Join(",", freezeButtonIndexes),
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeUseCellphoneInfo(IEnumerable<int> freezeButtonIndexes, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.USE_CELLPHONE,
            StringValue = freezeButtonIndexes == null
                ? null
                : string.Join(",", freezeButtonIndexes),
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeShakeScreen(float duration, int strength, bool fadeOut, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SHAKE_SCREEN,
            floatValue = duration,
            intValue = strength,
            boolValue = fadeOut,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeShakeScreenInfo(float duration, int strength, bool fadeOut, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SHAKE_SCREEN,
            FloatValue = duration,
            IntValue = strength,
            BoolValue = fadeOut,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeResetDolls(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.RESET_DOLLS,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeResetDollsInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.RESET_DOLLS,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeToggleOverlay(bool off_on, float duration, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.TOGGLE_OVERLAY,
            boolValue = off_on,
            floatValue = duration,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeToggleOverlayInfo(bool off_on, float duration, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.TOGGLE_OVERLAY,
            BoolValue = off_on,
            FloatValue = duration,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeSoundEffect(bool sound_voice, AudioKlip audioKlip, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SOUND_EFFECT,
            boolValue = sound_voice,
            audioKlip = audioKlip,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeSoundEffectInfo(bool sound_voice, AudioKlipInfo audioKlip, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SOUND_EFFECT,
            BoolValue = sound_voice,
            AudioKlipInfo = audioKlip,
            ProceedType = proceedType
        };
    }

    /// <summary>Clear doll mood. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeClearMood(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.CLEAR_MOOD,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED,
            proceedType = proceedType
        };
    }

    /// <summary>Clear doll mood. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeClearMoodInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.CLEAR_MOOD,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeParticleEmitter(EmitterBehavior emitter, int effectContainer, Vector2 position, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.PARTICLE_EMITTER,
            emitterBehavior = emitter,
            intValue = effectContainer,
            position = position,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeParticleEmitterInfo(string emitterName, int effectContainer, IGameDefinitionInfo<Vector2> position, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.PARTICLE_EMITTER,
            EmitterBehaviorName = emitterName,
            IntValue = effectContainer,
            PositionInfo = position,
            ProceedType = proceedType
        };
    }

    /// <summary>Show a notification. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepSubDefinition MakeShowNotification(string text, CutsceneStepNotificationType type, float duration, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SHOW_NOTIFICATION,
            stringValue = text,
            notificationType = type,
            floatValue = duration,
            dollTargetType = CutsceneStepDollTargetType.FOCUSED,
            proceedType = proceedType
        };
    }

    /// <summary>Show a notification. Target defaults to FOCUSED; chain .TargetX() to override.</summary>
    public static CutsceneStepInfo MakeShowNotificationInfo(string text, CutsceneStepNotificationType type, float duration, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.SHOW_NOTIFICATION,
            StringValue = text,
            NotificationType = type,
            FloatValue = duration,
            DollTargetType = CutsceneStepDollTargetType.FOCUSED,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeBannerTextHide(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.BANNER_TEXT,
            boolValue = false,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeBannerTextHideInfo(CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.BANNER_TEXT,
            BoolValue = false,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeBannerText(BannerTextBehavior bannerTextBehavior, int effectsContainerIndex, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.BANNER_TEXT,
            bannerTextPrefab = bannerTextBehavior,
            intValue = effectsContainerIndex,
            boolValue = true,
            proceedType = proceedType
        };
    }

    public static CutsceneStepInfo MakeBannerTextInfo(string bannerTextBehaviorName, int effectsContainerIndex, CutsceneStepProceedType proceedType = CutsceneStepProceedType.AUTOMATIC)
    {
        return new CutsceneStepInfo()
        {
            StepType = CutsceneStepType.BANNER_TEXT,
            BannerTextPrefabName = bannerTextBehaviorName,
            IntValue = effectsContainerIndex,
            BoolValue = true,
            ProceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeChangeLocation(params RelativeId[] locationIdPool)
    {
        return new ChangeLocationCutsceneStep(locationIdPool);
    }

    public static ChangeLocationCutsceneStep.Info MakeChangeLocationInfo(params RelativeId[] locationIdPool)
    {
        return new ChangeLocationCutsceneStep.Info(locationIdPool);
    }

    public static CutsceneStepSubDefinition MakeChangeGameState(RelativeId targetStateId) 
    { 
        return new ChangeGameStateCutsceneStep(targetStateId); 
    }
}
