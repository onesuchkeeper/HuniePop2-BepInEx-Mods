using System;
using UnityEngine;

namespace Hp2BaseMod.Utility;

public static partial class GameDataLogUtility
{
    public static void Log(this CutsceneDefinition cutsceneDefinition) => LogCutscene(cutsceneDefinition);

    public static void LogCutscene(CutsceneDefinition cutsceneDef, ILogger logger = null)
    {
        logger ??= ModInterface.Log;

        if (cutsceneDef == null)
        {
            logger.Message("null");
            return;
        }

        logger.Message($"Cutscene {cutsceneDef.ModId()} - {cutsceneDef.name}");
        logger.Message($"Cleanup Type: {Enum.GetName(typeof(CutsceneCleanUpType), cutsceneDef.cleanUpType)}");
        int i = 0;
        foreach (var step in cutsceneDef.steps)
        {
            logger.Message($"Step {i++}");
            logger.IncreaseIndent();
            LogCutsceneStep(step, logger);
            logger.DecreaseIndent();
        }
    }

    public static void LogCutsceneStep(CutsceneStepSubDefinition step, ILogger logger)
    {
        if (step == null)
        {
            logger.Message("null");
            return;
        }

        using (logger.MakeIndent($"Type: {Enum.GetName(typeof(CutsceneStepType), step.stepType)}"))
        {
            var usesTarget = false;
            switch (step.stepType)
            {
                case CutsceneStepType.BRANCH:
                    {
                        int i = 0;
                        foreach (var branch in step.branches)
                        {
                            logger.Message($"Branch {i++}:");
                            logger.IncreaseIndent();
                            LogCutsceneBranch(branch, logger);
                            logger.DecreaseIndent();
                        }
                        break;
                    }
                case CutsceneStepType.GAME_ACTION:
                    LogLogicAction(step.logicAction, logger);
                    break;
                case CutsceneStepType.SPECIAL_STEP:
                    logger.Message($"Special step prefab: {step.specialStepPrefab?.name ?? "null"}");
                    break;
                case CutsceneStepType.CHANGE_EXPRESSION:
                    usesTarget = true;
                    logger.Message($"setMood: {step.setMood}");
                    logger.Message($"expressionType: {Enum.GetName(typeof(GirlExpressionType), step.expressionType)}");
                    if (step.setMood)
                    {
                        logger.Message($"SetMood, EyesClosed: {step.boolValue}");
                    }
                    else
                    {
                        logger.Message($"ChangeExpression, BreathSpeed: {step.floatValue}");
                    }
                    break;
                case CutsceneStepType.DIALOG_LINE:
                    usesTarget = true;
                    logger.Message($"DialogLine: {step.dialogLine?.dialogText ?? "null"}");//fix this
                    logger.Message($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}");
                    logger.Message($"isDialogBoxLocked: {step.boolValue}");
                    break;
                case CutsceneStepType.DIALOG_TRIGGER:
                    usesTarget = true;
                    logger.Message($"DialogTrigger: {step.dialogTriggerDefinition.name}");

                    logger.Message($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}");
                    break;
                case CutsceneStepType.DOUBLE_TRIGGER:
                    logger.Message($"Use puzzle focus:{step.boolValue}");

                    logger.Message($"Trigger: {step.dialogTriggerDefinition.name}");

                    logger.Message($"Response: {step.dialogTriggerDefinition.responseTrigger.name}");

                    logger.Message($"toHidden:{step.proceedBool}");
                    break;
                case CutsceneStepType.DIALOG_OPTIONS:
                    {
                        var i = 0;
                        foreach (var option in step.dialogOptions)
                        {
                            logger.Message($"DialogOption {i++}");
                            logger.IncreaseIndent();
                            LogDialogOption(option, logger);
                            logger.DecreaseIndent();
                        }
                        logger.Message($"Shuffle: {step.boolValue}");
                        break;
                    }
                case CutsceneStepType.DOLL_MOVE:
                    usesTarget = true;
                    logger.Message($"dollPositionType: {Enum.GetName(typeof(DollPositionType), step.dollPositionType)}");
                    logger.Message($"Duration: {(step.floatValue > 0 ? step.floatValue : 1f)}");
                    logger.Message($"TargetType: {Enum.GetName(typeof(CutsceneStepDollTargetType), step.dollTargetType)}");
                    switch (step.dollTargetType)
                    {
                        case CutsceneStepDollTargetType.GIRL_DEFINITION:
                            if (step.girlDefinition == null)
                            {
                                logger.Message($"Def: null");
                            }
                            else
                            {
                                logger.Message($"Def: {ModInterface.Data.GetDataId(GameDataType.Girl, step.girlDefinition.id)} - {step.girlDefinition.girlName}");
                            }
                            break;
                        case CutsceneStepDollTargetType.ORIENTATION_TYPE:
                            logger.Message($"Orientation: {Enum.GetName(typeof(DollOrientationType), step.targetDollOrientation)}");
                            break;
                        case CutsceneStepDollTargetType.FOCUSED:
                        case CutsceneStepDollTargetType.RANDOM:
                            break;
                        default:
                            logger.Message($"Unknown target type: {step.dollTargetType}");
                            break;
                    }
                    break;
                case CutsceneStepType.LOAD_GIRL:
                    usesTarget = true;
                    if (step.girlDefinition == null)
                    {
                        logger.Message("unload");
                    }
                    else
                    {
                        logger.Message($"GirlDef: {step.girlDefinition.name}");

                        if (!step.boolValue)
                        {
                            logger.Message($"Default expressions/styles");
                        }
                        else
                        {
                            logger.Message($"Expression: {step.expressionIndex}");
                            logger.Message($"Hairstyle: {step.hairstyleIndex}");
                            logger.Message($"Outfit: {step.outfitIndex}");
                        }
                    }
                    break;
                case CutsceneStepType.TOGGLE_PHONE:
                    if (step.intValue == 0)
                    {
                        logger.Message("Both Header and Lower Cellphone");
                    }
                    else
                    {
                        logger.Message(step.intValue > 0 ? "Header" : "Lower Cellphone");
                    }

                    logger.Message(step.boolValue ? "Hub position" : "Normal Position");
                    break;
                case CutsceneStepType.REWIND:
                    logger.Message($"Steps: {Mathf.Abs(step.intValue)}");
                    break;
                case CutsceneStepType.PUZZLE_GRID:
                    logger.Message(step.boolValue ? "Show" : "Hide");
                    break;
                case CutsceneStepType.BANNER_TEXT:
                    logger.Message(step.boolValue ? "Show" : "Hide");
                    if (step.boolValue)
                    {
                        logger.Message($"BannerTextPrefab: {step.bannerTextPrefab.name}");
                        logger.Message($"Effect Index: {step.intValue}");
                    }
                    break;
                case CutsceneStepType.PUZZLE_REFOCUS:
                    logger.Message($"Express Failure: {step.boolValue}");
                    break;
                case CutsceneStepType.SET_EXHAUSTION:
                    usesTarget = true;
                    logger.Message($"Exhausted: {step.boolValue}");
                    break;
                case CutsceneStepType.SUB_CUTSCENE:
                    logger.Message($"subCutsceneType: {Enum.GetName(typeof(CutsceneStepSubCutsceneType), step.subCutsceneType)}");
                    switch (step.subCutsceneType)
                    {
                        case CutsceneStepSubCutsceneType.STRAIGHT:
                            LogCutscene(step.subCutsceneDefinition);
                            break;
                        case CutsceneStepSubCutsceneType.GIRL_PAIR:
                            logger.Message($"girlPairRelationshipType: {Enum.GetName(typeof(GirlPairRelationshipType), step.girlPairRelationshipType)}");
                            break;
                    }
                    break;
                case CutsceneStepType.SHOW_WINDOW:
                    logger.Message($"Don't Queue: {step.boolValue}");
                    logger.Message($"WindowPrefab: {step.windowPrefab?.name ?? "null"}");
                    break;
                case CutsceneStepType.USE_CELLPHONE:
                    if (!StringUtils.IsEmpty(step.stringValue))
                    {
                        logger.Message($"Freeze Button Indexes: {step.stringValue}");
                    }
                    break;
                case CutsceneStepType.SHAKE_SCREEN:
                    logger.Message($"Duration: {step.floatValue}");
                    logger.Message($"Strength: {step.intValue}");
                    logger.Message($"FadeOut: {step.boolValue}");
                    break;
                case CutsceneStepType.RESET_DOLLS:
                    //no arguments
                    break;
                case CutsceneStepType.TOGGLE_OVERLAY:
                    logger.Message(step.boolValue ? "On" : "Off");
                    logger.Message($"Duration: {step.floatValue}");
                    break;
                case CutsceneStepType.SOUND_EFFECT:
                    logger.Message(step.boolValue ? "Voice" : "Sound");
                    logger.Message($"Volume: {step.audioKlip.volume}");
                    logger.Message($"Clip: {step.audioKlip.clip.name}");
                    break;
                case CutsceneStepType.CLEAR_MOOD:
                    //no arguments
                    break;
                case CutsceneStepType.PARTICLE_EMITTER:
                    logger.Message($"EmitterBehavior: {step.emitterBehavior.name}");
                    logger.Message($"Effect Container: {step.intValue}");
                    logger.Message($"Position: {step.position}");
                    break;
                case CutsceneStepType.SHOW_NOTIFICATION:
                    logger.Message($"Text: {step.stringValue}");
                    logger.Message($"NotificationType: {Enum.GetName(typeof(CutsceneStepNotificationType), step.notificationType)}");
                    logger.Message($"Show duration: {step.floatValue}");
                    break;
            }
            logger.Message($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}, Proceed Float: {step.proceedFloat}");

            if (usesTarget)
            {
                var targetStr = $"Doll Target Type: {Enum.GetName(typeof(CutsceneStepDollTargetType), step.dollTargetType)}";
                switch (step.dollTargetType)
                {
                    case CutsceneStepDollTargetType.GIRL_DEFINITION:
                        logger.Message(targetStr + $", Target Def: {step.targetGirlDefinition?.name ?? "null"} - {step.targetGirlDefinition?.girlName ?? "null"}");
                        break;
                    case CutsceneStepDollTargetType.ORIENTATION_TYPE:
                        logger.Message(targetStr + $", Orientation: {step.targetDollOrientation}");
                        break;
                    case CutsceneStepDollTargetType.RANDOM:
                        break;
                    case CutsceneStepDollTargetType.FOCUSED:
                        break;
                }
            }
        }
    }

    public static void LogCutsceneBranch(CutsceneBranchSubDefinition branch, ILogger logger)
    {
        if (branch == null)
        {
            logger.Message("null");
            return;
        }

        int i = 0;
        foreach (var condition in branch.conditions)
        {
            using (logger.MakeIndent($"Condition {i++}:"))
            {
                LogLogicCondition(condition);
            }
        }

        i = 0;
        foreach (var step in branch.cutsceneDefinition?.steps ?? branch.steps)
        {
            using (logger.MakeIndent($"Step {i++}:"))
            {
                LogCutsceneStep(step, logger);
            }
        }
    }

    public static void LogDialogOption(CutsceneDialogOptionSubDefinition dialogOption, ILogger logger)
    {
        if (dialogOption == null)
        {
            logger.Message("null");
            return;
        }

        logger.Message($"Yuri: {dialogOption.yuri}");
        if (dialogOption.yuri)
        {
            logger.Message($"Yuri Text: {dialogOption.yuriDialogOptionText}");
        }
        logger.Message($"Text: {dialogOption.dialogOptionText}");
    }
}