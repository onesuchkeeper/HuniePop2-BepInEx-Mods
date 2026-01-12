using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hp2BaseMod;
using Hp2BaseMod.Extension;

namespace Hp2Randomizer;

public static class RandomizeUtil
{
    private static readonly FieldInfo f_hairstyleLookup = AccessTools.Field(typeof(ExpandedGirlDefinition), "_hairstyleLookup");
    private static readonly FieldInfo f_outfitLookup = AccessTools.Field(typeof(ExpandedGirlDefinition), "_outfitLookup");
    private static readonly FieldInfo f_expressionLookup = AccessTools.Field(typeof(ExpandedGirlDefinition), "_expressionLookup");
    private static readonly FieldInfo f_herQuestionIdToIndex = AccessTools.Field(typeof(ExpandedGirlDefinition), "_herQuestionIdToIndex");
    private static readonly FieldInfo f_herQuestionGoodResponseIdToIndex = AccessTools.Field(typeof(ExpandedGirlDefinition), "_herQuestionGoodResponseIdToIndex");
    private static readonly FieldInfo f_herQuestionBadResponseIdToIndex = AccessTools.Field(typeof(ExpandedGirlDefinition), "_herQuestionBadResponseIdToIndex");

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
        if (Plugin.Disable.Value) return;

        var seed = Plugin.Seed.Value;
        if (seed == -1)
        {
            Plugin.Seed.Value = seed = Environment.TickCount;
        }

        var swapHandlerDict = swapHandlers.ToDictionary(x => x.Item1, x => x.Item2);

        ModInterface.Log.Message($"Randomizing, seed:{seed}");
        var random = new Random(seed);

        var normalGirls = Game.Data.Girls.GetAllBySpecial(false);

        normalGirls.Remove(ModInterface.GameData.GetGirl(Girls.Kyu));
        normalGirls.Remove(ModInterface.GameData.GetGirl(Girls.Moxie));
        normalGirls.Remove(ModInterface.GameData.GetGirl(Girls.Jewn));

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
        if (!Plugin.RandomizePairs.Value)
        {
            pairings = normalPairs.Select(x => (x.girlDefinitionOne.id, x.girlDefinitionTwo.id));
        }
        else if (UndirectedGraph.TryMakeConnectedHHGraph(degrees, out var graph))
        {
            pairings = graph.Randomize(degreeTotal * 3, random).Edges;
        }
        else
        {
            ModInterface.Log.Warning($"Failed to create simple graph from pairings, they will not be randomized.");
            pairings = normalPairs.Select(x => (x.girlDefinitionOne.id, x.girlDefinitionTwo.id));
        }

        //gather data pools
        var namePool = new List<(string name, string nickName)>()
        {
            // hp
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

            // hcs
            ("Marlena", null),
            ("Nadia", null),
            ("Renee", null),
            
            // The Named
            ("Lily", null),         // Half Baked
            ("Schmendolyn", null),  // Mallot
            //opted for no name     // Tobias

            // Tele
            ("Nila", null),
            ("Zoe", null),
            ("Amina", null),
            ("Chloe", null),

            // Others
            ("Zone", null),
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

        var randomizedNames = Plugin.RandomizeNames.Value;

        var canSwapSingleDate = ModInterface.TryGetInterModValue("OSK.BepInEx.SingleDate", "SwapGirls", out Action<RelativeId, RelativeId> m_SwapGirls);

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

            if (canSwapSingleDate)
            {
                m_SwapGirls(specialId, targetId);
            }

            id_handler.Value.Invoke(specialGirl, targetGirl);
        }

        var handledCutscenes = new HashSet<CutsceneDefinition>();

        var randomizedBaggage = Plugin.RandomizeBaggage.Value;
        var randomizedAffection = Plugin.RandomizeAffection.Value;

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
        var lolaDef = ModInterface.GameData.GetGirl(Girls.Lola);
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

    public static void SwapNymphojinn(GirlDefinition nymphojinnDef, GirlDefinition otherGirlDef, bool keepSwappedWings)
    {
        var nymphojinnId = ModInterface.Data.GetDataId(GameDataType.Girl, nymphojinnDef.id);
        var otherId = ModInterface.Data.GetDataId(GameDataType.Girl, otherGirlDef.id);

        var nymphoExpansion = ExpandedGirlDefinition.Get(nymphojinnId);
        var otherGirlExpansion = ExpandedGirlDefinition.Get(otherId);

        // find the neutral glowing eyes to use for expressions that don't have glowing eyes
        foreach (var body in otherGirlExpansion.Bodies.Values)
        {
            if (keepSwappedWings && body.SpecialEffectPrefab == null)
            {
                body.SpecialEffectPrefab = nymphojinnDef.specialEffectPrefab;
            }

            var defaultGlowEyesIndex = -1;

            if (body.Expressions.TryGetFirst(x => x.expressionType == GirlExpressionType.NEUTRAL, out var neutralExpression))
            {
                defaultGlowEyesIndex = neutralExpression.partIndexEyesGlow;
            }
            else
            {
                ModInterface.Log.Warning($"Unable to find neutral expression for girl {otherGirlDef.girlName}]s body {body.BodyName}");
            }

            foreach (var expression in body.Expressions)
            {
                expression.partIndexEyes = expression.partIndexEyesGlow == -1
                    ? defaultGlowEyesIndex
                    : expression.partIndexEyesGlow;
            }
        }

        foreach (var expression in otherGirlDef.expressions)
        {
            if (expression.partIndexEyesGlow != -1)
            {
                expression.partIndexEyes = expression.partIndexEyesGlow;
            }
        }

        nymphojinnDef.defaultHairstyleIndex = 0;
        nymphojinnDef.defaultOutfitIndex = 0;

        HandleSpecialCharDtSwap(ExpandedGirlDefinition.DialogTriggerIndexes[nymphojinnId], ExpandedGirlDefinition.DialogTriggerIndexes[otherId]);
        HandleSpecialCharFavQuestionSwap(nymphoExpansion, otherGirlExpansion);
        HandleSpecialCharUiSwap(nymphojinnDef, nymphoExpansion, otherGirlDef, otherGirlExpansion);
    }

    public static void SwapKyu(GirlDefinition kyuDef, GirlDefinition otherGirlDef, bool keepSwappedWings)
    {
        var kyuId = ModInterface.Data.GetDataId(GameDataType.Girl, kyuDef.id);
        var otherId = ModInterface.Data.GetDataId(GameDataType.Girl, otherGirlDef.id);

        kyuDef.defaultHairstyleIndex = 1;
        kyuDef.defaultOutfitIndex = 1;

        if (keepSwappedWings && otherGirlDef.specialEffectPrefab == null)
        {
            foreach (var body in otherGirlDef.Expansion().Bodies.Values)
            {
                body.SpecialEffectPrefab = kyuDef.specialEffectPrefab;
            }
        }

        var kyuExp = ExpandedGirlDefinition.Get(kyuId);
        var otherGirlExp = ExpandedGirlDefinition.Get(otherId);

        HandleSpecialCharDtSwap(ExpandedGirlDefinition.DialogTriggerIndexes[kyuId], ExpandedGirlDefinition.DialogTriggerIndexes[otherId]);
        HandleSpecialCharFavQuestionSwap(kyuExp, otherGirlExp);
        HandleSpecialCharUiSwap(kyuDef, kyuExp, otherGirlDef, otherGirlExp);
    }

    public static void HandleSpecialCharUiSwap(
        GirlDefinition special,
        ExpandedGirlDefinition specialExp,
        GirlDefinition other,
        ExpandedGirlDefinition otherExp)
    {
        SwapOrCopyRef(ref specialExp.Bodies, ref otherExp.Bodies);
        SwapOrCopyRef(ref specialExp.OutfitLookup, ref otherExp.OutfitLookup);
        SwapOrCopyRef(ref specialExp.HairstyleLookup, ref otherExp.HairstyleLookup);

        SwapOrCopyRef(ref special.cellphoneHead, ref other.cellphoneHead);
        SwapOrCopyRef(ref special.cellphoneHeadAlt, ref other.cellphoneHeadAlt);
        SwapOrCopyRef(ref special.cellphoneMiniHead, ref other.cellphoneMiniHead);
        SwapOrCopyRef(ref special.cellphoneMiniHeadAlt, ref other.cellphoneMiniHeadAlt);
        SwapOrCopyRef(ref special.cellphonePortrait, ref other.cellphonePortrait);
        SwapOrCopyRef(ref special.cellphonePortraitAlt, ref other.cellphonePortraitAlt);
        SwapVal(ref special.hasAltStyles, ref other.hasAltStyles);
        SwapOrCopyRef(ref special.altStylesFlagName, ref other.altStylesFlagName);
        SwapOrCopyRef(ref special.altStylesCodeDefinition, ref other.altStylesCodeDefinition);
        SwapOrCopyRef(ref special.unlockStyleCodeDefinition, ref other.unlockStyleCodeDefinition);
    }

    // copy the other girls favorites if special doesn't have their own
    public static void HandleSpecialCharFavQuestionSwap(ExpandedGirlDefinition specialExp, ExpandedGirlDefinition otherExp)
    {
        // if the special doesn't have favorites
        if (!(specialExp.FavQuestionIdToAnswerId?.Any() ?? false))
        {
            // copy the other girl's
            specialExp.FavQuestionIdToAnswerId = otherExp.FavQuestionIdToAnswerId;
        }
    }

    public static void HandleSpecialCharHerQuestionSwap(ExpandedGirlDefinition specialExp, ExpandedGirlDefinition otherExp)
    {
        if (!(specialExp.HerQuestionIdToIndex?.Ids.Any() ?? false))
        {
            f_herQuestionIdToIndex.SetValue(specialExp, otherExp.HerQuestionIdToIndex);
            f_herQuestionGoodResponseIdToIndex.SetValue(specialExp, otherExp.HerQuestionGoodResponseIdToDtIndex);
            f_herQuestionBadResponseIdToIndex.SetValue(specialExp, otherExp.HerQuestionBadResponseIdToDtIndex);
        }
    }

    /// <summary>
    /// Swaps lines between characters. If one character lacks a line, copies it
    /// from the other instead.
    /// </summary>
    public static void HandleSpecialCharDtSwap(int specialDtIndex, int otherDtIndex)
    {
        foreach (var dt in Game.Data.DialogTriggers.GetAll())
        {
            var specialLines = dt.dialogLineSets.GetOrNew(specialDtIndex).dialogLines;
            var otherLines = dt.dialogLineSets.GetOrNew(otherDtIndex).dialogLines;

            // pad
            var maxCount = Math.Max(specialLines.Count, otherLines.Count);
            while (specialLines.Count < maxCount) specialLines.Add(null);
            while (otherLines.Count < maxCount) otherLines.Add(null);

            // handle lines
            for (var i = 0; i < maxCount; i++)
            {
                var special = specialLines[i];
                var other = otherLines[i];

                if (special != null && other != null)
                {
                    // swap
                    specialLines[i] = other;
                    otherLines[i] = special;
                }
                else if (special != null && other == null)
                {
                    // copy special to other
                    otherLines[i] = special;
                }
                else if (special == null && other != null)
                {
                    // copy other to special
                    specialLines[i] = other;
                }
            }
        }
    }

    private static void SwapVal<T>(ref T a, ref T b)
        where T : struct
    {
        var hold = a;
        a = b;
        b = hold;
    }

    private static void SwapOrCopyRef<T>(ref T a, ref T b)
        where T : class
    {
        var hold = a;
        if (b != null) a = b;
        if (hold != null) b = hold;
    }

    private static void SwapOrCopyField(FieldInfo fieldInfo, object a, object b)
    {
        var aValue = fieldInfo.GetValue(a);
        var bValue = fieldInfo.GetValue(b);

        if (aValue != null) fieldInfo.SetValue(b, aValue);
        if (bValue != null) fieldInfo.SetValue(a, bValue);
    }
}
