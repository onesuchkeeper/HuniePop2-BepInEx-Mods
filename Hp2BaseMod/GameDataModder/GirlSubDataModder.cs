using System.Collections.Generic;
using System.Linq;
using Hp2BaseMod.Extension;
using Hp2BaseMod.Extension.IEnumerableExtension;
using Hp2BaseMod.GameDataInfo.Interface;

namespace Hp2BaseMod;

internal static class GirlSubDataModder
{
    /// <summary>
    /// by default each has one default (0), 12 for the normal girls, 1 for kyu, 2 for nymphojinn. 1+12+1+2=16
    /// </summary>
    private const int DEFAULT_DT_SET_COUNT = 16;

    public static void GatherSubMods(IEnumerable<IGirlDataMod> girlDataMods,
        out Dictionary<RelativeId, Dictionary<RelativeId, BodyData>> GirlToBodyToMods,
        out Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, List<IDialogLineDataMod>>>> dialogLineModsByIdByDialogTriggerByGirlId)
    {
        GirlToBodyToMods = new();
        dialogLineModsByIdByDialogTriggerByGirlId = new();

        foreach (var girlMod in girlDataMods)
        {
            var bodyDataDict = GirlToBodyToMods.GetOrNew(girlMod.Id);
            if (girlMod.GetBodyMods() is not null and var bodyMods)
            {
                foreach (var bodyMod in bodyMods)
                {
                    var bodyData = bodyDataDict.GetOrNew(bodyMod.Id);
                    bodyData.bodyMods.Add(bodyMod);
                    AddGirlSubMods(bodyMod.GetPartDataMods(), bodyData.partMods);
                    AddGirlSubMods(bodyMod.GetExpressions(), bodyData.expressionMods);
                    AddGirlSubMods(bodyMod.GetOutfits(), bodyData.outfitMods);
                    AddGirlSubMods(bodyMod.GetHairstyles(), bodyData.hairstyleMods);
                    AddGirlSubMods(bodyMod.GetSpecialPartMods(), bodyData.specialPartMods);

                    //parts have mirrors and alts, only 1 deep
                    var subParts = bodyData.partMods.SelectMany(x => x.Value).SelectMany(x => x.GetPartDataMods()).Where(x => x != null).ToArray();
                    if (subParts.Any())
                    {
                        foreach (var subMod in subParts)
                        {
                            bodyData.partMods.GetOrNew(subMod.Id).Add(subMod);
                        }
                    }
                }
            }

            var linesByDialogTriggerId = girlMod.GetLinesByDialogTriggerId();

            if (linesByDialogTriggerId != null)
            {
                if (!dialogLineModsByIdByDialogTriggerByGirlId.ContainsKey(girlMod.Id))
                {
                    dialogLineModsByIdByDialogTriggerByGirlId.Add(girlMod.Id, new());
                }

                foreach (var dtId_Lines in linesByDialogTriggerId)
                {
                    if (!dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id].ContainsKey(dtId_Lines.Item1))
                    {
                        dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id].Add(dtId_Lines.Item1, new());
                    }

                    foreach (var lineMod in dtId_Lines.Item2)
                    {
                        if (!dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtId_Lines.Item1].ContainsKey(lineMod.Id))
                        {
                            dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtId_Lines.Item1].Add(lineMod.Id, new());
                        }

                        dialogLineModsByIdByDialogTriggerByGirlId[girlMod.Id][dtId_Lines.Item1][lineMod.Id].Add(lineMod);
                    }
                }
            }
        }
    }

    private static void AddGirlSubMods<T>(IEnumerable<IBodySubDataMod<T>> subMods,
            Dictionary<RelativeId, List<IBodySubDataMod<T>>> subModListsById)
    {
        if (subMods != null && subMods.Any())
        {
            foreach (var subMod in subMods.Where(x => x != null))
            {
                var subModList = subModListsById.GetOrNew(subMod.Id);
                subModList.Add(subMod);
            }
        }
    }

    public static void HandleSubMods(Dictionary<int, GirlDefinition> girlDataDict,
        IEnumerable<IGirlDataMod> girlDataMods,
        Dictionary<RelativeId, Dictionary<RelativeId, BodyData>> GirlToBodyToMods,
        Dictionary<int, QuestionDefinition> questionDataDict,
        IEnumerable<IQuestionDataMod> questionDataMods,
        Dictionary<int, DialogTriggerDefinition> dialogTriggerDataDict,
        Dictionary<RelativeId, Dictionary<RelativeId, Dictionary<RelativeId, List<IDialogLineDataMod>>>> dialogLineModsByIdByDialogTriggerByGirlId)
    {
        using (ModInterface.Log.MakeIndent("Creating and registering data for new ids for sub mods"))
        {
            foreach (var girlId_BodyToMods in GirlToBodyToMods)
            {
                var girlDef = girlDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Girl, girlId_BodyToMods.Key)];
                var expansion = ExpandedGirlDefinition.Get(girlDef);

                using (ModInterface.Log.MakeIndent($"{girlId_BodyToMods.Key} {girlDef.girlName}"))
                {
                    using (ModInterface.Log.MakeIndent("Bodies"))
                    {
                        foreach (var id_bodyMods in girlId_BodyToMods.Value)
                        {
                            var body = expansion.Bodies.GetOrNew(id_bodyMods.Key);

                            using (ModInterface.Log.MakeIndent($"body {id_bodyMods.Key}, total bodies {expansion.Bodies.Count}"))
                            {
                                using (ModInterface.Log.MakeIndent("parts"))
                                {
                                    RegisterUnregisteredIds(body.PartIdToIndex,
                                        body.PartIndexToId,
                                        body.Parts.Count,
                                        id_bodyMods.Value.partMods.Select(x => x.Key).Distinct(),
                                        body.Parts);
                                }

                                using (ModInterface.Log.MakeIndent("expressions"))
                                {
                                    RegisterUnregisteredIds(expansion.ExpressionIdToIndex,
                                        expansion.ExpressionIndexToId,
                                        expansion.ExpressionIdToIndex.Count - 1,
                                        id_bodyMods.Value.expressionMods.Select(x => x.Key).Distinct(),
                                        body.Expressions);
                                }

                                using (ModInterface.Log.MakeIndent("outfits"))
                                {
                                    RegisterUnregisteredIds(expansion.OutfitIdToIndex,
                                        expansion.OutfitIndexToId,
                                        expansion.OutfitIdToIndex.Count - 1,
                                        id_bodyMods.Value.outfitMods.Select(x => x.Key).Distinct(),
                                        body.Outfits);
                                }

                                using (ModInterface.Log.MakeIndent("hairstyles"))
                                {
                                    RegisterUnregisteredIds(expansion.HairstyleIdToIndex,
                                        expansion.HairstyleIndexToId,
                                        expansion.HairstyleIdToIndex.Count - 1,
                                        id_bodyMods.Value.hairstyleMods.Select(x => x.Key).Distinct(),
                                        body.Hairstyles);
                                }

                                using (ModInterface.Log.MakeIndent("specials"))
                                {
                                    RegisterUnregisteredIds(body.SpecialPartIdToIndex,
                                        body.SpecialPartIndexToId,
                                        body.SpecialParts.Count,
                                        id_bodyMods.Value.specialPartMods.Select(x => x.Key).Distinct(),
                                        body.SpecialParts);
                                }
                            }
                        }
                    }
                }

                foreach (var id_body in expansion.Bodies)
                {
                    var bodyMods = girlId_BodyToMods.Value[id_body.Key];

                    AddMissingIndexedDefinitions(bodyMods.expressionMods.Keys.Select(x => expansion.ExpressionIdToIndex[x]), id_body.Value.Expressions, expansion.ExpressionIdToIndex.Count);
                    AddMissingIndexedDefinitions(bodyMods.hairstyleMods.Keys.Select(x => expansion.HairstyleIdToIndex[x]), id_body.Value.Hairstyles, expansion.HairstyleIdToIndex.Count);
                    AddMissingIndexedDefinitions(bodyMods.outfitMods.Keys.Select(x => expansion.OutfitIdToIndex[x]), id_body.Value.Outfits, expansion.OutfitIdToIndex.Count);
                }
            }

            using (ModInterface.Log.MakeIndent("dialog triggers"))
            {
                using (ModInterface.Log.MakeIndent("Girl dt indexes"))
                {
                    int nextIndex = DEFAULT_DT_SET_COUNT;
                    foreach (var girlId in girlDataMods.Select(x => x.Id).Distinct())
                    {
                        var girlExpansion = ExpandedGirlDefinition.Get(girlId);

                        if (girlExpansion.DialogTriggerIndex == -1)
                        {
                            girlExpansion.DialogTriggerIndex = nextIndex;
                            nextIndex++;
                        }
                    }
                }

                using (ModInterface.Log.MakeIndent("line sets"))
                {
                    foreach (var dt in dialogTriggerDataDict.Values.Where(x => IsGirlDialogTrigger(x)))
                    {
                        var expansion = dt.Expansion();

                        if (!expansion.GirlIdToLineIndexToLineId.ContainsKey(RelativeId.Default))
                        {
                            dt.dialogLineSets.Add(new DialogTriggerLineSet());

                            expansion.GirlIdToLineIdToLineIndex[RelativeId.Default] = new() { { RelativeId.Default, -1 } };
                            expansion.GirlIdToLineIndexToLineId[RelativeId.Default] = new() { { -1, RelativeId.Default } };
                        }

                        foreach (var girlRuntime in girlDataDict.Keys)
                        {
                            var girlId = ModInterface.Data.GetDataId(GameDataType.Girl, girlRuntime);

                            if (!expansion.GirlIdToLineIndexToLineId.ContainsKey(girlId))
                            {
                                expansion.GirlIdToLineIndexToLineId[girlId] = new();
                                expansion.GirlIdToLineIdToLineIndex[girlId] = new();
                                dt.dialogLineSets.Add(new DialogTriggerLineSet());
                            }
                        }
                    }
                }
            }

            using (ModInterface.Log.MakeIndent("girl lines"))
            {
                foreach (var girlId_DialogLineModsByIdByDialogTrigger in dialogLineModsByIdByDialogTriggerByGirlId)
                {
                    using (ModInterface.Log.MakeIndent($"{girlId_DialogLineModsByIdByDialogTrigger.Key}"))
                    {
                        var girlExpansion = ExpandedGirlDefinition.Get(girlId_DialogLineModsByIdByDialogTrigger.Key);

                        foreach (var dialogTriggerId_DialogLineModsById in girlId_DialogLineModsByIdByDialogTrigger.Value)
                        {
                            var dialogTrigger = ModInterface.GameData.GetDialogTrigger(dialogTriggerId_DialogLineModsById.Key);

                            using (ModInterface.Log.MakeIndent($"dt {dialogTriggerId_DialogLineModsById.Key} with {dialogTrigger.dialogLineSets.Count} line sets"))
                            {
                                var dialogTriggerSet = dialogTrigger.dialogLineSets[girlExpansion.DialogTriggerIndex];
                                var expansion = ExpandedDialogTriggerDefinition.Get(dialogTriggerId_DialogLineModsById.Key);

                                RegisterUnregisteredIds(expansion.GirlIdToLineIdToLineIndex[girlId_DialogLineModsByIdByDialogTrigger.Key],
                                    expansion.GirlIdToLineIndexToLineId[girlId_DialogLineModsByIdByDialogTrigger.Key],
                                    dialogTriggerSet.dialogLines.Count,
                                    dialogTriggerId_DialogLineModsById.Value.Select(x => x.Key),
                                    dialogTriggerSet.dialogLines);
                            }
                        }
                    }
                }
            }

            using (ModInterface.Log.MakeIndent("Favorites"))
            {
                var idToMods = new Dictionary<RelativeId, List<IQuestionDataMod>>();

                foreach (var mod in questionDataMods)
                {
                    idToMods.GetOrNew(mod.Id).Add(mod);
                }


                foreach (var id_mods in idToMods)
                {
                    using (ModInterface.Log.MakeIndent($"Question {id_mods.Key}"))
                    {
                        var question = questionDataDict[ModInterface.Data.GetRuntimeDataId(GameDataType.Question, id_mods.Key)];
                        var expansion = ExpandedQuestionDefinition.Get(id_mods.Key);
                        var answers = id_mods.Value.SelectManyNN(x => x.GetAnswerIds()).Distinct();

                        var index = question.questionAnswers.Count;
                        foreach (var id in answers.Where(x => !expansion.AnswerIdToIndex.ContainsKey(x)))
                        {
                            ModInterface.Log.LogInfo($"Adding answer {id} at index {index}");
                            expansion.AnswerIdToIndex[id] = index;
                            expansion.AnswerIndexToId[index++] = id;
                            question.questionAnswers.Add(null);
                        }
                    }
                }
            }
        }
    }

    // there are inconsistencies in how dialog triggers are handled, yay.
    // Some are indexed by girl id, others are made specifically for the hub, and there's no easy way to differentiate them
    // so here's just a manual list, maybe add this to default data later. If someone wants to mod these they can make a dll I'm not gonna support it via json
    // greeting = 49
    // valediction = 50
    // nap = 51
    // wakeup = 52
    // wing check = 53
    // pre nymph = 54
    // pos nymph = 55
    // nonstop = 56
    // wardrobe = 57
    // album = 58
    private static bool IsGirlDialogTrigger(DialogTriggerDefinition dt) => dt.id < 49 || dt.id > 58;

    private static void RegisterUnregisteredIds<T>(IDictionary<RelativeId, int> idToIndex,
        IDictionary<int, RelativeId> indexToId,
        int startingIndex,
        IEnumerable<RelativeId> ids,
        List<T> gameData)
        where T : class, new()
    {
        foreach (var id in ids.Where(x => !idToIndex.ContainsKey(x)))
        {
            idToIndex[id] = startingIndex;
            indexToId[startingIndex++] = id;
            var newData = new T();
            gameData.Add(newData);
        }
    }

    private static void AddMissingIndexedDefinitions<T>(IEnumerable<int> indexes, List<T> definitions, int total)
        where T : class, new()
    {
        foreach (var index in indexes)
        {
            if (index > definitions.Count - 1)
            {
                for (int i = index - definitions.Count; i > 0; i--)
                {
                    definitions.Add(null);
                }

                definitions.Add(new());
            }
            else
            {
                definitions[index] = new();
            }
        }

        while (definitions.Count < total)
        {
            definitions.Add(null);
        }
    }
}