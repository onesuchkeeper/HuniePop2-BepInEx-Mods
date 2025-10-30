using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AssetStudio;
using Hp2BaseMod;
using Hp2BaseMod.Extension.OrderedDictionaryExtension;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

public partial class HpExtraction
{
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
                        ExtractDialogLineSet(new RelativeId(-1, 13), lineSets[0], file);//ask date
                    }
                    break;
                case "DateGreeting":
                    if (lineSets.Length > 11)
                    {
                        //0-11 - locations
                        //12 - bonus round loc (use for cutscene instead)
                        for (int i = 0; i < 12; i++)// TODO map these properly to the locations
                        {
                            ExtractDialogLineSet(new RelativeId(-1, 15), lineSets[i], file);
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
                        ExtractDialogLineSet(new RelativeId(-1, 16), lineSets[0], file);//date success
                        ExtractDialogLineSet(new RelativeId(-1, 17), lineSets[1], file);//date fail
                        ExtractDialogLineSet(DialogTriggers.PreBedroom, lineSets[2], file);
                    }
                    break;
                case "GivenDateGift":
                    if (lineSets.Length > 1)
                    {
                        //0 - success
                        //1 - fail
                        ExtractDialogLineSet(new RelativeId(-1, 30), lineSets[0], file);//date gift accept
                        ExtractDialogLineSet(new RelativeId(-1, 31), lineSets[0], file);//date gift sexy
                        ExtractDialogLineSet(new RelativeId(-1, 32), lineSets[0], file);//date gift weird
                        ExtractDialogLineSet(new RelativeId(-1, 33), lineSets[1], file);//date gift reject
                    }
                    break;
                case "GivenDrink":
                    if (lineSets.Length > 3)
                    {
                        //0 - inebriation == 12, drunk
                        //1 - appetite == 0, too hungry
                        //2 - not at drinking location
                        //3 - default
                        ExtractDialogLineSet(new RelativeId(-1, 18), lineSets[3], file);//smoothie accept
                        ExtractDialogLineSet(new RelativeId(-1, 19), lineSets[1], file);//smoothie full
                        ExtractDialogLineSet(new RelativeId(-1, 20), lineSets[2], file);//smoothie reject
                    }
                    break;
                case "GivenFood":
                    if (lineSets.Length > 3)
                    {
                        //0 - appetite == 12, full
                        //1 - loves
                        //2 - likes
                        //3 - default
                        ExtractDialogLineSet(new RelativeId(-1, 21), lineSets[2], file);//food accept
                        ExtractDialogLineSet(new RelativeId(-1, 22), lineSets[3], file);//food reject
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

                        ExtractDialogLineSet(new RelativeId(-1, 23), lineSets[3], file);//unique accept
                        ExtractDialogLineSet(new RelativeId(-1, 23), lineSets[4], file);//unique reject

                        ExtractDialogLineSet(new RelativeId(-1, 25), lineSets[1], file);//shoes accept
                        ExtractDialogLineSet(new RelativeId(-1, 25), lineSets[4], file);//shoes reject

                        ExtractDialogLineSet(new RelativeId(-1, 27), lineSets[2], file);//shell accept
                        ExtractDialogLineSet(new RelativeId(-1, 27), lineSets[4], file);//shell reject
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
                        ExtractDialogLineSet(new RelativeId(-1, 9), lineSets[0], file);//greeting morning
                        ExtractDialogLineSet(new RelativeId(-1, 10), lineSets[1], file);//greeting afternoon
                        ExtractDialogLineSet(new RelativeId(-1, 11), lineSets[2], file);//greeting evening
                        ExtractDialogLineSet(new RelativeId(-1, 12), lineSets[3], file);//greeting night
                        ExtractDialogLineSet(DialogTriggers.PostSex, lineSets[4], file);
                    }
                    break;
                case "InventoryFull":
                    if (lineSets.Length > 0)
                    {
                        ExtractDialogLineSet(new RelativeId(-1, 29), lineSets[0], file);//inventory full
                    }
                    break;
                case "IsTooHungry":
                    if (lineSets.Length > 0)
                    {
                        ExtractDialogLineSet(new RelativeId(-1, 35), lineSets[0], file);//stamina insufficient
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
                        ExtractDialogLineSet(new RelativeId(-1, 34), lineSets[0], file);//big move
                        ExtractDialogLineSet(new RelativeId(-1, 39), lineSets[1], file);//broken exhausted
                    }
                    break;
                case "SexualSounds":
                    if (lineSets.Length > 4)
                    {
                        // 0 - 3 - sex moans
                        ExtractDialogLineSet(new RelativeId(-1, 43), lineSets[0], file);//sex moans 1
                        ExtractDialogLineSet(new RelativeId(-1, 44), lineSets[1], file);//sex moans 2
                        ExtractDialogLineSet(new RelativeId(-1, 45), lineSets[2], file);//sex moans 3
                        ExtractDialogLineSet(new RelativeId(-1, 46), lineSets[3], file);//sex moans 4
                        ExtractDialogLineSet(new RelativeId(-1, 47), lineSets[4], file);//sex climax
                    }
                    break;
                case "Valediction"://leave without dating
                    if (lineSets.Length > 3)
                    {
                        // 0 morning
                        // 1 afternoon
                        // 2 evening
                        // 3 night
                        ExtractDialogLineSet(new RelativeId(-1, 14), lineSets[0], file);
                        ExtractDialogLineSet(new RelativeId(-1, 14), lineSets[1], file);
                        ExtractDialogLineSet(new RelativeId(-1, 14), lineSets[2], file);
                    }
                    break;
                case "QueryDuplicate":
                    break;
                case "QueryIntro":
                    ExtractDialogLineSet(new RelativeId(-1, 4), lineSets[0], file);//favQuestionIntro
                    break;
                case "QuestionBadChoice":
                    ExtractDialogLineSet(new RelativeId(-1, 3), lineSets[0], file);//herQuestionBadResponse
                    break;
                case "QuestionCorrect":
                    ExtractDialogLineSet(new RelativeId(-1, 8), lineSets[0], file);//herQuestionGoodResponse
                    break;
                case "QuestionIncorrect":
                    break;
                case "QuestionIntro":
                    ExtractDialogLineSet(new RelativeId(-1, 2), lineSets[0], file);//herQuestionIntro
                    break;
                case "QuizCorrect":
                    ExtractDialogLineSet(new RelativeId(-1, 6), lineSets[0], file);//favQuestionAgree
                    ExtractDialogLineSet(new RelativeId(-1, 5), lineSets[0], file);//favQuestionResponse
                    break;
                case "QuizIncorrect":
                    break;
                case "QuizIntro":
                    break;
                default:
                    ModInterface.Log.LogError($"Unhandled hp1 dt {name}");
                    break;
            }
        }
    }

    int _dialogLineCount = 0;

    private void ExtractDialogLineSet(RelativeId dtId, OrderedDictionary lineSetDef, SerializedFile file)
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
                    var girlMod = GetGirlMod(index++);

                    var lineMod = new DialogLineDataMod(new RelativeId(Plugin.ModId, _dialogLineCount++));
                    girlMod.linesByDialogTriggerId ??= new();

                    var lineMods = girlMod.linesByDialogTriggerId.FirstOrDefault(x => x.Item1 == dtId).Item2;
                    if (lineMods == null)
                    {
                        lineMods = new();
                        girlMod.linesByDialogTriggerId.Add((dtId, lineMods));
                    }
                    lineMods.Add(ExtractDialogLine(dialogLine, file));
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

    private void ExtractDialogScene(OrderedDictionary dialogSceneDef)
    {
        if (dialogSceneDef.TryGetValue("steps", out List<object> steps))
        {
            foreach (var step in steps.OfType<OrderedDictionary>())
            {

            }
        }
    }

    private void ExtractDialogSceneStep(OrderedDictionary dialogSceneStep)
    {
        if (dialogSceneStep.TryGetValue("type", out int type))
        {
            switch (type)
            {
                case 0://dialog line

                    break;
                case 1://response options
                    break;
                case 2://branch dialog
                    break;
                case 3://show alt girl
                    break;
                case 4://hide alt girl
                    break;
                case 5://show girl
                    break;
                case 6://hide girl
                    break;
                case 7://insert scene
                    break;
                case 8://wait
                    break;
                case 9://set next loc
                    break;
                case 10://set met status
                    break;
                case 11://dialog trigger
                    break;
                case 12://know girl detail
                    break;
                case 13://step back
                    break;
                case 14://add item
                    break;
                case 15://remove item
                    break;
                case 16://wait for cellphone close
                    break;
                case 17://wait for token match
                    break;
                case 18://wait for date gift
                    break;
                case 19://unlock cellphone
                    break;
                case 20://make game pausable
                    break;
                case 21://particle emitter
                    break;
                case 22://send message
                    break;
            }
        }
    }

    private CutsceneStepInfo ExtractDialogSceneLine(OrderedDictionary dialogSceneLineDef)
    {
        if (dialogSceneLineDef.TryGetValue("altGirl", out bool altGirl)
            && dialogSceneLineDef.TryGetValue("dialogLine", out OrderedDictionary dialogLine))
        {
            var step = CutsceneStepInfo.MakeDialogLine(new RelativeId(), false, CutsceneStepProceedType.AUTOMATIC, CutsceneStepDollTargetType.ORIENTATION_TYPE);
            step.TargetDollOrientation = altGirl ? DollOrientationType.LEFT : DollOrientationType.RIGHT;
            return step;
        }

        return new();
    }

    private DialogLineDataMod ExtractDialogLine(OrderedDictionary dialogLine, SerializedFile file)
    {
        var lineMod = new DialogLineDataMod(new RelativeId(Plugin.ModId, _dialogLineCount++));

        //the text may cause a bit of trouble here, it's formatted for the mouth movements
        //so we'll see how this goes...
        dialogLine.TryGetValue("text", out lineMod.DialogText);
        lineMod.DialogText = CleanText(lineMod.DialogText);

        if (dialogLine.TryGetValue("audioDefinition", out OrderedDictionary audioDefinition)
            && TryExtractAudioDef(audioDefinition, file, out var clipInfo))
        {
            lineMod.AudioClipInfo = clipInfo;
        }

        if (dialogLine.TryGetValue("secondary", out bool secondary)
            && secondary
            && dialogLine.TryGetValue("secondaryAudioDefinition", out OrderedDictionary secondaryAudioDefinition)
            && TryExtractAudioDef(secondaryAudioDefinition, file, out var yuriClipInfo))
        {
            dialogLine.TryGetValue("secondaryText", out lineMod.YuriDialogText);
            lineMod.YuriDialogText = CleanText(lineMod.YuriDialogText);
            lineMod.Yuri = true;
            lineMod.YuriAudioClipInfo = yuriClipInfo;
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

        return lineMod;
    }
}