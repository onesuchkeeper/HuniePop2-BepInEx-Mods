using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AssetStudio;
using AssetStudio.Extractor;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;
using Hp2BaseMod.GameDataInfo.Interface;
using Hp2BaseMod.Utility;

namespace HuniePopUltimate;

/// <summary>
/// Builds <see cref="CutsceneDataMod"/> trees from HP1 DialogScene assets.
/// Depends on <see cref="HpDialogExtractor"/> for individual line extraction.
/// The meeting cutscene style sequence for each girl is sourced from
/// <see cref="IGirlConfigurator.MeetingCutsceneStyleSequence"/> rather than
/// a hardcoded lookup table.
/// </summary>
public class HpCutsceneExtractor
{
    private readonly HpDialogExtractor _dialog;
    private readonly IReadOnlyDictionary<int, IGirlConfigurator> _configs;
    private readonly Extractor _extractor;

    public HpCutsceneExtractor(HpDialogExtractor dialog, IReadOnlyDictionary<int, IGirlConfigurator> configs, Extractor extractor)
    {
        _dialog = dialog;
        _configs = configs;
        _extractor = extractor;
    }

    public void ExtractIntroCutscene(RelativeId girlId, OrderedDictionary girlDef, SerializedFile file, SingleDatePairData pair)
    {
        if (girlDef.TryGetValue("introScene", out OrderedDictionary introScene)
            && UnityAssetPath.TryExtract(introScene, out var introScenePath)
            && _extractor.TryExtractMonoBehavior(file, introScenePath, out var introSceneDef)
            && TryExtractDialogScene(introSceneDef, file, girlId, out var introSceneMod))
        {
            pair.MeetingCutscene = introSceneMod;
        }
        else
        {
            ModInterface.Log.Warning("failed to extract intro cutscene");
        }
    }

    private bool TryExtractDialogScene(OrderedDictionary dialogSceneDef, SerializedFile file, RelativeId girlId, out CutsceneDataMod cutsceneMod)
    {
        if (!dialogSceneDef.TryGetValue("steps", out List<object> steps))
        {
            ModInterface.Log.Warning("Failed to extract dialog scene");
            cutsceneMod = null;
            return false;
        }

        cutsceneMod = new(new RelativeId(Plugin.ModId, Cutscenes.NextCutsceneId++), InsertStyle.append);
        cutsceneMod.Steps = new();

        var girlConfig = _configs.Values.FirstOrDefault(c => c.Mod.Id == girlId);
        var styleSequence = girlConfig?.MeetingCutsceneStyleSequence
            ?? System.Array.Empty<(RelativeId, RelativeId)>();
        var styleEnum = ((IEnumerable<(RelativeId outfit, RelativeId hairstyle)>)styleSequence).GetEnumerator();

        UnityAssetPath currentAltGirl = UnityAssetPath.NullPath;
        foreach (var step in steps.OfType<OrderedDictionary>())
        {
            if (TryExtractDialogSceneStep(step, file, girlId, styleEnum, ref currentAltGirl, out var stepMods))
            {
                cutsceneMod.Steps.AddRange(stepMods);
            }
        }

        return true;
    }

    private bool TryExtractDialogSceneStep(
        OrderedDictionary dialogSceneStep,
        SerializedFile file,
        RelativeId girlId,
        IEnumerator<(RelativeId outfit, RelativeId hairstyle)> styleEnum,
        ref UnityAssetPath altGirlId,
        out CutsceneStepInfo[] stepMods)
    {
        stepMods = null;

        if (!dialogSceneStep.TryGetValue("type", out int type))
        {
            return false;
        }

        switch (type)
        {
            case 0: // dialog line
            {
                if (dialogSceneStep.TryGetValue("sceneLine", out OrderedDictionary sceneLineDef)
                    && sceneLineDef.TryGetValue("altGirl", out bool altGirl)
                    && sceneLineDef.TryGetValue("dialogLine", out OrderedDictionary dialogLine)
                    && _dialog.TryExtractDialogLine(dialogLine, file, _dialog.NextDialogLineId(), out var lineMod))
                {
                    var stepMod = CutsceneStepUtility.MakeDialogLineInfo(lineMod, false, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.ORIENTATION_TYPE);
                    stepMod.TargetDollOrientation = altGirl ? DollOrientationType.LEFT : DollOrientationType.RIGHT;
                    stepMods = [stepMod];
                    return true;
                }
                else
                {
                    ModInterface.Log.Warning("failed to extract \"dialog line\" dialog step");
                }
                break;
            }
            case 1: // response options
            {
                if (dialogSceneStep.TryGetValue("preventOptionShuffle", out bool preventOptionShuffle)
                    && dialogSceneStep.TryGetValue("responseOptions", out List<object> responseOptions))
                {
                    var options = new List<IGameDefinitionInfo<CutsceneDialogOptionSubDefinition>>();
                    foreach (var responseOption in responseOptions.OfType<OrderedDictionary>())
                    {
                        if (TryExtractResponseOption(responseOption, file, girlId, styleEnum, ref altGirlId, out var dialogOption))
                        {
                            options.Add(dialogOption);
                        }
                    }
                    stepMods = [CutsceneStepUtility.MakeDialogOptionsInfo(options, !preventOptionShuffle, CutsceneStepProceedType.AUTOMATIC)];
                    return true;
                }
                else
                {
                    ModInterface.Log.Warning("failed to extract \"response options\" dialog step");
                }
                break;
            }
            case 2: // branch dialog
            {
                if (dialogSceneStep.TryGetValue("hasBestBranch", out bool hasBestBranch)
                    && dialogSceneStep.TryGetValue("conditionalBranchs", out List<object> conditionalBranches))
                {
                    var branches = new List<IGameDefinitionInfo<CutsceneBranchSubDefinition>>();
                    foreach (var branch in conditionalBranches.OfType<OrderedDictionary>())
                    {
                        if (TryExtractConditionalBranch(branch, file, girlId, styleEnum, ref altGirlId, out var branchMod))
                        {
                            branches.Add(branchMod);
                        }
                    }
                    stepMods = [CutsceneStepUtility.MakeBranchInfo(branches, CutsceneStepProceedType.AUTOMATIC)];
                    return true;
                }
                else
                {
                    ModInterface.Log.Warning("failed to extract \"branch dialog\" dialog step");
                }
                break;
            }
            case 3: // show alt girl
            {
                if (dialogSceneStep.TryGetValue("altGirl", out OrderedDictionary altGirl)
                    && UnityAssetPath.TryExtract(altGirl, out var altGirlPath)
                    && dialogSceneStep.TryGetValue("showGirlStyles", out string showGirlStyles)
                    && dialogSceneStep.TryGetValue("hideOppositeSpeechBubble", out bool hideOppositeSpeechBubble))
                {
                    var initialShow = altGirlId == UnityAssetPath.NullPath;
                    var stepList = new List<CutsceneStepInfo>();

                    if (altGirlPath != altGirlId)
                    {
                        altGirlId = altGirlPath;
                        stepList.Add(CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.HIDDEN, DollOrientationType.LEFT, initialShow ? CutsceneStepProceedType.INSTANT : CutsceneStepProceedType.AUTOMATIC));
                        var loadStep = CutsceneStepUtility.MakeLoadGirlInfo(Girls.FromUnityPath(altGirlId), CutsceneStepDollTargetType.ORIENTATION_TYPE, CutsceneStepProceedType.AUTOMATIC);
                        loadStep.TargetDollOrientation = DollOrientationType.LEFT;
                        stepList.Add(loadStep);

                        if (!string.IsNullOrWhiteSpace(showGirlStyles) && styleEnum.MoveNext())
                        {
                            loadStep.HairstyleId = styleEnum.Current.hairstyle;
                            loadStep.OutfitId = styleEnum.Current.outfit;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(showGirlStyles))
                    {
                        var loadStep = CutsceneStepUtility.MakeLoadGirlInfo(Girls.FromUnityPath(altGirlId), CutsceneStepDollTargetType.ORIENTATION_TYPE, CutsceneStepProceedType.AUTOMATIC);
                        loadStep.TargetDollOrientation = DollOrientationType.LEFT;
                        stepList.Add(loadStep);

                        if (styleEnum.MoveNext())
                        {
                            loadStep.HairstyleId = styleEnum.Current.hairstyle;
                            loadStep.OutfitId = styleEnum.Current.outfit;
                        }
                    }

                    stepList.Add(CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.INNER, DollOrientationType.LEFT, CutsceneStepProceedType.AUTOMATIC));
                    stepMods = stepList.ToArray();
                    return true;
                }
                else
                {
                    ModInterface.Log.Warning("failed to extract \"show alt girl\" dialog step");
                }
                break;
            }
            case 4: // hide alt girl
            {
                stepMods = [CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.HIDDEN, DollOrientationType.LEFT, CutsceneStepProceedType.AUTOMATIC)];
                return true;
            }
            case 5: // show girl
            {
                if (dialogSceneStep.TryGetValue("showGirlStyles", out string showGirlStyles)
                    && dialogSceneStep.TryGetValue("hideOppositeSpeechBubble", out bool hideOppositeSpeechBubble))
                {
                    var stepList = new List<CutsceneStepInfo>();

                    if (!string.IsNullOrWhiteSpace(showGirlStyles))
                    {
                        var loadStep = CutsceneStepUtility.MakeLoadGirlInfo(girlId, CutsceneStepDollTargetType.ORIENTATION_TYPE, CutsceneStepProceedType.AUTOMATIC);
                        loadStep.TargetDollOrientation = DollOrientationType.RIGHT;
                        stepList.Add(loadStep);

                        if (styleEnum.MoveNext())
                        {
                            loadStep.HairstyleId = styleEnum.Current.hairstyle;
                            loadStep.OutfitId = styleEnum.Current.outfit;
                        }
                    }

                    var initialShow = altGirlId == UnityAssetPath.NullPath;
                    stepList.Add(CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.INNER, DollOrientationType.RIGHT, initialShow ? CutsceneStepProceedType.INSTANT : CutsceneStepProceedType.INSTANT));
                    stepMods = stepList.ToArray();
                    return true;
                }
                else
                {
                    ModInterface.Log.Warning("failed to extract \"show girl\" dialog step");
                }
                break;
            }
            case 6: // hide girl
            {
                stepMods = [CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.HIDDEN, DollOrientationType.RIGHT, CutsceneStepProceedType.AUTOMATIC)];
                return true;
            }
            case 7:  ModInterface.Log.Warning("\"insert scene\" dialog step unimplemented"); break;
            case 8: // wait
            {
                if (dialogSceneStep.TryGetValue("waitTime", out int waitTime))
                {
                    stepMods = [CutsceneStepUtility.MakeWaitInfo(waitTime)];
                    return true;
                }
                else
                {
                    ModInterface.Log.Warning("failed to extract \"wait\" dialog step");
                }
                break;
            }
            case 9:  ModInterface.Log.Warning("\"set next location\" dialog step unimplemented"); break;
            case 10: // set met status
            {
                if (dialogSceneStep.TryGetValue("girlDefinition", out OrderedDictionary girlDefinition)
                    && UnityAssetPath.TryExtract(girlDefinition, out var girlDefUap))
                {
                    var metGirlId = Girls.FromUnityPath(girlDefUap);
                    var showNotif = CutsceneStepUtility.MakeShowNotificationInfo(
                        $"{Girls.IdToName(metGirlId)} has been added to the HunieBee!",
                        CutsceneStepNotificationType.MESSAGE,
                        2f,
                        CutsceneStepDollTargetType.ORIENTATION_TYPE,
                        CutsceneStepProceedType.INSTANT);
                    showNotif.TargetDollOrientation = DollOrientationType.MIDDLE;

                    stepMods = [
                        CutsceneStepUtility.MakeGameActionInfo(
                            new LogicActionInfo()
                            {
                                Type = LogicActionType.SET_GIRL_MET,
                                GirlDefinitionID = Girls.FromUnityPath(girlDefUap),
                                BoolValue = true
                            },
                            CutsceneStepProceedType.INSTANT),
                        showNotif
                    ];
                    return true;
                }
                break;
            }
            case 11: ModInterface.Log.Warning("\"dialog trigger\" dialog step unimplemented"); break;
            case 12: ModInterface.Log.Warning("\"know girl detail\" dialog step unimplemented"); break;
            case 13: // step back
            {
                if (dialogSceneStep.TryGetValue("stepBackSteps", out int stepBackSteps))
                {
                    stepMods = [CutsceneStepUtility.MakeRewindInfo(stepBackSteps, CutsceneStepProceedType.AUTOMATIC)];
                    return true;
                }
                break;
            }
            case 14: ModInterface.Log.Warning("\"add item\" dialog step unimplemented"); break;
            case 15: ModInterface.Log.Warning("\"remove item\" dialog step unimplemented"); break;
            case 16: ModInterface.Log.Warning("\"wait for cellphone close\" dialog step unimplemented"); break;
            case 17: ModInterface.Log.Warning("\"wait for token match\" dialog step unimplemented"); break;
            case 18: ModInterface.Log.Warning("\"wait for date gift\" dialog step unimplemented"); break;
            case 19: ModInterface.Log.Warning("\"unlock cellphone\" dialog step unimplemented"); break;
            case 20: ModInterface.Log.Warning("\"make game pausable\" dialog step unimplemented"); break;
            case 21: ModInterface.Log.Warning("\"particle emitter\" dialog step unimplemented"); break;
            case 22: ModInterface.Log.Warning("\"send message\" dialog step unimplemented"); break;
        }

        return false;
    }

    private bool TryExtractResponseOption(
        OrderedDictionary responseOption,
        SerializedFile file,
        RelativeId girlId,
        IEnumerator<(RelativeId outfit, RelativeId hairstyle)> styleEnum,
        ref UnityAssetPath altGirlId,
        out CutsceneDialogOptionInfo dialogOption)
    {
        if (responseOption.TryGetValue("text", out string text)
            && responseOption.TryGetValue("secondary", out bool secondary)
            && responseOption.TryGetValue("secondaryText", out string secondaryText)
            && responseOption.TryGetValue("steps", out List<object> steps))
        {
            dialogOption = new();
            dialogOption.DialogOptionText = text;
            dialogOption.Yuri = secondary;
            dialogOption.YuriDialogOptionText = secondaryText;
            dialogOption.Steps = new();

            foreach (var step in steps.OfType<OrderedDictionary>())
            {
                if (TryExtractDialogSceneStep(step, file, girlId, styleEnum, ref altGirlId, out var stepMods))
                {
                    dialogOption.Steps.AddRange(stepMods);
                }
            }

            return true;
        }

        dialogOption = null;
        return false;
    }

    private bool TryExtractConditionalBranch(
        OrderedDictionary branch,
        SerializedFile file,
        RelativeId girlId,
        IEnumerator<(RelativeId outfit, RelativeId hairstyle)> styleEnum,
        ref UnityAssetPath altGirl,
        out CutsceneBranchInfo branchMod)
    {
        if (!branch.TryGetValue("type", out int type)
            || !branch.TryGetValue("steps", out List<object> steps))
        {
            branchMod = null;
            return false;
        }

        branchMod = new();

        // Conditions are ignored for now — none of the extracted cutscenes need them
        // and HP1 conditions don't map cleanly to HP2.
        switch (type)
        {
            case 0: // else — always true
                break;
            case 1: // girl met status count
                break;
            case 2: // girl detail known
                break;
            default:
                ModInterface.Log.Warning($"unhandled conditional branch type {type}");
                branchMod = null;
                return false;
        }

        branchMod.Steps = new();
        foreach (var step in steps.OfType<OrderedDictionary>())
        {
            if (TryExtractDialogSceneStep(step, file, girlId, styleEnum, ref altGirl, out var stepMods))
            {
                branchMod.Steps.AddRange(stepMods);
            }
        }

        return true;
    }
}