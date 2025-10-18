using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.GameDataInfo;
using UnityEngine;

namespace Hp2BaseMod.Utility;

public static class CutsceneStepUtility
{
    public static CutsceneStepSubDefinition MakeNothing(CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.NOTHING,
            proceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeBranch(IEnumerable<CutsceneBranchSubDefinition> branches, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.BRANCH,
            branches = branches.ToList(),
            proceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeGameAction(LogicAction logicAction, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.GAME_ACTION,
            logicAction = logicAction,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeSpecialStep(CutsceneStepSpecial cutsceneStepSpecial, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SPECIAL_STEP,
            specialStepPrefab = cutsceneStepSpecial,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeChangeExpression(GirlExpressionType girlExpressionType, float breathSpeed, CutsceneStepDollTargetType dollTargetType, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.CHANGE_EXPRESSION,
            floatValue = breathSpeed,
            expressionType = girlExpressionType,
            setMood = false,
            dollTargetType = dollTargetType,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeSetMood(GirlExpressionType girlExpressionType, bool eyesClosed, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.CHANGE_EXPRESSION,
            boolValue = eyesClosed,
            expressionType = girlExpressionType,
            setMood = true,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeDialogLine(DialogLine dialogLine, bool isDialogBoxLocked, CutsceneStepProceedType proceedType, CutsceneStepDollTargetType dollTargetType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DIALOG_LINE,
            boolValue = isDialogBoxLocked,
            proceedType = proceedType,
            dialogLine = dialogLine,
            dollTargetType = dollTargetType
        };
    }
    public static CutsceneStepSubDefinition MakeDialogTrigger(DialogTriggerDefinition dialogTriggerDefinition, CutsceneStepProceedType proceedType, CutsceneStepDollTargetType dollTargetType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DIALOG_TRIGGER,
            dialogTriggerDefinition = dialogTriggerDefinition,
            proceedType = proceedType,
            dollTargetType = dollTargetType
        };
    }
    public static CutsceneStepSubDefinition MakeDoubleTrigger(bool usePuzzleFocus, DialogTriggerDefinition dialogTrigger, bool toHidden, CutsceneStepProceedType proceedType)
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
    public static CutsceneStepSubDefinition MakeDialogOptions(IEnumerable<CutsceneDialogOptionSubDefinition> options, bool shuffle, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DIALOG_OPTIONS,
            dialogOptions = options.ToList(),
            boolValue = shuffle,
            proceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeDollMove(DollPositionType positionType, CutsceneStepDollTargetType dollTargetType, CutsceneStepProceedType proceedType, float duration = 1f)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DOLL_MOVE,
            dollPositionType = positionType,
            floatValue = duration,
            dollTargetType = dollTargetType,
            proceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeDollMove(DollPositionType positionType, GirlDefinition targetDef, CutsceneStepProceedType proceedType, float duration = 1f)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DOLL_MOVE,
            dollPositionType = positionType,
            floatValue = duration,
            dollTargetType = CutsceneStepDollTargetType.GIRL_DEFINITION,
            targetGirlDefinition = targetDef,
            proceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeDollMove(DollPositionType positionType, DollOrientationType targetOrientation, CutsceneStepProceedType proceedType, float duration = 1f)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.DOLL_MOVE,
            dollPositionType = positionType,
            floatValue = duration,
            dollTargetType = CutsceneStepDollTargetType.ORIENTATION_TYPE,
            targetDollOrientation = targetOrientation,
            proceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeLoadGirl(GirlDefinition girlDefinition, CutsceneStepDollTargetType dollTargetType, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.LOAD_GIRL,
            girlDefinition = girlDefinition,
            boolValue = true,
            dollTargetType = dollTargetType,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeLoadGirl(GirlDefinition girlDefinition, int expression, int hairstyle, int outfit, CutsceneStepDollTargetType dollTargetType, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.LOAD_GIRL,
            girlDefinition = girlDefinition,
            boolValue = false,
            expressionIndex = expression,
            hairstyleIndex = hairstyle,
            outfitIndex = outfit,
            dollTargetType = dollTargetType,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeUnloadGirl(CutsceneStepDollTargetType dollTargetType, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.LOAD_GIRL,
            dollTargetType = dollTargetType,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeTogglePhone(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.TOGGLE_PHONE,
            intValue = -1,
            boolValue = leftPosition_centerPosition,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeToggleHeader(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.TOGGLE_PHONE,
            intValue = 1,
            boolValue = leftPosition_centerPosition,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeTogglePhoneAndHeader(bool leftPosition_centerPosition, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.TOGGLE_PHONE,
            intValue = 0,
            boolValue = leftPosition_centerPosition,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeRewind(int steps, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.REWIND,
            intValue = steps,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeShowPuzzleGrid(BannerTextBehavior banner, int effectIndex, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.PUZZLE_GRID,
            boolValue = true,
            bannerTextPrefab = banner,
            intValue = effectIndex,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeHidePuzzleGrid(CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.PUZZLE_GRID,
            boolValue = false,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakePuzzleRefocus(bool expressFailure, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.PUZZLE_REFOCUS,
            boolValue = expressFailure,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeSetExhaustion(bool exhausted, CutsceneStepDollTargetType dollTargetType, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SET_EXHAUSTION,
            boolValue = exhausted,
            dollTargetType = dollTargetType,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeSubCutsceneStraight(CutsceneDefinition cutsceneDefinition, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SUB_CUTSCENE,
            subCutsceneType = CutsceneStepSubCutsceneType.STRAIGHT,
            subCutsceneDefinition = cutsceneDefinition,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeSubCutsceneGirlPair(GirlPairRelationshipType relationshipType, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SUB_CUTSCENE,
            subCutsceneType = CutsceneStepSubCutsceneType.GIRL_PAIR,
            girlPairRelationshipType = relationshipType,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeSubCutsceneGirlPairRound(CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SUB_CUTSCENE,
            subCutsceneType = CutsceneStepSubCutsceneType.GIRL_PAIR_ROUND,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeSubCutsceneInner(CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SUB_CUTSCENE,
            subCutsceneType = CutsceneStepSubCutsceneType.INNER,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeShowWindow(bool queue, UiWindow windowPrefab, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SHOW_WINDOW,
            boolValue = !queue,
            windowPrefab = windowPrefab,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeUseCellphone(IEnumerable<int> freezeButtonIndexes, CutsceneStepProceedType proceedType)
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
    public static CutsceneStepSubDefinition MakeShakeScreen(float duration, int strength, bool fadeOut, CutsceneStepProceedType proceedType)
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
    public static CutsceneStepSubDefinition MakeResetDolls(CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.RESET_DOLLS,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeToggleOverlay(bool off_on, float duration, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.TOGGLE_OVERLAY,
            boolValue = off_on,
            floatValue = duration,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeSoundEffect(bool sound_voice, AudioKlip audioKlip, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SOUND_EFFECT,
            boolValue = sound_voice,
            audioKlip = audioKlip,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeClearMood(CutsceneStepDollTargetType dollTargetType, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.CLEAR_MOOD,
            dollTargetType = dollTargetType,
            proceedType = proceedType
        };
    }
    public static CutsceneStepSubDefinition MakeParticleEmitter(EmitterBehavior emitter, int effectContainer, Vector2 position, CutsceneStepProceedType proceedType)
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
    public static CutsceneStepSubDefinition MakeShowNotification(string text, CutsceneStepNotificationType type, float duration, CutsceneStepDollTargetType dollTargetType, CutsceneStepProceedType proceedType)
    {
        return new CutsceneStepSubDefinition()
        {
            stepType = CutsceneStepType.SHOW_NOTIFICATION,
            stringValue = text,
            notificationType = type,
            floatValue = duration,
            dollTargetType = dollTargetType,
            proceedType = proceedType
        };
    }

    public static CutsceneStepSubDefinition MakeBannerTextHide()
    {
        return new CutsceneStepSubDefinition()
        {
            boolValue = true
        };
    }

    public static CutsceneStepSubDefinition MakeBannerText(BannerTextBehavior bannerTextBehavior, int effectsContainerIndex)
    {
        return new CutsceneStepSubDefinition()
        {
            bannerTextPrefab = bannerTextBehavior,
            intValue = effectsContainerIndex,
            boolValue = false
        };
    }
}