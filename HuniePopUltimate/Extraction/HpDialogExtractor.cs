using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using AssetStudio;
using Hp2BaseMod;
using Hp2BaseMod.Extension;
using Hp2BaseMod.GameDataInfo;

namespace HuniePopUltimate;

/// <summary>
/// Reads HP1 DialogTriggerDefinition assets and populates each girl's
/// <see cref="GirlDataMod.LinesByDialogTriggerId"/> and
/// <see cref="GirlDataMod.LocationGreetingDialogLines"/>.
/// </summary>
public class HpDialogExtractor
{
    private readonly HpAudioCache _audio;
    private readonly Dictionary<int, IGirlConfigurator> _configs;

    public int DialogLineCount { get; private set; } = 0;

    public HpDialogExtractor(HpAudioCache audio, Dictionary<int, IGirlConfigurator> configs)
    {
        _audio = audio;
        _configs = configs;
    }

    public void ExtractDialogTrigger(OrderedDictionary dtDef, SerializedFile file)
    {
        if (!dtDef.TryGetValue("lineSets", out List<object> lineSetsList))
        {
            return;
        }

        var lineSets = lineSetsList.OfType<OrderedDictionary>().ToArray();

        if (!dtDef.TryGetValue("m_Name", out string name))
        {
            return;
        }

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
                    // 0-11 - date locations
                    // 12 - bonus round loc (use for cutscene instead)
                    for (int i = 0; i < 12; i++)
                    {
                        ExtractLocGreetingDialogLineSet(LocationIds.FromGreetingDtIndex(i), lineSets[i], file);
                    }
                    ExtractDialogLineSet(DialogTriggers.PreSex, lineSets[12], file);
                }
                break;
            case "DateValediction":
                if (lineSets.Length > 1)
                {
                    // 0 - success
                    // 1 - fail
                    // 2 - goToBonus round - repurposed for cutscenes
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateSuccess, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateFailure, lineSets[1], file);
                    ExtractDialogLineSet(DialogTriggers.PreBedroom, lineSets[2], file);
                }
                break;
            case "GivenDateGift":
                if (lineSets.Length > 1)
                {
                    // 0 - success
                    // 1 - fail
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateGiftAccept, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateGiftSexy, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateGiftWeird, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.DateGiftReject, lineSets[1], file);
                }
                break;
            case "GivenDrink":
                if (lineSets.Length > 3)
                {
                    // 0 - inebriation == 12, drunk
                    // 1 - appetite == 0, too hungry
                    // 2 - not at drinking location
                    // 3 - default
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SmoothieAccept, lineSets[3], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SmoothieFull, lineSets[1], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SmoothieReject, lineSets[2], file);
                }
                break;
            case "GivenFood":
                if (lineSets.Length > 3)
                {
                    // 0 - appetite == 12, full
                    // 1 - loves
                    // 2 - likes
                    // 3 - default
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.FoodAccept, lineSets[2], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.FoodReject, lineSets[3], file);
                }
                break;
            case "GivenGift":
                if (lineSets.Length > 4)
                {
                    // 0 - loves
                    // 1 - likes type 1
                    // 2 - like type 2
                    // 3 - unique gift type
                    // 4 - default / reject
                    // 5 - dirty magazine given to kyu

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
                    // 0 - morning / 1 - afternoon / 2 - evening / 3 - night
                    // 4 - post sex (use in cutscene)
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
                // 13 - given tissue box
                // item.id 277 is for panties given to kyu
                // 12 - given all panties, activate alpha mode
                break;
            case "MatchToken":
                if (lineSets.Length > 1)
                {
                    // 0 - big move
                    // 1 - broken heart
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.BigMove, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.StaminaRecovered, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.BrokenRecovered, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.BrokenExhausted, lineSets[1], file);
                }
                break;
            case "SexualSounds":
                if (lineSets.Length > 4)
                {
                    // 0-3 - sex moans
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexMoans1, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexMoans2, lineSets[1], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexMoans3, lineSets[2], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexMoans4, lineSets[3], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.SexClimax, lineSets[4], file);
                }
                break;
            case "Valediction": // leave without dating
                if (lineSets.Length > 3)
                {
                    // 0 morning / 1 afternoon / 2 evening / 3 night
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.Valediction, lineSets[0], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.Valediction, lineSets[1], file);
                    ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.Valediction, lineSets[2], file);
                }
                break;
            case "QueryDuplicate":
                break;
            case "QueryIntro":
                ExtractDialogLineSet(Hp2BaseMod.DialogTriggers.FavQuestionIntro, lineSets[0], file);
                break;
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

    private void ExtractLocGreetingDialogLineSet(RelativeId locId, OrderedDictionary lineSetDef, SerializedFile file)
    {
        if (!lineSetDef.TryGetValue("lines", out List<object> lines))
        {
            return;
        }

        foreach (var line in lines.OfType<OrderedDictionary>())
        {
            if (!line.TryGetValue("dialogLine", out List<object> dialogLines))
            {
                continue;
            }

            int index = 1;
            foreach (var dialogLine in dialogLines.OfType<OrderedDictionary>())
            {
                if (TryExtractDialogLine(dialogLine, file, locId, out var lineMod))
                {
                    var girlMod = _configs[index].Mod;
                    girlMod.LocationGreetingDialogLines ??= new();
                    girlMod.LocationGreetingDialogLines[locId] = lineMod;
                }
                index++;
            }
        }
    }

    private void ExtractDialogLineSet(RelativeId dtId, OrderedDictionary lineSetDef, SerializedFile file)
        => ExtractDialogLineSet(dtId, lineSetDef, file, () => new RelativeId(Plugin.ModId, DialogLineCount++));

    private void ExtractDialogLineSet(RelativeId dtId, OrderedDictionary lineSetDef, SerializedFile file, Func<RelativeId> getId)
    {
        if (!lineSetDef.TryGetValue("lines", out List<object> lines))
        {
            return;
        }

        var isUniqueAcceptDt = dtId == Hp2BaseMod.DialogTriggers.UniqueAccept;

        foreach (var line in lines.OfType<OrderedDictionary>())
        {
            if (!line.TryGetValue("dialogLine", out List<object> dialogLines))
            {
                continue;
            }

            // one entry per girl, starting at index 1
            int index = 1;
            foreach (var dialogLine in dialogLines.OfType<OrderedDictionary>())
            {
                var girlConfig = _configs[index];

                if ((!isUniqueAcceptDt || girlConfig.ExtractUniqueAcceptDialogLines)
                    && TryExtractDialogLine(dialogLine, file, getId(), out var lineMod))
                {
                    var girlMod = girlConfig.Mod;
                    girlMod.LinesByDialogTriggerId ??= new();
                    girlMod.LinesByDialogTriggerId.GetOrNew(dtId).Add(lineMod);
                }

                index++;
            }
        }
    }

    public bool TryExtractDialogLine(OrderedDictionary dialogLine, SerializedFile file, RelativeId id, out DialogLineDataMod lineMod)
    {
        lineMod = new DialogLineDataMod(id);

        if (dialogLine.TryGetValue("text", out lineMod.DialogText))
        {
            lineMod.DialogText = CleanText(lineMod.DialogText);
        }
        else
        {
            HpDebugLog.DialogWarning("Failed to get straight line!");
            HpDebugLog.LogAll(dialogLine);
            return false;
        }

        if (lineMod.DialogText == "Dialog...")
        {
            return false;
        }

        if (dialogLine.TryGetValue("audioDefinition", out OrderedDictionary audioDefinition)
            && _audio.TryExtractAudioDef(audioDefinition, file, out var clipInfo))
        {
            lineMod.AudioClipInfo = clipInfo;
        }

        if (dialogLine.TryGetValue("secondary", out bool secondary) && secondary)
        {
            if (dialogLine.TryGetValue("secondaryText", out lineMod.YuriDialogText)
                && lineMod.YuriDialogText != null
                && dialogLine.TryGetValue("secondaryAudioDefinition", out OrderedDictionary secondaryAudioDefinition)
                && _audio.TryExtractAudioDef(secondaryAudioDefinition, file, out var yuriClipInfo))
            {
                ModInterface.Log.IsNull(lineMod.YuriDialogText);
                lineMod.YuriDialogText = CleanText(lineMod.YuriDialogText);
                lineMod.Yuri = true;
                lineMod.YuriAudioClipInfo = yuriClipInfo;
            }
            else
            {
                HpDebugLog.DialogWarning("Failed to get yuri line!");
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

    public DialogLineExpression ExtractDialogLineExpression(OrderedDictionary expressionDef, float charWeight)
    {
        var expression = new DialogLineExpression();

        if (expressionDef.TryGetValue("expression", out int expressionType))
        {
            switch (expressionType)
            {
                case 0: expression.expressionType = GirlExpressionType.NEUTRAL; break;
                case 1: expression.expressionType = GirlExpressionType.DISAPPOINTED; break;
                case 2: expression.expressionType = GirlExpressionType.UPSET; break;
                case 3: expression.expressionType = GirlExpressionType.EXCITED; break;
                case 4: expression.expressionType = GirlExpressionType.SHY; break;
                case 5: expression.expressionType = GirlExpressionType.CONFUSED; break;
                case 6: expression.expressionType = GirlExpressionType.HORNY; break;
                case 7: expression.expressionType = GirlExpressionType.EXHAUSTED; break;
            }
        }

        if (expressionDef.TryGetValue("closeEyes", out bool closeEyes))
        {
            expression.eyesClosed = closeEyes;
        }

        if (expressionDef.TryGetValue("startAtCharIndex", out int startAtCharIndex))
        {
            expression.percentRead = startAtCharIndex * charWeight;
        }

        return expression;
    }

    public RelativeId NextDialogLineId() => new RelativeId(Plugin.ModId, DialogLineCount++);

    private static string CleanText(string input) => input.Replace('·', '▸');
}