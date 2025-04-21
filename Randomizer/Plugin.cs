using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.Extension.StringExtension;
using Newtonsoft.Json;

namespace Hp2Randomizer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OSK.BepInEx.Hp2BaseMod", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal static SaveFile ModConfig;
    private static int _modId;

    /// <summary>
    /// Sets the swap handler for a particular Special Girl.
    /// Handler accepts the the special girl, then the girl she swaps with
    /// and swaps their properties
    /// </summary>
    public static void SetSpecialCharacterSwapHandler(RelativeId specialGirlId, Action<GirlDefinition, GirlDefinition> swapHandler)
        => _swapHandlers[specialGirlId] = swapHandler;

    private static Dictionary<RelativeId, Action<GirlDefinition, GirlDefinition>> _swapHandlers = new Dictionary<RelativeId, Action<GirlDefinition, GirlDefinition>>();

    private DialogTriggerDefinition[] _moanTriggers;

    private void Awake()
    {
        _modId = ModInterface.GetSourceId(MyPluginInfo.PLUGIN_GUID);

        ModInterface.AddCommand(new SetSeedCommand());

        ModInterface.Events.PreGameSave += On_PreSave;
        ModInterface.Events.PostDataMods += On_PostDataMods;
        ModInterface.Events.PreLoadPlayerFile += On_PreLoadPlayerFile;
    }

    private void On_PreLoadPlayerFile(PlayerFile file)
    {
        //if randomization has made a girl's saved outfit invalid, set it to the default
        foreach (var fileGirl in file.girls)
        {
            if (fileGirl.outfitIndex >= fileGirl.girlDefinition.outfits.Count
                || !fileGirl.unlockedOutfits.Contains(fileGirl.outfitIndex))
            {
                fileGirl.outfitIndex = fileGirl.girlDefinition.defaultOutfitIndex;
            }

            if (fileGirl.hairstyleIndex >= fileGirl.girlDefinition.hairstyles.Count
                || !fileGirl.unlockedHairstyles.Contains(fileGirl.hairstyleIndex))
            {
                fileGirl.hairstyleIndex = fileGirl.girlDefinition.defaultHairstyleIndex;
            }
        }
    }

    private void SwapNymphojinn(GirlDefinition nymphojinnDef, GirlDefinition otherGirlDef)
    {
        //find the neutral glowing eyes to use for expressions that don't have glowing eyes
        var defaultGlowEyesIndex = -1;
        if (otherGirlDef.expressions.TryGetFirst(x => x.expressionType == GirlExpressionType.NEUTRAL, out var neutralExpression))
        {
            defaultGlowEyesIndex = neutralExpression.partIndexEyesGlow;
        }
        else
        {
            ModInterface.Log.LogWarning($"Unable to find neutral expression for girl {otherGirlDef.name}");
        }

        foreach (var expression in otherGirlDef.expressions)
        {
            expression.partIndexEyes = expression.partIndexEyesGlow == -1
                ? defaultGlowEyesIndex
                : expression.partIndexEyesGlow;
        }

        foreach (var expression in otherGirlDef.expressions)
        {
            if (expression.partIndexEyesGlow != -1)
            {
                expression.partIndexEyes = expression.partIndexEyesGlow;
            }
        }

        foreach (var dt in _moanTriggers)
        {
            var hold = dt.dialogLineSets[(int)nymphojinnDef.dialogTriggerTab];
            dt.dialogLineSets[(int)nymphojinnDef.dialogTriggerTab] = dt.dialogLineSets[(int)otherGirlDef.dialogTriggerTab];
            dt.dialogLineSets[(int)otherGirlDef.dialogTriggerTab] = hold;
        }

        nymphojinnDef.defaultHairstyleIndex = 0;
        nymphojinnDef.defaultOutfitIndex = 0;

        //girls already have nymphojinn wing positions for the boss,
        //except kyu. The special effects are hard-coded to look at the
        //def.SpecialEffectOffset which is unfortunate, maybe change kyu wings to not use that?
        if (ModConfig.SwappedSpecialCharactersKeepWings)
        {
            otherGirlDef.specialEffectPrefab = nymphojinnDef.specialEffectPrefab;
        }
    }

    private void SwapKyu(GirlDefinition kyuDef, GirlDefinition otherGirlDef)
    {
        kyuDef.defaultHairstyleIndex = 1;
        kyuDef.defaultOutfitIndex = 1;

        if (ModConfig.SwappedSpecialCharactersKeepWings)
        {
            otherGirlDef.specialEffectPrefab = kyuDef.specialEffectPrefab;
            otherGirlDef.specialEffectOffset = kyuDef.specialEffectOffset;
        }
    }

    private void On_PreSave()
    {
        ModInterface.SetSourceSave(_modId, JsonConvert.SerializeObject(ModConfig));
    }

    private void On_PostDataMods()
    {
        var saveString = ModInterface.GetSourceSave(_modId);

        if (saveString.IsNullOrWhiteSpace())
        {
            ModConfig = new SaveFile();
            ModConfig.Seed = Environment.TickCount;
        }
        else
        {
            ModConfig = JsonConvert.DeserializeObject<SaveFile>(saveString) ?? new SaveFile();
        }

        if (ModConfig.Disable)
        {
            ModInterface.Log.LogInfo($"Randomizer disabled");
            return;
        }

        if (ModConfig.IncludeKyu)
        {
            _swapHandlers.Add(Girls.KyuId, SwapKyu);
        }

        if (ModConfig.IncludeNymphojinn)
        {
            _swapHandlers.Add(Girls.MoxieId, SwapNymphojinn);
            _swapHandlers.Add(Girls.JewnId, SwapNymphojinn);
        }

        ModInterface.Log.LogInfo($"Randomizing, seed:{ModConfig.Seed}");
        var random = new Random(ModConfig.Seed);

        var normalGirls = Game.Data.Girls.GetAllBySpecial(false);
        var normalPairs = Game.Data.GirlPairs.GetAll().Where(x => !x.girlDefinitionOne.specialCharacter && !x.girlDefinitionTwo.specialCharacter).ToArray();

        //determine pairs
        var degreeTotal = normalPairs.Length * 2;
        var pairAllotment = degreeTotal / normalGirls.Count;
        var remaining = degreeTotal % normalGirls.Count;

        var degrees = new int[normalGirls.Count];
        int i = 0;
        for (i = 0; i < remaining; i++)
        {
            degrees[i] = pairAllotment + 1;
        }

        for (; i < normalGirls.Count; i++)
        {
            degrees[i] = pairAllotment;
        }

        IEnumerable<(int a, int b)> pairings;
        if (!ModConfig.RandomizePairs)
        {
            pairings = normalPairs.Select(x => (x.girlDefinitionOne.id, x.girlDefinitionTwo.id));
        }
        else if (UndirectedGraph.TryMakeConnectedHHGraph(degrees, out var graph))
        {
            pairings = graph.Randomize(degreeTotal * 3, random).Edges;
        }
        else
        {
            ModInterface.Log.LogWarning($"Failed to create simple graph from pairings, they will not be randomized.");
            pairings = normalPairs.Select(x => (x.girlDefinitionOne.id, x.girlDefinitionTwo.id));
        }

        //gather data pools
        var namePool = new List<(string name, string nickName)>()
        {
            //hp
            ("Misty", null),
            ("Aiko", null),
            ("Beli", null),
            ("Kyanna", null),
            ("Tiffany", null),
            ("Celeste", null),
            ("Venus", null),
            ("Nikki", null),
            ("Audrey", null),
            ("Momo", null),
            //hcs
            ("Marlena", null),
            ("Nadia", null),
            ("Renee", null),
        };

        var assignedNames = new Dictionary<GirlDefinition, (string name, string nickName)>();

        var baggagePool = new List<(GirlDefinition girl, ItemDefinition ailment)>();

        foreach (var girl in normalGirls)
        {
            namePool.Add((girl.girlName, girl.girlNickName));
            baggagePool.AddRange(girl.baggageItemDefs.Select(x => (girl, x)));
        }

        _moanTriggers = [
            ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 43)),
                ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 44)),
                ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 45)),
                ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 46)),
                ModInterface.GameData.GetDialogTrigger(new RelativeId(-1, 47))
        ];

        //special characters don't have all the stuff they need,
        //so instead I'll just swap their visuals and other bits with someone
        //swaps like this aren't the same odds but I don't really care
        foreach (var id_handler in _swapHandlers)
        {
            var specialGirl = ModInterface.GameData.GetGirl(id_handler.Key);
            var target = normalGirls[random.Next() % normalGirls.Count];

            namePool.Add((specialGirl.girlName, specialGirl.girlNickName));

            assignedNames[specialGirl] = ModConfig.RandomizeNames
                ? PopRandom(random, namePool)
                : (specialGirl.girlName, specialGirl.girlNickName);

            var specialId = ModInterface.Data.GetDataId(GameDataType.Girl, specialGirl.id);
            var targetId = ModInterface.Data.GetDataId(GameDataType.Girl, target.id);

            id_handler.Value.Invoke(specialGirl, target);

            ModInterface.Data.SwapGirlStyles(specialId, targetId);

            var holdParts = specialGirl.parts;
            specialGirl.parts = target.parts;
            target.parts = holdParts;

            var holdSpecialParts = specialGirl.specialParts;
            specialGirl.specialParts = target.specialParts;
            target.specialParts = holdSpecialParts;

            var holdInt = specialGirl.partIndexBlink;
            specialGirl.partIndexBlink = target.partIndexBlink;
            target.partIndexBlink = holdInt;

            holdInt = specialGirl.partIndexBlushHeavy;
            specialGirl.partIndexBlushHeavy = target.partIndexBlushHeavy;
            target.partIndexBlushHeavy = holdInt;

            holdInt = specialGirl.partIndexBlushLight;
            specialGirl.partIndexBlushLight = target.partIndexBlushLight;
            target.partIndexBlushLight = holdInt;

            holdInt = specialGirl.partIndexBody;
            specialGirl.partIndexBody = target.partIndexBody;
            target.partIndexBody = holdInt;

            var holdIndexList = specialGirl.partIndexesPhonemes;
            specialGirl.partIndexesPhonemes = target.partIndexesPhonemes;
            target.partIndexesPhonemes = holdIndexList;

            holdIndexList = specialGirl.partIndexesPhonemesTeeth;
            specialGirl.partIndexesPhonemesTeeth = target.partIndexesPhonemesTeeth;
            target.partIndexesPhonemesTeeth = holdIndexList;

            holdInt = specialGirl.partIndexMouthNeutral;
            specialGirl.partIndexMouthNeutral = target.partIndexMouthNeutral;
            target.partIndexMouthNeutral = holdInt;

            holdInt = specialGirl.partIndexNipples;
            specialGirl.partIndexNipples = target.partIndexNipples;
            target.partIndexNipples = holdInt;

            holdInt = specialGirl.defaultOutfitIndex;
            specialGirl.defaultOutfitIndex = target.defaultOutfitIndex;
            target.defaultOutfitIndex = holdInt;

            holdInt = specialGirl.defaultHairstyleIndex;
            specialGirl.defaultHairstyleIndex = target.defaultHairstyleIndex;
            target.defaultHairstyleIndex = holdInt;

            var holdOutfits = specialGirl.outfits;
            specialGirl.outfits = target.outfits;
            target.outfits = holdOutfits;

            var holdHair = specialGirl.hairstyles;
            specialGirl.hairstyles = target.hairstyles;
            target.hairstyles = holdHair;

            holdInt = specialGirl.defaultExpressionIndex;
            specialGirl.defaultExpressionIndex = target.defaultExpressionIndex;
            target.defaultExpressionIndex = holdInt;

            var holdExpressions = specialGirl.expressions;
            specialGirl.expressions = target.expressions;
            target.expressions = holdExpressions;

            var holdVec2 = specialGirl.specialEffectOffset;
            specialGirl.specialEffectOffset = target.specialEffectOffset;
            target.specialEffectOffset = holdVec2;

            var holdSpecialEffectPrefab = specialGirl.specialEffectPrefab;
            specialGirl.specialEffectPrefab = target.specialEffectPrefab;
            target.specialEffectPrefab = holdSpecialEffectPrefab;

            var holdSprite = specialGirl.cellphoneHead;
            specialGirl.cellphoneHead = target.cellphoneHead;
            target.cellphoneHead = holdSprite;

            holdSprite = specialGirl.cellphoneHeadAlt;
            specialGirl.cellphoneHeadAlt = target.cellphoneHeadAlt;
            target.cellphoneHeadAlt = holdSprite;

            holdSprite = specialGirl.cellphoneMiniHead;
            specialGirl.cellphoneMiniHead = target.cellphoneMiniHead;
            target.cellphoneMiniHead = holdSprite;

            holdSprite = specialGirl.cellphoneMiniHeadAlt;
            specialGirl.cellphoneMiniHeadAlt = target.cellphoneMiniHeadAlt;
            target.cellphoneMiniHeadAlt = holdSprite;

            holdSprite = specialGirl.cellphonePortrait;
            specialGirl.cellphonePortrait = target.cellphonePortrait;
            target.cellphonePortrait = holdSprite;

            holdSprite = specialGirl.cellphonePortraitAlt;
            specialGirl.cellphonePortraitAlt = target.cellphonePortraitAlt;
            target.cellphonePortraitAlt = holdSprite;

            holdVec2 = specialGirl.upsetEmitterPos;
            specialGirl.upsetEmitterPos = target.upsetEmitterPos;
            target.upsetEmitterPos = holdVec2;

            holdVec2 = specialGirl.breathEmitterPos;
            specialGirl.breathEmitterPos = target.breathEmitterPos;
            target.breathEmitterPos = holdVec2;

            var holdBool = specialGirl.hasAltStyles;
            specialGirl.hasAltStyles = target.hasAltStyles;
            target.hasAltStyles = holdBool;

            var holdString = specialGirl.altStylesFlagName;
            specialGirl.altStylesFlagName = target.altStylesFlagName;
            target.altStylesFlagName = holdString;

            var holdCode = specialGirl.altStylesCodeDefinition;
            specialGirl.altStylesCodeDefinition = target.altStylesCodeDefinition;
            target.altStylesCodeDefinition = holdCode;

            holdCode = specialGirl.unlockStyleCodeDefinition;
            specialGirl.unlockStyleCodeDefinition = target.unlockStyleCodeDefinition;
            target.unlockStyleCodeDefinition = holdCode;

            holdInt = specialGirl.failureExpressionIndex;
            specialGirl.failureExpressionIndex = target.failureExpressionIndex;
            target.failureExpressionIndex = holdInt;
        }

        var handledCutscenes = new HashSet<CutsceneDefinition>();

        //randomize girls
        foreach (var girl in normalGirls)
        {
            var name = ModConfig.RandomizeNames ? PopRandom(random, namePool) : (girl.name, girl.girlNickName);
            assignedNames[girl] = name;

            if (ModConfig.RandomizeBaggage)
            {
                var baggageA = PopRandom(random, baggagePool);
                var baggageB = PopRandom(random, baggagePool);
                var baggageC = PopRandom(random, baggagePool);

                girl.baggageItemDefs = new List<ItemDefinition>(){
                baggageA.ailment,
                baggageB.ailment,
                baggageC.ailment
            };
                ReplaceCutsceneGirls(baggageA.ailment.cutsceneDefinition.steps, [(baggageA.girl, girl, assignedNames[girl])]);
                ReplaceCutsceneGirls(baggageB.ailment.cutsceneDefinition.steps, [(baggageB.girl, girl, assignedNames[girl])]);
                ReplaceCutsceneGirls(baggageC.ailment.cutsceneDefinition.steps, [(baggageC.girl, girl, assignedNames[girl])]);

                handledCutscenes.Add(baggageA.ailment.cutsceneDefinition);
                handledCutscenes.Add(baggageB.ailment.cutsceneDefinition);
                handledCutscenes.Add(baggageC.ailment.cutsceneDefinition);
            }

            if (ModConfig.RandomizeAffection)
            {
                var favoriteAffection = random.Next() % 4;
                girl.favoriteAffectionType = (PuzzleAffectionType)favoriteAffection;
                girl.leastFavoriteAffectionType = (PuzzleAffectionType)((favoriteAffection + 1 + (random.Next() % 3)) % 4);
            }
        }

        //apply pair randomization
        var finalPairsEnumerator = pairings.GetEnumerator();
        finalPairsEnumerator.MoveNext();

        foreach (var pair in normalPairs)
        {
            var flip = random.Next() % 2 == 0;

            var newGirlOne = normalGirls[flip ? finalPairsEnumerator.Current.a : finalPairsEnumerator.Current.b];
            var newGirlTwo = normalGirls[flip ? finalPairsEnumerator.Current.b : finalPairsEnumerator.Current.a];

            foreach (var cutscene in pair.relationshipCutsceneDefinitions)
            {
                ReplaceCutsceneGirls(cutscene?.steps, [
                    (pair.girlDefinitionOne, newGirlOne, assignedNames[newGirlOne]),
                    (pair.girlDefinitionTwo, newGirlTwo, assignedNames[newGirlTwo])
                ]);

                handledCutscenes.Add(cutscene);
            }

            pair.girlDefinitionOne = newGirlOne;
            pair.girlDefinitionTwo = newGirlTwo;
            finalPairsEnumerator.MoveNext();
        }

        //swap lola in tutorial pair
        var swapPool = normalGirls.ToList();
        var lolaDef = ModInterface.GameData.GetGirl(Girls.LolaId);
        normalGirls.Remove(lolaDef);
        var lolaSwap = PopRandom(random, swapPool);

        var tutorialPair = Game.Data.GirlPairs.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.GirlPair, new RelativeId(-1, 26)));

        tutorialPair.girlDefinitionTwo = lolaSwap;

        //for all other cutscenes lets just swap out each normal girl with another random one
        var replaceGroups = normalGirls.Select(x =>
        {
            var newGirl = PopRandom(random, swapPool);
            return (x, newGirl, assignedNames[newGirl]);
        }).Append((lolaDef, lolaSwap, assignedNames[lolaSwap])).ToArray();

        foreach (var cutscene in Game.Data.Cutscenes.GetAll().Where(x => !handledCutscenes.Contains(x)))
        {
            ReplaceCutsceneGirls(cutscene.steps, replaceGroups);
        }

        //names get changed after all cutscenes so that the name data is still matches
        foreach (var girl_names in assignedNames)
        {
            girl_names.Key.girlName = girl_names.Value.name;
            girl_names.Key.girlNickName = girl_names.Value.nickName;
        }
    }

    private T PopRandom<T>(Random random, List<T> list)
    {
        var index = random.Next() % list.Count;
        var result = list[index];
        list.RemoveAt(index);
        return result;
    }

    private void ReplaceCutsceneGirls(IEnumerable<CutsceneStepSubDefinition> steps,
        params IEnumerable<(GirlDefinition oldGirl, GirlDefinition newGirl, (string name, string nickName) newName)> replaceGroups)
    {
        if (steps == null)
        {
            return;
        }

        var strReplaces = replaceGroups.Select(x => (x.oldGirl.girlName, x.newName.name))
            .Concat(
                replaceGroups.Where(x => !x.oldGirl.girlNickName.IsNullOrWhiteSpace())
                    .Select(x => (x.oldGirl.girlNickName, x.newName.nickName.IsNullOrWhiteSpace()
                        ? x.newName.name
                        : x.newName.nickName))
            );

        foreach (var step in steps)
        {
            step.stringValue = step.stringValue.Replace(strReplaces);

            if (replaceGroups.TryGetFirst(x => step.girlDefinition == x.oldGirl, out var girlDef))
            {
                step.girlDefinition = girlDef.newGirl;
            }

            if (replaceGroups.TryGetFirst(x => step.targetGirlDefinition == x.oldGirl, out var targetGirlDef))
            {
                step.targetGirlDefinition = targetGirlDef.newGirl;
            }

            switch (step.stepType)
            {
                case CutsceneStepType.BRANCH:
                    foreach (var branch in step.branches)
                    {
                        ReplaceCutsceneGirls(branch?.cutsceneDefinition?.steps,
                            replaceGroups);
                        ReplaceCutsceneGirls(branch?.steps,
                            replaceGroups);
                    }
                    break;
                case CutsceneStepType.SUB_CUTSCENE:
                    CutsceneDefinition cutsceneDefinition = null;
                    switch (step.subCutsceneType)
                    {
                        case CutsceneStepSubCutsceneType.STRAIGHT:
                            cutsceneDefinition = step.subCutsceneDefinition;
                            break;
                        //These two cases are ignored because every pair-cutscene is already being cleaned
                        case CutsceneStepSubCutsceneType.GIRL_PAIR:
                            break;
                        case CutsceneStepSubCutsceneType.GIRL_PAIR_ROUND:
                            break;
                        //This case depends on an inner cutscene being passed in, which is not relevant here.
                        case CutsceneStepSubCutsceneType.INNER:
                            break;
                    }

                    if (cutsceneDefinition != null && cutsceneDefinition.steps.Count > 0)
                    {
                        ReplaceCutsceneGirls(cutsceneDefinition.steps, replaceGroups);
                    }
                    break;
                case CutsceneStepType.DIALOG_OPTIONS:
                    foreach (var option in step.dialogOptions)
                    {
                        option.dialogOptionText = option.dialogOptionText.Replace(strReplaces);
                        option.yuriDialogOptionText = option.yuriDialogOptionText.Replace(strReplaces);
                    }
                    ReplaceCutsceneGirls(step.dialogOptions.SelectMany(x => x.steps), replaceGroups);
                    break;
                case CutsceneStepType.SHOW_NOTIFICATION:
                    //nothing to do here, the string value is already corrected above
                    break;
            }
        }
    }
}
