using System;
using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.Extension.StringExtension;

namespace Hp2Randomizer;

public static class RandomizeUtil
{
    /// <summary>
    /// if randomization has made a girl's saved outfit invalid, sets it to the default
    /// </summary>
    public static void CleanGirlStyles(PlayerFile file)
    {
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

    /// <summary>
    /// Randomizes game data
    /// </summary>
    /// <param name="swapHandlerDict"></param>
    public static void Randomize(IEnumerable<(RelativeId, Action<GirlDefinition, GirlDefinition>)> swapHandlers)
    {
        if (Plugin.Disable) return;

        var seed = Plugin.Seed;
        if (seed == -1)
        {
            Plugin.Seed = seed = Environment.TickCount;
        }

        var swapHandlerDict = swapHandlers.ToDictionary(x => x.Item1, x => x.Item2);

        var keepSwappedWings = Plugin.SwappedSpecialKeepWings;

        if (Plugin.IncludeKyu)
        {
            swapHandlerDict.Add(Girls.KyuId, (a, b) => SwapKyu(a, b, keepSwappedWings));
        }

        if (Plugin.IncludeNymphojinn)
        {
            swapHandlerDict.Add(Girls.MoxieId, (a, b) => SwapNymphojinn(a, b, keepSwappedWings));
            swapHandlerDict.Add(Girls.JewnId, (a, b) => SwapNymphojinn(a, b, keepSwappedWings));
        }

        ModInterface.Log.LogInfo($"Randomizing, seed:{seed}");
        var random = new Random(seed);

        var normalGirls = Game.Data.Girls.GetAllBySpecial(false);
        var normalPairs = Game.Data.GirlPairs.GetAll().Where(x =>
            x.girlDefinitionOne != null
            && x.girlDefinitionTwo != null
            && !x.girlDefinitionOne.specialCharacter
            && !x.girlDefinitionTwo.specialCharacter)
            .ToArray();

        // determine pairs
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
        if (!Plugin.RandomizePairs)
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
            
            //The Named
            ("Lily", null),//Half Baked
            ("Schmendolyn", null),//Mallot
            //opted for no name
            
        };

        var assignedNames = new Dictionary<GirlDefinition, (string name, string nickName)>();

        var baggagePool = new List<(GirlDefinition girl, ItemDefinition ailment)>();

        foreach (var girl in normalGirls)
        {
            namePool.Add((girl.girlName, girl.girlNickName));
            baggagePool.AddRange(girl.baggageItemDefs.Select(x => (girl, x)));

            //add nulls to fill in missing baggage, later if a null is popped ignore it
            if (girl.baggageItemDefs.Count < 3)
            {
                baggagePool.AddRange(Enumerable.Repeat((girl, (ItemDefinition)null), 3 - girl.baggageItemDefs.Count));
            }
        }

        var randomizedNames = Plugin.RandomizeNames;

        var canSwapCharms = ModInterface.TryGetInterModValue("OSK.BepInEx.SingleDate", "SwapCharms", out Action<RelativeId, RelativeId> m_swapCharms);

        //special characters don't have all the stuff they need,
        //so instead I'll just swap their visuals and other bits with someone
        //swaps like this aren't the same odds but I don't really care
        foreach (var id_handler in swapHandlerDict)
        {
            var specialGirl = ModInterface.GameData.GetGirl(id_handler.Key);
            namePool.Add((specialGirl.girlName, specialGirl.girlNickName));
        }

        // remove dupes
        namePool = namePool.DistinctBy(x => x.name).ToList();

        foreach (var id_handler in swapHandlerDict)
        {
            var specialGirl = ModInterface.GameData.GetGirl(id_handler.Key);
            var targetGirl = normalGirls[random.Next() % normalGirls.Count];

            namePool.Add((specialGirl.girlName, specialGirl.girlNickName));

            assignedNames[specialGirl] = randomizedNames
                ? namePool.PopRandom(random)
                : (specialGirl.girlName, specialGirl.girlNickName);

            var specialId = ModInterface.Data.GetDataId(GameDataType.Girl, specialGirl.id);
            var targetId = ModInterface.Data.GetDataId(GameDataType.Girl, targetGirl.id);

            if (canSwapCharms)
            {
                m_swapCharms(specialId, targetId);
            }

            id_handler.Value.Invoke(specialGirl, targetGirl);

            var specialGirlExpanded = specialGirl.Expansion();
            var targetGirlExpanded = targetGirl.Expansion();

            var holdIdToIndex = specialGirlExpanded.OutfitIdToIndex;
            specialGirlExpanded.OutfitIdToIndex = targetGirlExpanded.OutfitIdToIndex;
            targetGirlExpanded.OutfitIdToIndex = holdIdToIndex;

            holdIdToIndex = specialGirlExpanded.HairstyleIdToIndex;
            specialGirlExpanded.HairstyleIdToIndex = targetGirlExpanded.HairstyleIdToIndex;
            targetGirlExpanded.HairstyleIdToIndex = holdIdToIndex;

            holdIdToIndex = specialGirlExpanded.ExpressionIdToIndex;
            specialGirlExpanded.ExpressionIdToIndex = targetGirlExpanded.ExpressionIdToIndex;
            targetGirlExpanded.ExpressionIdToIndex = holdIdToIndex;

            var holdIndexToId = specialGirlExpanded.OutfitIndexToId;
            specialGirlExpanded.OutfitIndexToId = targetGirlExpanded.OutfitIndexToId;
            targetGirlExpanded.OutfitIndexToId = holdIndexToId;

            holdIndexToId = specialGirlExpanded.HairstyleIndexToId;
            specialGirlExpanded.HairstyleIndexToId = targetGirlExpanded.HairstyleIndexToId;
            targetGirlExpanded.HairstyleIndexToId = holdIndexToId;

            holdIndexToId = specialGirlExpanded.ExpressionIndexToId;
            specialGirlExpanded.ExpressionIndexToId = targetGirlExpanded.ExpressionIndexToId;
            targetGirlExpanded.ExpressionIndexToId = holdIndexToId;

            var holdBodies = specialGirlExpanded.Bodies;
            specialGirlExpanded.Bodies = targetGirlExpanded.Bodies;
            targetGirlExpanded.Bodies = holdBodies;

            var holdParts = specialGirl.parts;
            specialGirl.parts = targetGirl.parts;
            targetGirl.parts = holdParts;

            var holdSpecialParts = specialGirl.specialParts;
            specialGirl.specialParts = targetGirl.specialParts;
            targetGirl.specialParts = holdSpecialParts;

            var holdInt = specialGirl.partIndexBlink;
            specialGirl.partIndexBlink = targetGirl.partIndexBlink;
            targetGirl.partIndexBlink = holdInt;

            holdInt = specialGirl.partIndexBlushHeavy;
            specialGirl.partIndexBlushHeavy = targetGirl.partIndexBlushHeavy;
            targetGirl.partIndexBlushHeavy = holdInt;

            holdInt = specialGirl.partIndexBlushLight;
            specialGirl.partIndexBlushLight = targetGirl.partIndexBlushLight;
            targetGirl.partIndexBlushLight = holdInt;

            holdInt = specialGirl.partIndexBody;
            specialGirl.partIndexBody = targetGirl.partIndexBody;
            targetGirl.partIndexBody = holdInt;

            var holdIndexList = specialGirl.partIndexesPhonemes;
            specialGirl.partIndexesPhonemes = targetGirl.partIndexesPhonemes;
            targetGirl.partIndexesPhonemes = holdIndexList;

            holdIndexList = specialGirl.partIndexesPhonemesTeeth;
            specialGirl.partIndexesPhonemesTeeth = targetGirl.partIndexesPhonemesTeeth;
            targetGirl.partIndexesPhonemesTeeth = holdIndexList;

            holdInt = specialGirl.partIndexMouthNeutral;
            specialGirl.partIndexMouthNeutral = targetGirl.partIndexMouthNeutral;
            targetGirl.partIndexMouthNeutral = holdInt;

            holdInt = specialGirl.partIndexNipples;
            specialGirl.partIndexNipples = targetGirl.partIndexNipples;
            targetGirl.partIndexNipples = holdInt;

            holdInt = specialGirl.defaultOutfitIndex;
            specialGirl.defaultOutfitIndex = targetGirl.defaultOutfitIndex;
            targetGirl.defaultOutfitIndex = holdInt;

            holdInt = specialGirl.defaultHairstyleIndex;
            specialGirl.defaultHairstyleIndex = targetGirl.defaultHairstyleIndex;
            targetGirl.defaultHairstyleIndex = holdInt;

            var holdOutfits = specialGirl.outfits;
            specialGirl.outfits = targetGirl.outfits;
            targetGirl.outfits = holdOutfits;

            var holdHair = specialGirl.hairstyles;
            specialGirl.hairstyles = targetGirl.hairstyles;
            targetGirl.hairstyles = holdHair;

            holdInt = specialGirl.defaultExpressionIndex;
            specialGirl.defaultExpressionIndex = targetGirl.defaultExpressionIndex;
            targetGirl.defaultExpressionIndex = holdInt;

            var holdExpressions = specialGirl.expressions;
            specialGirl.expressions = targetGirl.expressions;
            targetGirl.expressions = holdExpressions;

            var holdSpecialEffectPrefab = specialGirl.specialEffectPrefab;
            specialGirl.specialEffectPrefab = targetGirl.specialEffectPrefab;
            targetGirl.specialEffectPrefab = holdSpecialEffectPrefab;

            var holdSprite = specialGirl.cellphoneHead;
            specialGirl.cellphoneHead = targetGirl.cellphoneHead;
            targetGirl.cellphoneHead = holdSprite;

            holdSprite = specialGirl.cellphoneHeadAlt;
            specialGirl.cellphoneHeadAlt = targetGirl.cellphoneHeadAlt;
            targetGirl.cellphoneHeadAlt = holdSprite;

            holdSprite = specialGirl.cellphoneMiniHead;
            specialGirl.cellphoneMiniHead = targetGirl.cellphoneMiniHead;
            targetGirl.cellphoneMiniHead = holdSprite;

            holdSprite = specialGirl.cellphoneMiniHeadAlt;
            specialGirl.cellphoneMiniHeadAlt = targetGirl.cellphoneMiniHeadAlt;
            targetGirl.cellphoneMiniHeadAlt = holdSprite;

            holdSprite = specialGirl.cellphonePortrait;
            specialGirl.cellphonePortrait = targetGirl.cellphonePortrait;
            targetGirl.cellphonePortrait = holdSprite;

            holdSprite = specialGirl.cellphonePortraitAlt;
            specialGirl.cellphonePortraitAlt = targetGirl.cellphonePortraitAlt;
            targetGirl.cellphonePortraitAlt = holdSprite;

            var holdBool = specialGirl.hasAltStyles;
            specialGirl.hasAltStyles = targetGirl.hasAltStyles;
            targetGirl.hasAltStyles = holdBool;

            var holdString = specialGirl.altStylesFlagName;
            specialGirl.altStylesFlagName = targetGirl.altStylesFlagName;
            targetGirl.altStylesFlagName = holdString;

            var holdCode = specialGirl.altStylesCodeDefinition;
            specialGirl.altStylesCodeDefinition = targetGirl.altStylesCodeDefinition;
            targetGirl.altStylesCodeDefinition = holdCode;

            holdCode = specialGirl.unlockStyleCodeDefinition;
            specialGirl.unlockStyleCodeDefinition = targetGirl.unlockStyleCodeDefinition;
            targetGirl.unlockStyleCodeDefinition = holdCode;

            holdInt = specialGirl.failureExpressionIndex;
            specialGirl.failureExpressionIndex = targetGirl.failureExpressionIndex;
            targetGirl.failureExpressionIndex = holdInt;
        }

        var handledCutscenes = new HashSet<CutsceneDefinition>();

        var randomizedBaggage = Plugin.RandomizeBaggage;
        var randomizedAffection = Plugin.RandomizeAffection;

        // randomize girls
        foreach (var girl in normalGirls)
        {
            var name = randomizedNames
                ? namePool.PopRandom(random)
                : (girl.name, girl.girlNickName);

            assignedNames[girl] = name;

            if (randomizedBaggage)
            {
                girl.baggageItemDefs = new();

                void HandleBaggage((GirlDefinition girl, ItemDefinition ailment) baggage)
                {
                    if (baggage.ailment != null)
                    {
                        girl.baggageItemDefs.Add(baggage.ailment);
                        ReplaceCutsceneGirls(baggage.ailment.cutsceneDefinition.steps, [(baggage.girl, girl, assignedNames[girl])]);
                        handledCutscenes.Add(baggage.ailment.cutsceneDefinition);
                    }
                }

                HandleBaggage(baggagePool.PopRandom(random));
                HandleBaggage(baggagePool.PopRandom(random));
                HandleBaggage(baggagePool.PopRandom(random));
            }

            if (randomizedAffection)
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
        var lolaSwap = swapPool.PopRandom(random);

        var tutorialPair = Game.Data.GirlPairs.Get(ModInterface.Data.GetRuntimeDataId(GameDataType.GirlPair, new RelativeId(-1, 26)));

        tutorialPair.girlDefinitionTwo = lolaSwap;

        //for all other cutscenes lets just swap out each normal girl with another random one
        var replaceGroups = normalGirls.Select(x =>
        {
            var newGirl = swapPool.PopRandom(random);
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

    private static void ReplaceCutsceneGirls(IEnumerable<CutsceneStepSubDefinition> steps,
        params IEnumerable<(GirlDefinition oldGirl, GirlDefinition newGirl, (string name, string nickName) newName)> replaceGroups)
    {
        if (steps == null)
        {
            return;
        }

        var strReplaces = replaceGroups.Select(x => (x.oldGirl.girlName, x.newName.name))
            .Concat(
                replaceGroups.Where(x => !string.IsNullOrWhiteSpace(x.oldGirl.girlNickName))
                    .Select(x => (x.oldGirl.girlNickName,
                                    string.IsNullOrWhiteSpace(x.newName.nickName)
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

    private static void SwapNymphojinn(GirlDefinition nymphojinnDef, GirlDefinition otherGirlDef, bool keepSwappedWings)
    {
        //find the neutral glowing eyes to use for expressions that don't have glowing eyes
        var defaultGlowEyesIndex = -1;
        if (otherGirlDef.expressions.TryGetFirst(x => x.expressionType == GirlExpressionType.NEUTRAL, out var neutralExpression))
        {
            defaultGlowEyesIndex = neutralExpression.partIndexEyesGlow;
        }
        else
        {
            ModInterface.Log.LogWarning($"Unable to find neutral expression for girl {otherGirlDef.girlName}");
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

        var nymphoExpansion = nymphojinnDef.Expansion();
        var otherGirlExpansion = otherGirlDef.Expansion();

        foreach (var dt in Game.Data.DialogTriggers.GetAll())
        {
            var kyuLines = dt.dialogLineSets[nymphoExpansion.DialogTriggerIndex];
            if (kyuLines.dialogLines.Count != 0)
            {
                dt.dialogLineSets[nymphoExpansion.DialogTriggerIndex] = dt.dialogLineSets[otherGirlExpansion.DialogTriggerIndex];
                dt.dialogLineSets[otherGirlExpansion.DialogTriggerIndex] = kyuLines;
            }
        }

        nymphojinnDef.defaultHairstyleIndex = 0;
        nymphojinnDef.defaultOutfitIndex = 0;

        if (keepSwappedWings && otherGirlDef.specialEffectPrefab == null)
        {
            otherGirlDef.specialEffectPrefab = nymphojinnDef.specialEffectPrefab;
        }
    }

    private static void SwapKyu(GirlDefinition kyuDef, GirlDefinition otherGirlDef, bool keepSwappedWings)
    {
        kyuDef.defaultHairstyleIndex = 1;
        kyuDef.defaultOutfitIndex = 1;

        if (keepSwappedWings && otherGirlDef.specialEffectPrefab == null)
        {
            otherGirlDef.specialEffectPrefab = kyuDef.specialEffectPrefab;
        }

        HandleSpecialCharDtSwap(kyuDef.Expansion(), otherGirlDef.Expansion());
    }

    private static void HandleSpecialCharDtSwap(ExpandedGirlDefinition special, ExpandedGirlDefinition other)
    {
        foreach (var dt in Game.Data.DialogTriggers.GetAll())
        {
            var kyuLines = dt.dialogLineSets[special.DialogTriggerIndex];
            if (kyuLines.dialogLines.Count != 0)
            {
                dt.dialogLineSets[special.DialogTriggerIndex] = dt.dialogLineSets[other.DialogTriggerIndex];
                dt.dialogLineSets[other.DialogTriggerIndex] = kyuLines;
            }
        }
    }
}
