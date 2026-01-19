using System;
using UnityEngine;

namespace Hp2BaseMod.Utility;

public static partial class GameDataLogUtility
{
    public static void LogCutscene(CutsceneDefinition cutsceneDef)
    {
        if (cutsceneDef == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        ModInterface.Log.Message($"Cleanup Type: {Enum.GetName(typeof(CutsceneCleanUpType), cutsceneDef.cleanUpType)}");
        int i = 0;
        foreach (var step in cutsceneDef.steps)
        {
            ModInterface.Log.Message($"Step {i++}");
            ModInterface.Log.IncreaseIndent();
            LogCutsceneStep(step);
            ModInterface.Log.DecreaseIndent();
        }
    }

    public static void LogCutsceneStep(CutsceneStepSubDefinition step)
    {
        if (step == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Type: {Enum.GetName(typeof(CutsceneStepType), step.stepType)}"))
        {
            var usesTarget = false;
            switch (step.stepType)
            {
                case CutsceneStepType.BRANCH:
                    {
                        int i = 0;
                        foreach (var branch in step.branches)
                        {
                            ModInterface.Log.Message($"Branch {i++}:");
                            ModInterface.Log.IncreaseIndent();
                            LogCutsceneBranch(branch);
                            ModInterface.Log.DecreaseIndent();
                        }
                        break;
                    }
                case CutsceneStepType.GAME_ACTION:
                    LogLogicAction(step.logicAction);
                    break;
                case CutsceneStepType.SPECIAL_STEP:
                    ModInterface.Log.Message($"Special step prefab: {step.specialStepPrefab?.name ?? "null"}");
                    break;
                case CutsceneStepType.CHANGE_EXPRESSION:
                    usesTarget = true;
                    ModInterface.Log.Message($"setMood: {step.setMood}");
                    ModInterface.Log.Message($"expressionType: {Enum.GetName(typeof(GirlExpressionType), step.expressionType)}");
                    if (step.setMood)
                    {
                        ModInterface.Log.Message($"SetMood, EyesClosed: {step.boolValue}");
                    }
                    else
                    {
                        ModInterface.Log.Message($"ChangeExpression, BreathSpeed: {step.floatValue}");
                    }
                    break;
                case CutsceneStepType.DIALOG_LINE:
                    usesTarget = true;
                    ModInterface.Log.Message($"DialogLine: {step.dialogLine?.dialogText ?? "null"}");//fix this
                    ModInterface.Log.Message($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}");
                    ModInterface.Log.Message($"isDialogBoxLocked: {step.boolValue}");
                    break;
                case CutsceneStepType.DIALOG_TRIGGER:
                    usesTarget = true;
                    ModInterface.Log.Message($"DialogTrigger: {step.dialogTriggerDefinition.name}");

                    ModInterface.Log.Message($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}");
                    break;
                case CutsceneStepType.DOUBLE_TRIGGER:
                    ModInterface.Log.Message($"Use puzzle focus:{step.boolValue}");

                    ModInterface.Log.Message($"Trigger: {step.dialogTriggerDefinition.name}");

                    ModInterface.Log.Message($"Response: {step.dialogTriggerDefinition.responseTrigger.name}");

                    ModInterface.Log.Message($"toHidden:{step.proceedBool}");
                    break;
                case CutsceneStepType.DIALOG_OPTIONS:
                    {
                        var i = 0;
                        foreach (var option in step.dialogOptions)
                        {
                            ModInterface.Log.Message($"DialogOption {i}");
                            ModInterface.Log.IncreaseIndent();
                            LogDialogOption(option);
                            ModInterface.Log.DecreaseIndent();
                        }
                        ModInterface.Log.Message($"Shuffle: {step.boolValue}");
                        break;
                    }
                case CutsceneStepType.DOLL_MOVE:
                    usesTarget = true;
                    ModInterface.Log.Message($"dollPositionType: {Enum.GetName(typeof(DollPositionType), step.dollPositionType)}");
                    ModInterface.Log.Message($"Duration: {(step.floatValue > 0 ? step.floatValue : 1f)}");
                    ModInterface.Log.Message($"TargetType: {Enum.GetName(typeof(CutsceneStepDollTargetType), step.dollTargetType)}");
                    switch (step.dollTargetType)
                    {
                        case CutsceneStepDollTargetType.GIRL_DEFINITION:
                            if (step.girlDefinition == null)
                            {
                                ModInterface.Log.Message($"Def: null");
                            }
                            else
                            {
                                ModInterface.Log.Message($"Def: {ModInterface.Data.GetDataId(GameDataType.Girl, step.girlDefinition.id)} - {step.girlDefinition.girlName}");
                            }
                            break;
                        case CutsceneStepDollTargetType.ORIENTATION_TYPE:
                            ModInterface.Log.Message($"Orientation: {Enum.GetName(typeof(DollOrientationType), step.targetDollOrientation)}");
                            break;
                        case CutsceneStepDollTargetType.FOCUSED:
                        case CutsceneStepDollTargetType.RANDOM:
                            break;
                        default:
                            ModInterface.Log.Message($"Unknown target type: {step.dollTargetType}");
                            break;
                    }
                    break;
                case CutsceneStepType.LOAD_GIRL:
                    usesTarget = true;
                    if (step.girlDefinition == null)
                    {
                        ModInterface.Log.Message("unload");
                    }
                    else
                    {
                        ModInterface.Log.Message($"GirlDef: {step.girlDefinition.name}");

                        if (step.boolValue)
                        {
                            ModInterface.Log.Message($"Default expressions/styles");
                        }
                        else
                        {
                            ModInterface.Log.Message($"Expression: {step.expressionIndex}");
                            ModInterface.Log.Message($"Hairstyle: {step.hairstyleIndex}");
                            ModInterface.Log.Message($"Outfit: {step.outfitIndex}");
                        }
                    }
                    break;
                case CutsceneStepType.TOGGLE_PHONE:
                    if (step.intValue == 0)
                    {
                        ModInterface.Log.Message("Both Header and Lower Cellphone");
                    }
                    else
                    {
                        ModInterface.Log.Message(step.intValue > 0 ? "Header" : "Lower Cellphone");
                    }

                    ModInterface.Log.Message(step.boolValue ? "Hub position" : "Normal Position");
                    break;
                case CutsceneStepType.REWIND:
                    ModInterface.Log.Message($"Steps: {Mathf.Abs(step.intValue)}");
                    break;
                case CutsceneStepType.PUZZLE_GRID:
                    ModInterface.Log.Message(step.boolValue ? "Show" : "Hide");
                    break;
                case CutsceneStepType.BANNER_TEXT:
                    ModInterface.Log.Message(step.boolValue ? "Show" : "Hide");
                    if (step.boolValue)
                    {
                        ModInterface.Log.Message($"BannerTextPrefab: {step.bannerTextPrefab.name}");
                        ModInterface.Log.Message($"Effect Index: {step.intValue}");
                    }
                    break;
                case CutsceneStepType.PUZZLE_REFOCUS:
                    ModInterface.Log.Message($"Express Failure: {step.boolValue}");
                    break;
                case CutsceneStepType.SET_EXHAUSTION:
                    usesTarget = true;
                    ModInterface.Log.Message($"Exhausted: {step.boolValue}");
                    break;
                case CutsceneStepType.SUB_CUTSCENE:
                    ModInterface.Log.Message($"subCutsceneType: {Enum.GetName(typeof(CutsceneStepSubCutsceneType), step.subCutsceneType)}");
                    switch (step.subCutsceneType)
                    {
                        case CutsceneStepSubCutsceneType.STRAIGHT:
                            LogCutscene(step.subCutsceneDefinition);
                            break;
                        case CutsceneStepSubCutsceneType.GIRL_PAIR:
                            ModInterface.Log.Message($"girlPairRelationshipType: {Enum.GetName(typeof(GirlPairRelationshipType), step.girlPairRelationshipType)}");
                            break;
                    }
                    break;
                case CutsceneStepType.SHOW_WINDOW:
                    ModInterface.Log.Message($"Don't Queue: {step.boolValue}");
                    ModInterface.Log.Message($"WindowPrefab: {step.windowPrefab?.name ?? "null"}");
                    break;
                case CutsceneStepType.USE_CELLPHONE:
                    if (!StringUtils.IsEmpty(step.stringValue))
                    {
                        ModInterface.Log.Message($"Freeze Button Indexes: {step.stringValue}");
                    }
                    break;
                case CutsceneStepType.SHAKE_SCREEN:
                    ModInterface.Log.Message($"Duration: {step.floatValue}");
                    ModInterface.Log.Message($"Strength: {step.intValue}");
                    ModInterface.Log.Message($"FadeOut: {step.boolValue}");
                    break;
                case CutsceneStepType.RESET_DOLLS:
                    //no arguments
                    break;
                case CutsceneStepType.TOGGLE_OVERLAY:
                    ModInterface.Log.Message(step.boolValue ? "On" : "Off");
                    ModInterface.Log.Message($"Duration: {step.floatValue}");
                    break;
                case CutsceneStepType.SOUND_EFFECT:
                    ModInterface.Log.Message(step.boolValue ? "Voice" : "Sound");
                    ModInterface.Log.Message($"Volume: {step.audioKlip.volume}");
                    ModInterface.Log.Message($"Clip: {step.audioKlip.clip.name}");
                    break;
                case CutsceneStepType.CLEAR_MOOD:
                    //no arguments
                    break;
                case CutsceneStepType.PARTICLE_EMITTER:
                    ModInterface.Log.Message($"EmitterBehavior: {step.emitterBehavior.name}");
                    ModInterface.Log.Message($"Effect Container: {step.intValue}");
                    ModInterface.Log.Message($"Position: {step.position}");
                    break;
                case CutsceneStepType.SHOW_NOTIFICATION:
                    ModInterface.Log.Message($"Text: {step.stringValue}");
                    ModInterface.Log.Message($"NotificationType: {Enum.GetName(typeof(CutsceneStepNotificationType), step.notificationType)}");
                    ModInterface.Log.Message($"Show duration: {step.floatValue}");
                    break;
            }
            ModInterface.Log.Message($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}, Proceed Float: {step.proceedFloat}");

            if (usesTarget)
            {
                var targetStr = $"Doll Target Type: {Enum.GetName(typeof(CutsceneStepDollTargetType), step.dollTargetType)}";
                switch (step.dollTargetType)
                {
                    case CutsceneStepDollTargetType.GIRL_DEFINITION:
                        ModInterface.Log.Message(targetStr + $", Target Def: {step.targetGirlDefinition?.name ?? "null"} - {step.targetGirlDefinition?.girlName ?? "null"}");
                        break;
                    case CutsceneStepDollTargetType.ORIENTATION_TYPE:
                        ModInterface.Log.Message(targetStr + $", Orientation: {step.targetDollOrientation}");
                        break;
                    case CutsceneStepDollTargetType.RANDOM:
                        break;
                    case CutsceneStepDollTargetType.FOCUSED:
                        break;
                }
            }
        }
    }

    public static void LogCutsceneBranch(CutsceneBranchSubDefinition branch)
    {
        if (branch == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        int i = 0;
        foreach (var condition in branch.conditions)
        {
            using (ModInterface.Log.MakeIndent($"Condition {i++}:"))
            {
                LogLogicCondition(condition);
            }
        }

        i = 0;
        foreach (var step in branch.cutsceneDefinition?.steps ?? branch.steps)
        {
            using (ModInterface.Log.MakeIndent($"Step {i++}:"))
            {
                LogCutsceneStep(step);
            }
        }
    }

    public static void LogDialogOption(CutsceneDialogOptionSubDefinition dialogOption)
    {
        if (dialogOption == null)
        {
            ModInterface.Log.Message("null");
            return;
        }

        ModInterface.Log.Message($"Yuri: {dialogOption.yuri}");
        if (dialogOption.yuri)
        {
            ModInterface.Log.Message($"Yuri Text: {dialogOption.yuriDialogOptionText}");
        }
        ModInterface.Log.Message($"Text: {dialogOption.yuriDialogOptionText}");
        ModInterface.Log.Message($"Text: {dialogOption.yuriDialogOptionText}");
    }
}