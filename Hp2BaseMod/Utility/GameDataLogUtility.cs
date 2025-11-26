using System;
using UnityEngine;

namespace Hp2BaseMod.Utility;

public static class GameDataLogUtility
{
    public static void LogCutscene(CutsceneDefinition cutsceneDef)
    {
        if (cutsceneDef == null)
        {
            ModInterface.Log.LogInfo("null");
            return;
        }

        ModInterface.Log.LogInfo($"Cleanup Type: {Enum.GetName(typeof(CutsceneCleanUpType), cutsceneDef.cleanUpType)}");
        int i = 0;
        foreach (var step in cutsceneDef.steps)
        {
            ModInterface.Log.LogInfo($"Step {i++}");
            ModInterface.Log.IncreaseIndent();
            LogCutsceneStep(step);
            ModInterface.Log.DecreaseIndent();
        }
    }

    public static void LogCutsceneStep(CutsceneStepSubDefinition step)
    {
        if (step == null)
        {
            ModInterface.Log.LogInfo("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Type: {Enum.GetName(typeof(CutsceneStepType), step.stepType)}"))
        {
            switch (step.stepType)
            {
                case CutsceneStepType.BRANCH:
                    {
                        int i = 0;
                        foreach (var branch in step.branches)
                        {
                            ModInterface.Log.LogInfo($"Branch {i++}:");
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
                    ModInterface.Log.LogInfo($"Special step prefab: {step.specialStepPrefab?.name ?? "null"}");
                    break;
                case CutsceneStepType.CHANGE_EXPRESSION:
                    ModInterface.Log.LogInfo($"setMood: {step.setMood}");
                    ModInterface.Log.LogInfo($"expressionType: {Enum.GetName(typeof(GirlExpressionType), step.expressionType)}");
                    if (step.setMood)
                    {
                        ModInterface.Log.LogInfo($"SetMood, EyesClosed: {step.boolValue}");
                    }
                    else
                    {
                        ModInterface.Log.LogInfo($"ChangeExpression, BreathSpeed: {step.floatValue}");
                    }
                    break;
                case CutsceneStepType.DIALOG_LINE:
                    ModInterface.Log.LogInfo($"DialogLine: {step.dialogLine?.dialogText ?? "null"}");//fix this
                    ModInterface.Log.LogInfo($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}");
                    ModInterface.Log.LogInfo($"isDialogBoxLocked: {step.boolValue}");
                    break;
                case CutsceneStepType.DIALOG_TRIGGER:
                    ModInterface.Log.LogInfo($"DialogTrigger: {step.dialogTriggerDefinition.name}");

                    ModInterface.Log.LogInfo($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}");
                    break;
                case CutsceneStepType.DOUBLE_TRIGGER:
                    ModInterface.Log.LogInfo($"Use puzzle focus:{step.boolValue}");

                    ModInterface.Log.LogInfo($"Trigger: {step.dialogTriggerDefinition.name}");

                    ModInterface.Log.LogInfo($"Response: {step.dialogTriggerDefinition.responseTrigger.name}");

                    ModInterface.Log.LogInfo($"toHidden:{step.proceedBool}");
                    break;
                case CutsceneStepType.DIALOG_OPTIONS:
                    {
                        var i = 0;
                        foreach (var option in step.dialogOptions)
                        {
                            ModInterface.Log.LogInfo($"DialogOption {i}");
                            ModInterface.Log.IncreaseIndent();
                            LogDialogOption(option);
                            ModInterface.Log.DecreaseIndent();
                        }
                        ModInterface.Log.LogInfo($"Shuffle: {step.boolValue}");
                        break;
                    }
                case CutsceneStepType.DOLL_MOVE:
                    ModInterface.Log.LogInfo($"dollPositionType: {Enum.GetName(typeof(DollPositionType), step.dollPositionType)}");
                    ModInterface.Log.LogInfo($"Duration: {(step.floatValue > 0 ? step.floatValue : 1f)}");
                    ModInterface.Log.LogInfo($"TargetType: {Enum.GetName(typeof(CutsceneStepDollTargetType), step.dollTargetType)}");
                    switch (step.dollTargetType)
                    {
                        case CutsceneStepDollTargetType.GIRL_DEFINITION:
                            if (step.girlDefinition == null)
                            {
                                ModInterface.Log.LogInfo($"Def: null");
                            }
                            else
                            {
                                ModInterface.Log.LogInfo($"Def: {ModInterface.Data.GetDataId(GameDataType.Girl, step.girlDefinition.id)} - {step.girlDefinition.girlName}");
                            }
                            break;
                        case CutsceneStepDollTargetType.ORIENTATION_TYPE:
                            ModInterface.Log.LogInfo($"Orientation: {Enum.GetName(typeof(DollOrientationType), step.targetDollOrientation)}");
                            break;
                        case CutsceneStepDollTargetType.FOCUSED:
                        case CutsceneStepDollTargetType.RANDOM:
                            break;
                        default:
                            ModInterface.Log.LogInfo($"Unknown target type: {step.dollTargetType}");
                            break;
                    }
                    break;
                case CutsceneStepType.LOAD_GIRL:
                    if (step.girlDefinition == null)
                    {
                        ModInterface.Log.LogInfo("unload");
                    }
                    else
                    {
                        ModInterface.Log.LogInfo($"GirlDef: {step.girlDefinition.name}");

                        if (step.boolValue)
                        {
                            ModInterface.Log.LogInfo($"Default expressions/styles");
                        }
                        else
                        {
                            ModInterface.Log.LogInfo($"Expression: {step.expressionIndex}");
                            ModInterface.Log.LogInfo($"Hairstyle: {step.hairstyleIndex}");
                            ModInterface.Log.LogInfo($"Outfit: {step.outfitIndex}");
                        }
                    }
                    break;
                case CutsceneStepType.TOGGLE_PHONE:
                    if (step.intValue == 0)
                    {
                        ModInterface.Log.LogInfo("Both Header and Lower Cellphone");
                    }
                    else
                    {
                        ModInterface.Log.LogInfo(step.intValue > 0 ? "Header" : "Lower Cellphone");
                    }

                    ModInterface.Log.LogInfo(step.boolValue ? "Hub position" : "Normal Position");
                    break;
                case CutsceneStepType.REWIND:
                    ModInterface.Log.LogInfo($"Steps: {Mathf.Abs(step.intValue)}");
                    break;
                case CutsceneStepType.PUZZLE_GRID:
                    ModInterface.Log.LogInfo(step.boolValue ? "Show" : "Hide");
                    break;
                case CutsceneStepType.BANNER_TEXT:
                    ModInterface.Log.LogInfo(step.boolValue ? "Show" : "Hide");
                    if (step.boolValue)
                    {
                        ModInterface.Log.LogInfo($"BannerTextPrefab: {step.bannerTextPrefab.name}");
                        ModInterface.Log.LogInfo($"Effect Index: {step.intValue}");
                    }
                    break;
                case CutsceneStepType.PUZZLE_REFOCUS:
                    ModInterface.Log.LogInfo($"Express Failure: {step.boolValue}");
                    break;
                case CutsceneStepType.SET_EXHAUSTION:
                    ModInterface.Log.LogInfo($"Exhausted: {step.boolValue}");
                    break;
                case CutsceneStepType.SUB_CUTSCENE:
                    ModInterface.Log.LogInfo($"subCutsceneType: {Enum.GetName(typeof(CutsceneStepSubCutsceneType), step.subCutsceneType)}");
                    switch (step.subCutsceneType)
                    {
                        case CutsceneStepSubCutsceneType.STRAIGHT:
                            LogCutscene(step.subCutsceneDefinition);
                            break;
                        case CutsceneStepSubCutsceneType.GIRL_PAIR:
                            ModInterface.Log.LogInfo($"girlPairRelationshipType: {Enum.GetName(typeof(GirlPairRelationshipType), step.girlPairRelationshipType)}");
                            break;
                    }
                    break;
                case CutsceneStepType.SHOW_WINDOW:
                    ModInterface.Log.LogInfo($"Don't Queue: {step.boolValue}");
                    ModInterface.Log.LogInfo($"WindowPrefab: {step.windowPrefab?.name ?? "null"}");
                    break;
                case CutsceneStepType.USE_CELLPHONE:
                    if (!StringUtils.IsEmpty(step.stringValue))
                    {
                        ModInterface.Log.LogInfo($"Freeze Button Indexes: {step.stringValue}");
                    }
                    break;
                case CutsceneStepType.SHAKE_SCREEN:
                    ModInterface.Log.LogInfo($"Duration: {step.floatValue}");
                    ModInterface.Log.LogInfo($"Strength: {step.intValue}");
                    ModInterface.Log.LogInfo($"FadeOut: {step.boolValue}");
                    break;
                case CutsceneStepType.RESET_DOLLS:
                    //no arguments
                    break;
                case CutsceneStepType.TOGGLE_OVERLAY:
                    ModInterface.Log.LogInfo(step.boolValue ? "On" : "Off");
                    ModInterface.Log.LogInfo($"Duration: {step.floatValue}");
                    break;
                case CutsceneStepType.SOUND_EFFECT:
                    ModInterface.Log.LogInfo(step.boolValue ? "Voice" : "Sound");
                    ModInterface.Log.LogInfo($"Volume: {step.audioKlip.volume}");
                    ModInterface.Log.LogInfo($"Clip: {step.audioKlip.clip.name}");
                    break;
                case CutsceneStepType.CLEAR_MOOD:
                    //no arguments
                    break;
                case CutsceneStepType.PARTICLE_EMITTER:
                    ModInterface.Log.LogInfo($"EmitterBehavior: {step.emitterBehavior.name}");
                    ModInterface.Log.LogInfo($"Effect Container: {step.intValue}");
                    ModInterface.Log.LogInfo($"Position: {step.position}");
                    break;
                case CutsceneStepType.SHOW_NOTIFICATION:
                    ModInterface.Log.LogInfo($"Text: {step.stringValue}");
                    ModInterface.Log.LogInfo($"NotificationType: {Enum.GetName(typeof(CutsceneStepNotificationType), step.notificationType)}");
                    ModInterface.Log.LogInfo($"Show duration: {step.floatValue}");
                    break;
            }
            ModInterface.Log.LogInfo($"ProceedType: {Enum.GetName(typeof(CutsceneStepProceedType), step.proceedType)}, Proceed Float: {step.proceedFloat}");
        }
    }

    public static void LogCutsceneBranch(CutsceneBranchSubDefinition branch)
    {
        if (branch == null)
        {
            ModInterface.Log.LogInfo("null");
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

    public static void LogLogicCondition(LogicCondition logicCondition)
    {
        if (logicCondition == null)
        {
            ModInterface.Log.LogInfo("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Type: {logicCondition.type}"))
        {
            switch (logicCondition.type)
            {
                case LogicConditionType.LOCATION:
                    ModInterface.Log.LogInfo($"Must be {ModInterface.Data.GetDataId(GameDataType.Location, logicCondition.locationDefinition.id)} - {logicCondition.locationDefinition.locationName}");
                    break;
                case LogicConditionType.DAYTIME:
                    ModInterface.Log.LogInfo($"Must be {logicCondition.daytimeType}");
                    break;
                case LogicConditionType.GIRL_PAIR:
                    ModInterface.Log.LogInfo($"Must be {ModInterface.Data.GetDataId(GameDataType.GirlPair, logicCondition.girlPairDefinition.id)} - {logicCondition.girlPairDefinition.name}");
                    break;
                case LogicConditionType.GIRL_ORIENTATION:
                    ModInterface.Log.LogInfo($"Doll on {logicCondition.dollOrientation} must be {ModInterface.Data.GetDataId(GameDataType.Girl, logicCondition.girlDefinition.id)} - {logicCondition.girlDefinition.name}");
                    break;
                case LogicConditionType.WING_COUNT:
                    ModInterface.Log.LogInfo($"Must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.FRUIT_COUNT:
                    ModInterface.Log.LogInfo($"{logicCondition.affectionType} fruit must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.PUZZLE_RESOURCE:
                    ModInterface.Log.LogInfo($"{logicCondition.resourceType} must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.STORY_PROGRESS:
                    ModInterface.Log.LogInfo($"must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.FLAG_VALUE:
                    ModInterface.Log.LogInfo($"{logicCondition.stringValue} must be {logicCondition.comparisonType} {logicCondition.intValue}");
                    break;
                case LogicConditionType.IS_TUTORIAL:
                    ModInterface.Log.LogInfo($"Must be {logicCondition.boolValue}");
                    break;
                case LogicConditionType.DIFFICULTY:
                    ModInterface.Log.LogInfo($"Must be {logicCondition.settingDifficulty}");
                    break;
                case LogicConditionType.DATE_TYPE:
                    ModInterface.Log.LogInfo($"Must be {logicCondition.dateType}");
                    break;
            }
        }
    }

    public static void LogLogicAction(LogicAction logicAction)
    {
        if (logicAction == null)
        {
            ModInterface.Log.LogInfo("null");
            return;
        }

        using (ModInterface.Log.MakeIndent($"Type: {logicAction.type}"))
        {
            switch (logicAction.type)
            {
                case LogicActionType.TRAVEL_TO:
                    ModInterface.Log.LogInfo($"Time passing amount: {logicAction.intValue}");
                    ModInterface.Log.LogInfo($"Destination location definition: {ModInterface.Data.GetDataId(GameDataType.Location, logicAction.locationDefinition.id)} - {logicAction.locationDefinition.locationName}");
                    var pairString = logicAction.girlPairDefinition == null
                        ? "null"
                        : $"{ModInterface.Data.GetDataId(GameDataType.GirlPair, logicAction.girlPairDefinition.id)} - {logicAction.girlPairDefinition.name}";
                    ModInterface.Log.LogInfo($"Pair at loc {pairString}");
                    ModInterface.Log.LogInfo($"Pair sides flipped: {logicAction.boolValue}");
                    break;
                case LogicActionType.ADD_WINGS:
                    ModInterface.Log.LogInfo($"This action just doesn't do anything! Odd...");
                    break;
                case LogicActionType.ADD_FRUIT:
                    ModInterface.Log.LogInfo($"Fruit type: {logicAction.affectionType}");
                    ModInterface.Log.LogInfo($"Fruit count: {logicAction.intValue}");
                    break;
                case LogicActionType.ADD_PUZZLE_RESOURCE:
                    ModInterface.Log.LogInfo($"Resource type: {logicAction.resourceType}");
                    ModInterface.Log.LogInfo($"Resource amount: {logicAction.intValue}");
                    ModInterface.Log.LogInfo($"Give to alt girl: {logicAction.boolValue}");
                    break;
                case LogicActionType.SET_STORY_PROGRESS:
                    ModInterface.Log.LogInfo($"Story Progress value: {logicAction.intValue}");
                    break;
                case LogicActionType.SET_FLAG:
                    ModInterface.Log.LogInfo($"Flag name: {logicAction.stringValue}");
                    ModInterface.Log.LogInfo($"Flag value: {logicAction.intValue}");
                    break;
                case LogicActionType.START_CUTSCENE:
                    ModInterface.Log.LogInfo($"Cutscene id: {ModInterface.Data.GetDataId(GameDataType.Cutscene, logicAction.cutsceneDefinition.id)}");
                    break;
                case LogicActionType.SET_GIRL_FOCUS:
                    ModInterface.Log.LogInfo($"Focus alt girl: {logicAction.boolValue}");
                    break;
                case LogicActionType.RESET_DOLLS:
                    ModInterface.Log.LogInfo($"Unload: {logicAction.boolValue}");
                    break;
                case LogicActionType.DIALOG_OPTION_FLAG:
                    ModInterface.Log.LogInfo($"Flag gets set the the current 'Game.Session.Dialog.selectedDialogOptionIndex'");
                    ModInterface.Log.LogInfo($"Flag name: {logicAction.stringValue}");
                    break;
                case LogicActionType.CHANGE_PUZZLE_STATE:
                    ModInterface.Log.LogInfo($"Puzzle state: {logicAction.puzzleState}");
                    break;
                case LogicActionType.POP_DATE_GIFT:
                    ModInterface.Log.LogInfo($"Target alt girl: {logicAction.boolValue}");
                    ModInterface.Log.LogInfo($"Item to populate Id: {ModInterface.Data.GetDataId(GameDataType.Item, logicAction.itemDefinition.id)}");
                    break;
                case LogicActionType.SET_GIRL_MET:
                    ModInterface.Log.LogInfo($"Girl id: {ModInterface.Data.GetDataId(GameDataType.Girl, logicAction.girlDefinition.id)}");
                    ModInterface.Log.LogInfo($"Set value to: {logicAction.boolValue}");
                    break;
                case LogicActionType.ADD_INVENTORY_ITEM:
                    ModInterface.Log.LogInfo($"Item Id: {ModInterface.Data.GetDataId(GameDataType.Item, logicAction.itemDefinition.id)}");
                    break;
                case LogicActionType.BACKGROUND_MUSIC:
                    ModInterface.Log.LogInfo($"Background Music: {logicAction.backgroundMusic?.clip?.name ?? "null"}");
                    break;
                case LogicActionType.UI_TAG_EVENT:
                    ModInterface.Log.LogInfo($"Name: {logicAction.stringValue}");
                    break;
            }
        }
    }

    public static void LogDialogOption(CutsceneDialogOptionSubDefinition dialogOption)
    {
        if (dialogOption == null)
        {
            ModInterface.Log.LogInfo("null");
            return;
        }

        ModInterface.Log.LogInfo($"Yuri: {dialogOption.yuri}");
        if (dialogOption.yuri)
        {
            ModInterface.Log.LogInfo($"Yuri Text: {dialogOption.yuriDialogOptionText}");
        }
        ModInterface.Log.LogInfo($"Text: {dialogOption.yuriDialogOptionText}");
        ModInterface.Log.LogInfo($"Text: {dialogOption.yuriDialogOptionText}");
    }

    public static void LogLogicBundle(LogicBundle logicBundle)
    {
        using (ModInterface.Log.MakeIndent("Conditions"))
        {
            foreach (var condition in logicBundle.conditions)
            {
                LogLogicCondition(condition);
            }
        }

        using (ModInterface.Log.MakeIndent("Actions"))
        {
            foreach (var condition in logicBundle.actions)
            {
                LogLogicAction(condition);
            }
        }
    }
}