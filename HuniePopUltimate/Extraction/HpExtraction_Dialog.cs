using System;
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

public partial class HpExtraction
{
    // cutscene outfit changes are done by the part's index, and I don't wanna deal with that
    // so for the intro cutscenes this looks up their intended style sequences
    private Dictionary<RelativeId, (RelativeId outfit, RelativeId hairstyle)[]> _hackyStyleLookup = new()
    {
        {Girls.Aiko, [(Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity), (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)]},
        {Girls.Audrey, [(Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity), (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)]},
        {Girls.Beli, [(Hp2BaseMod.Styles.Water, Hp2BaseMod.Styles.Water), (Hp2BaseMod.Styles.Water, Hp2BaseMod.Styles.Water), (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)]},
        {Girls.Celeste, []},
        {Hp2BaseMod.Girls.Jessie, [(Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity), (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)]},
        {Girls.Kyanna, [(Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity), (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)]},
        {Hp2BaseMod.Girls.Kyu, []},
        {Hp2BaseMod.Girls.Lola, [(Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity), (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)]},
        {Girls.Momo, []},
        {Girls.Nikki, []},
        {Girls.Tiffany, [(Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity), (Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)]},
        {Girls.Venus, [(Hp2BaseMod.Styles.Activity, Hp2BaseMod.Styles.Activity)]},
    };

    private void ExtractDialogTrigger(OrderedDictionary dtDef, SerializedFile file)
    {
        if (!dtDef.TryGetValue("lineSets", out List<object> lineSetsList))
        {
            return;
        }

        var lineSets = lineSetsList.OfType<OrderedDictionary>().ToArray();

        if (dtDef.TryGetValue("m_Name", out string name))
        {
            switch (name)
            {
                case "AskDate":
                    if (lineSets.Length > 0)
                    {
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.AskDate, lineSets[0], file);
                    }
                    break;
                case "DateGreeting":
                    if (lineSets.Length > 11)
                    {
                        //0-11 - date locations
                        //12 - bonus round loc (use for cutscene instead)
                        for (int i = 0; i < 12; i++)// TODO map these properly to the locations
                        {
                            ExtractLocGreetingDialogLineSet(LocationIds.FromGreetingDtIndex(i), lineSets[i], file);
                        }
                        ExtractDialogLineSet(DialogTriggers.PreSex, lineSets[12], file);
                    }
                    break;
                case "DateValediction":
                    if (lineSets.Length > 1)
                    {
                        //0 - success
                        //1 - fail
                        //2 - goToBonus round - I'll have to repurpose this for cutscenes
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateSuccess, lineSets[0], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateFailure, lineSets[1], file);
                        ExtractDialogLineSet(DialogTriggers.PreBedroom, lineSets[2], file);
                    }
                    break;
                case "GivenDateGift":
                    if (lineSets.Length > 1)
                    {
                        //0 - success
                        //1 - fail
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateGiftAccept, lineSets[0], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateGiftSexy, lineSets[0], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateGiftWeird, lineSets[0], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateGiftReject, lineSets[1], file);
                    }
                    break;
                case "GivenDrink":
                    if (lineSets.Length > 3)
                    {
                        //0 - inebriation == 12, drunk
                        //1 - appetite == 0, too hungry
                        //2 - not at drinking location
                        //3 - default
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SmoothieAccept, lineSets[3], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SmoothieFull, lineSets[1], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SmoothieReject, lineSets[2], file);
                    }
                    break;
                case "GivenFood":
                    if (lineSets.Length > 3)
                    {
                        //0 - appetite == 12, full
                        //1 - loves
                        //2 - likes
                        //3 - default
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.FoodAccept, lineSets[2], file);//food accept
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.FoodReject, lineSets[3], file);//food reject
                    }
                    break;
                case "GivenGift":
                    if (lineSets.Length > 4)
                    {
                        //0 - loves
                        //1 - likes type 1
                        //2 - like type 2
                        //3 - unique gift type
                        //4 - default? maybe reject?
                        //5 - dirty magazine given to kyu

                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.UniqueAccept, lineSets[3], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.UniqueReject, lineSets[4], file);

                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.ShoesAccept, lineSets[1], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.ShoesReject, lineSets[4], file);

                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.ShellAccept, lineSets[2], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.ShellReject, lineSets[4], file);
                    }
                    break;
                case "Greeting":
                    if (lineSets.Length > 3)
                    {
                        // 0 - morning
                        // 1 - afternoon
                        // 2 - evening
                        // 3 - night
                        // 4 - post sex (use this in cutscene)
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.GreetingMorning, lineSets[0], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.GreetingAfternoon, lineSets[1], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.GreetingEvening, lineSets[2], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.GreetingNight, lineSets[3], file);
                        ExtractDialogLineSet(DialogTriggers.PostSex, lineSets[4], file);
                    }
                    break;
                case "InventoryFull":
                    if (lineSets.Length > 0)
                    {
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.InventoryFull, lineSets[0], file);
                    }
                    break;
                case "IsTooHungry":
                    if (lineSets.Length > 0)
                    {
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.StaminaInsufficient, lineSets[0], file);
                    }
                    break;
                case "KyuSpecial":
                    //13 - given tissue box
                    //item.id - 277 is for panties given to kyu, that would probably be a whole different dt if I were to add them
                    //12 - given all panties, activate alpha mode
                    break;
                case "MatchToken":
                    if (lineSets.Length > 1)
                    {
                        //0 - big move
                        //1 - broken heart
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.BigMove, lineSets[0], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.BrokenExhausted, lineSets[1], file);
                    }
                    break;
                case "SexualSounds":
                    if (lineSets.Length > 4)
                    {
                        // 0 - 3 - sex moans
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexMoans1, lineSets[0], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexMoans2, lineSets[1], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexMoans3, lineSets[2], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexMoans4, lineSets[3], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexClimax, lineSets[4], file);
                    }
                    break;
                case "Valediction"://leave without dating
                    if (lineSets.Length > 3)
                    {
                        // 0 morning
                        // 1 afternoon
                        // 2 evening
                        // 3 night
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.Valediction, lineSets[0], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.Valediction, lineSets[1], file);
                        ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.Valediction, lineSets[2], file);
                    }
                    break;
                //I think queries are the player asking about a trait
                case "QueryDuplicate":
                    break;
                case "QueryIntro":
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.FavQuestionIntro, lineSets[0], file);
                    break;
                //I think questions are the abstract general questions unique to each
                case "QuestionBadChoice":
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.HerQuestionBadResponse, lineSets[0], file);
                    break;
                case "QuestionCorrect":
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.HerQuestionGoodResponse, lineSets[0], file);
                    break;
                case "QuestionIncorrect":
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.HerQuestionBadResponse, lineSets[0], file);
                    break;
                case "QuestionIntro":
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.HerQuestionIntro, lineSets[0], file);
                    break;
                case "QuizCorrect":
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.FavQuestionAgreement, lineSets[0], file);
                    break;
                case "QuizIncorrect":
                    break;
                case "QuizIntro":
                    break;
                default:
                    ModInterface.Log.Error($"Unhandled hp1 dt {name}");
                    break;
            }
        }
    }

    private void ExtractLocGreetingDialogLineSet(RelativeId locId, OrderedDictionary lineSetDef, SerializedFile file)
    {
        // dtLineSets have a collection of lines. A line can be specified or is chosen at random
        if (!lineSetDef.TryGetValue("lines", out List<object> lines))
        {
            return;
        }

        foreach (var line in lines.OfType<OrderedDictionary>())
        {
            if (line.TryGetValue("dialogLine", out List<object> dialogLines))
            {
                int index = 1;
                foreach (var dialogLine in dialogLines.OfType<OrderedDictionary>())
                {
                    if (TryExtractDialogLine(dialogLine, file, locId, out var lineMod))
                    {
                        var girlMod = GetGirlMod(index);

                        girlMod.LocationGreetingDialogLines ??= new();
                        girlMod.LocationGreetingDialogLines[locId] = lineMod;
                    }

                    index++;
                }
            }
        }
    }

    private void ExtractDialogLineSet(RelativeId dtId, OrderedDictionary lineSetDef, SerializedFile file)
        => ExtractDialogLineSet(dtId, lineSetDef, file, () => new RelativeId(Plugin.ModId, _dialogLineCount++));

    private void ExtractDialogLineSet(RelativeId dtId, OrderedDictionary lineSetDef, SerializedFile file, Func<RelativeId> getId)
    {
        //dtLineSets have a collection of lines. A line can be specified or is chosen at random
        if (!lineSetDef.TryGetValue("lines", out List<object> lines))
        {
            return;
        }

        foreach (var line in lines.OfType<OrderedDictionary>())
        {
            if (line.TryGetValue("dialogLine", out List<object> dialogLines))
            {
                //one of these for each girl, starting at 1
                int index = 1;
                foreach (var dialogLine in dialogLines.OfType<OrderedDictionary>())
                {
                    if (TryExtractDialogLine(dialogLine, file, getId(), out var lineMod))
                    {
                        var girlMod = GetGirlMod(index);

                        girlMod.LinesByDialogTriggerId ??= new();

                        var lineMods = girlMod.LinesByDialogTriggerId.GetOrNew(dtId);
                        lineMods.Add(lineMod);
                    }
                    index++;
                }
            }
        }
    }

    private string CleanText(string input)
    {
        var cleaned = input.Replace('·', '▸');
        return cleaned;
    }

    private DialogLineExpression ExtractDialogLineExpression(OrderedDictionary expressionDef, float charWeight)
    {
        var expression = new DialogLineExpression();

        if (expressionDef.TryGetValue("expression", out int expressionType))
        {
            switch (expressionType)
            {
                case 0://happy
                    expression.expressionType = GirlExpressionType.NEUTRAL;
                    break;
                case 1://sad
                    expression.expressionType = GirlExpressionType.DISAPPOINTED;
                    break;
                case 2://angy
                    expression.expressionType = GirlExpressionType.UPSET;
                    break;
                case 3://excited
                    expression.expressionType = GirlExpressionType.EXCITED;
                    break;
                case 4://shy
                    expression.expressionType = GirlExpressionType.SHY;
                    break;
                case 5://confused
                    expression.expressionType = GirlExpressionType.CONFUSED;
                    break;
                case 6://horny
                    expression.expressionType = GirlExpressionType.HORNY;
                    break;
                case 7://sick
                    expression.expressionType = GirlExpressionType.EXHAUSTED;
                    break;
            }
        }

        if (expressionDef.TryGetValue("closeEyes", out bool closeEyes))
        {
            expression.eyesClosed = closeEyes;
        }

        //so here the prahblem...
        //idk the percent read...
        //I can guess based on the characters I suppose...
        //hmmm
        if (expressionDef.TryGetValue("startAtCharIndex", out int startAtCharIndex))
        {
            expression.percentRead = startAtCharIndex * charWeight;
        }

        return expression;
    }

    private bool TryExtractDialogScene(OrderedDictionary dialogSceneDef, SerializedFile file, RelativeId girlId, out CutsceneDataMod cutsceneMod)
    {
        if (dialogSceneDef.TryGetValue("steps", out List<object> steps))
        {
            cutsceneMod = new(new RelativeId(Plugin.ModId, Cutscenes.NextCutsceneId++), InsertStyle.append);
            cutsceneMod.Steps = new();
            var hackyStyleEnum = ((IEnumerable<(RelativeId outfit, RelativeId hairstyle)>)_hackyStyleLookup[girlId]).GetEnumerator();
            UnityAssetPath currentAltGirl = UnityAssetPath.NullPath;
            foreach (var step in steps.OfType<OrderedDictionary>())
            {
                if (TryExtractDialogSceneStep(step, file, girlId, hackyStyleEnum, ref currentAltGirl, out var stepMods))
                {
                    cutsceneMod.Steps.AddRange(stepMods);
                }
            }

            return true;
        }
        else
        {
            ModInterface.Log.Warning("Failed to extract dialog scene");
        }

        cutsceneMod = null;
        return false;
    }

    private void ExtractIntroCutscene(RelativeId girlId, OrderedDictionary girlDef, SerializedFile file, SingleDatePairData pair)
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

    private bool TryExtractDialogSceneStep(OrderedDictionary dialogSceneStep,
        SerializedFile file,
        RelativeId girlId,
        IEnumerator<(RelativeId outfit, RelativeId hairstyle)> styleEnum,
        ref UnityAssetPath altGirlId,
        out CutsceneStepInfo[] stepMods)
    {
        if (dialogSceneStep.TryGetValue("type", out int type))
        {
            switch (type)
            {
                case 0://dialog line
                    {
                        if (dialogSceneStep.TryGetValue("sceneLine", out OrderedDictionary sceneLineDef)
                            && sceneLineDef.TryGetValue("altGirl", out bool altGirl)
                            && sceneLineDef.TryGetValue("dialogLine", out OrderedDictionary dialogLine)
                            && TryExtractDialogLine(dialogLine, file, new RelativeId(Plugin.ModId, _dialogLineCount++), out var lineMod))
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
                    }
                    break;
                case 1://response options
                    {
                        if (dialogSceneStep.TryGetValue("preventOptionShuffle", out bool preventOptionShuffle)
                            && dialogSceneStep.TryGetValue("responseOptions", out List<object> responseOptions))
                        {
                            var options = new List<IGameDefinitionInfo<CutsceneDialogOptionSubDefinition>>();

                            foreach (var responseOption in responseOptions.OfType<OrderedDictionary>())
                            {
                                if (TryExtractResponseOption(responseOption, file, girlId, styleEnum, ref altGirlId, out CutsceneDialogOptionInfo dialogOption))
                                {
                                    options.Add(dialogOption);
                                }
                            }

                            var stepMod = CutsceneStepUtility.MakeDialogOptionsInfo(options, !preventOptionShuffle, CutsceneStepProceedType.AUTOMATIC);
                            stepMods = [stepMod];
                            return true;
                        }
                        else
                        {
                            ModInterface.Log.Warning("failed to extract \"response options\" dialog step");
                        }
                    }
                    break;
                case 2://branch dialog
                    {
                        if (dialogSceneStep.TryGetValue("hasBestBranch", out bool hasBestBranch)
                            && dialogSceneStep.TryGetValue("conditionalBranchs", out List<object> conditionalBranches))// the og has it spelled wrong
                        {
                            var branches = new List<IGameDefinitionInfo<CutsceneBranchSubDefinition>>();

                            foreach (var branch in conditionalBranches.OfType<OrderedDictionary>())
                            {
                                if (TryExtractConditionalBranch(branch, file, girlId, styleEnum, ref altGirlId, out var branchMod))
                                {
                                    branches.Add(branchMod);
                                }
                            }

                            var stepMod = CutsceneStepUtility.MakeBranchInfo(branches, CutsceneStepProceedType.AUTOMATIC);
                            stepMods = [stepMod];
                            return true;
                        }
                        else
                        {
                            ModInterface.Log.Warning("failed to extract \"branch dialog\" dialog step");
                        }
                    }
                    break;
                case 3://show alt girl
                    {
                        if (dialogSceneStep.TryGetValue("altGirl", out OrderedDictionary altGirl)
                            && UnityAssetPath.TryExtract(altGirl, out var altGirlPath)
                            && dialogSceneStep.TryGetValue("showGirlStyles", out string showGirlStyles)
                            && dialogSceneStep.TryGetValue("hideOppositeSpeechBubble", out bool hideOppositeSpeechBubble))
                        {
                            var initialShow = altGirlId == UnityAssetPath.NullPath;
                            var steps = new List<CutsceneStepInfo>();

                            if (altGirlPath != altGirlId)
                            {
                                altGirlId = altGirlPath;
                                steps.Add(CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.HIDDEN, DollOrientationType.LEFT, initialShow ? CutsceneStepProceedType.INSTANT : CutsceneStepProceedType.AUTOMATIC));
                                var loadStep = CutsceneStepUtility.MakeLoadGirlInfo(Girls.FromUnityPath(altGirlId), CutsceneStepDollTargetType.ORIENTATION_TYPE, CutsceneStepProceedType.AUTOMATIC);
                                loadStep.TargetDollOrientation = DollOrientationType.LEFT;
                                steps.Add(loadStep);

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
                                steps.Add(loadStep);

                                if (!string.IsNullOrWhiteSpace(showGirlStyles) && styleEnum.MoveNext())
                                {
                                    loadStep.HairstyleId = styleEnum.Current.hairstyle;
                                    loadStep.OutfitId = styleEnum.Current.outfit;
                                }
                            }

                            steps.Add(CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.INNER, DollOrientationType.LEFT, CutsceneStepProceedType.AUTOMATIC));

                            stepMods = steps.ToArray();
                            return true;
                        }
                        else
                        {
                            ModInterface.Log.Warning("failed to extract \"show alt girl\" dialog step");
                        }
                    }
                    break;
                case 4://hide alt girl
                    {
                        var stepMod = CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.HIDDEN, DollOrientationType.LEFT, CutsceneStepProceedType.AUTOMATIC);
                        stepMods = [stepMod];
                        return true;
                    }
                case 5://show girl
                    {
                        if (dialogSceneStep.TryGetValue("showGirlStyles", out string showGirlStyles)
                            && dialogSceneStep.TryGetValue("hideOppositeSpeechBubble", out bool hideOppositeSpeechBubble))
                        {
                            var steps = new List<CutsceneStepInfo>();

                            if (!string.IsNullOrWhiteSpace(showGirlStyles))
                            {
                                var loadStep = CutsceneStepUtility.MakeLoadGirlInfo(girlId, CutsceneStepDollTargetType.ORIENTATION_TYPE, CutsceneStepProceedType.AUTOMATIC);
                                steps.Add(loadStep);

                                loadStep.TargetDollOrientation = DollOrientationType.RIGHT;

                                if (styleEnum.MoveNext())
                                {
                                    loadStep.HairstyleId = styleEnum.Current.hairstyle;
                                    loadStep.OutfitId = styleEnum.Current.outfit;
                                }
                            }

                            var initialShow = altGirlId == UnityAssetPath.NullPath;
                            steps.Add(CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.INNER, DollOrientationType.RIGHT, initialShow ? CutsceneStepProceedType.INSTANT : CutsceneStepProceedType.INSTANT));
                            stepMods = steps.ToArray();
                            return true;
                        }
                        else
                        {
                            ModInterface.Log.Warning("failed to extract \"show girl\" dialog step");
                        }
                    }
                    break;
                case 6://hide girl
                    {
                        var stepMod = CutsceneStepUtility.MakeDollMoveInfo(DollPositionType.HIDDEN, DollOrientationType.RIGHT, CutsceneStepProceedType.AUTOMATIC);
                        stepMods = [stepMod];
                        return true;
                    }
                case 7://insert scene
                    ModInterface.Log.Warning("\"insert scene\" dialog step unimplemented");
                    break;
                case 8://wait
                    {
                        if (dialogSceneStep.TryGetValue("waitTime", out int waitTime))
                        {
                            var stepMod = CutsceneStepUtility.MakeWaitInfo(waitTime);
                            stepMods = [stepMod];
                            return true;
                        }
                        else
                        {
                            ModInterface.Log.Warning("failed to extract \"wait\" dialog step");
                        }
                    }
                    break;
                case 9://set next loc
                    ModInterface.Log.Warning("\"set next location\" dialog step unimplemented");
                    break;
                case 10://set met status
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
                                CutsceneStepProceedType.INSTANT
                            );

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
                case 11://dialog trigger
                    ModInterface.Log.Warning("\"dialog trigger\" dialog step unimplemented");
                    break;
                case 12://know girl detail
                    ModInterface.Log.Warning("\"know girl detail\" dialog step unimplemented");
                    break;
                case 13://step back
                    if (dialogSceneStep.TryGetValue("stepBackSteps", out int stepBackSteps))
                    {
                        stepMods = [
                            CutsceneStepUtility.MakeRewindInfo(stepBackSteps, CutsceneStepProceedType.AUTOMATIC)
                        ];
                        return true;
                    }
                    break;
                case 14://add item
                    ModInterface.Log.Warning("\"add item\" dialog step unimplemented");
                    break;
                case 15://remove item
                    ModInterface.Log.Warning("\"remove item\" dialog step unimplemented");
                    break;
                case 16://wait for cellphone close
                    ModInterface.Log.Warning("\"wait for cellphone close\" dialog step unimplemented");
                    break;
                case 17://wait for token match
                    ModInterface.Log.Warning("\"wait for token match\" dialog step unimplemented");
                    break;
                case 18://wait for date gift
                    ModInterface.Log.Warning("\"wait for date gift\" dialog step unimplemented");
                    break;
                case 19://unlock cellphone
                    ModInterface.Log.Warning("\"unlock cellphone\" dialog step unimplemented");
                    break;
                case 20://make game pausable
                    ModInterface.Log.Warning("\"make game pausable\" dialog step unimplemented");
                    break;
                case 21://particle emitter
                    ModInterface.Log.Warning("\"particle emitter\" dialog step unimplemented");
                    break;
                case 22://send message
                    ModInterface.Log.Warning("\"send message\" dialog step unimplemented");
                    break;
            }
        }

        stepMods = null;
        return false;
    }

    private bool TryExtractConditionalBranch(OrderedDictionary branch, SerializedFile file, RelativeId girlId, IEnumerator<(RelativeId outfit, RelativeId hairstyle)> styleEnum, ref UnityAssetPath altGirl, out CutsceneBranchInfo branchMod)
    {
        if (branch.TryGetValue("type", out int type)
            && branch.TryGetValue("steps", out List<object> steps))
        {
            branchMod = new();

            // I'm just going to ignore the conditions for now.
            // I don't think any of the cutscenes I'm extracting need them
            // and their conditions don't really match hp2
            switch (type)
            {
                case 0://else (always treated as true and cannot be inverted...)
                    break;
                case 1://girl met status count
                    if (branch.TryGetValue("girlMetStatus", out int girlMetStatus))
                    {
                        branchMod = new();

                        switch (girlMetStatus)
                        {
                            case 0: //locked
                                break;
                            case 1: //unmet
                                break;
                            case 2: //unknown
                                break;
                            case 3: //met
                                break;
                        }

                        // I'm just going to ignore these for now
                        //branchMod.Conditions.Add(new LogicConditionInfo());
                    }
                    break;
                case 2://girl detail known
                    if (branch.TryGetValue("girlDetailType", out int girlDetailType))
                    {
                        switch (girlDetailType)
                        {
                            case 0: //last name
                                break;
                            case 1: //age
                                break;
                            case 2: //education
                                break;
                            case 3: //height
                                break;
                            case 4: //weight
                                break;
                            case 5: //occupation
                                break;
                            case 6: //cup size
                                break;
                            case 7: //birthday
                                break;
                            case 8: //hobby
                                break;
                            case 9: //fav color
                                break;
                            case 10: //fav season
                                break;
                            case 11: //fav hangout
                                break;
                        }
                    }
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
        }

        branchMod = null;
        return false;
    }

    private bool TryExtractResponseOption(OrderedDictionary responseOption, SerializedFile file, RelativeId girlId, IEnumerator<(RelativeId outfit, RelativeId hairstyle)> styleEnum, ref UnityAssetPath altGirlId, out CutsceneDialogOptionInfo dialogOption)
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

    private int _dialogLineCount = 0;

    private bool TryExtractDialogLine(OrderedDictionary dialogLine, SerializedFile file, RelativeId id, out DialogLineDataMod lineMod)
    {
        lineMod = new DialogLineDataMod(id);

        //the text may cause a bit of trouble here, it's formatted for the mouth movements
        //so we'll see how this goes...
        if (dialogLine.TryGetValue("text", out lineMod.DialogText))
        {
            lineMod.DialogText = CleanText(lineMod.DialogText);
        }
        else
        {
            ModInterface.Log.Warning("Failed to get straight line!");
            LogAll(dialogLine);
            return false;
        }

        if (lineMod.DialogText == "Dialog...")
        {
            return false;
        }

        if (dialogLine.TryGetValue("audioDefinition", out OrderedDictionary audioDefinition)
            && TryExtractAudioDef(audioDefinition, file, out var clipInfo))
        {
            lineMod.AudioClipInfo = clipInfo;
        }

        if (dialogLine.TryGetValue("secondary", out bool secondary)
            && secondary
            )
        {
            if (dialogLine.TryGetValue("secondaryText", out lineMod.YuriDialogText)
                && lineMod.YuriDialogText != null // some of the entries are wrong...
                && dialogLine.TryGetValue("secondaryAudioDefinition", out OrderedDictionary secondaryAudioDefinition)
                && TryExtractAudioDef(secondaryAudioDefinition, file, out var yuriClipInfo))
            {
                ModInterface.Log.IsNull(lineMod.YuriDialogText);
                lineMod.YuriDialogText = CleanText(lineMod.YuriDialogText);
                lineMod.Yuri = true;
                lineMod.YuriAudioClipInfo = yuriClipInfo;
            }
            else
            {
                ModInterface.Log.Warning("Failed to get yuri line!");
                return false;
            }
        }

        var charCount = 1f / (lineMod.DialogText?.Length ?? 100);

        if (dialogLine.TryGetValue("startExpression", out OrderedDictionary startExpressionDef))
        {
            lineMod.StartExpression = ExtractDialogLineExpression(startExpressionDef, charCount);
        }

        if (dialogLine.TryGetValue("endExpression", out OrderedDictionary endExpressionDef))
        {
            lineMod.EndExpression = ExtractDialogLineExpression(endExpressionDef, charCount);
        }

        if (dialogLine.TryGetValue("expressions", out List<object> expressions))
        {
            lineMod.Expressions = expressions
                .OfType<OrderedDictionary>()
                .Select(x => ExtractDialogLineExpression(x, charCount))
                .ToList();
        }

        return true;
    }
}